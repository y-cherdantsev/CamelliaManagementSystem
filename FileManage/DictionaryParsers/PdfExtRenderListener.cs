using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CamelliaManagementSystem.FileManage.DictionaryParsers.Objects;
using iTextSharp.text.pdf.parser;
using Path = iTextSharp.text.pdf.parser.Path;

namespace CamelliaManagementSystem.FileManage.DictionaryParsers
{
    public sealed class PdfExtRenderListener : IExtRenderListener, ITextExtractionStrategy
    {
        public const double VerticalThreshold = 320.0;

        public const double UniDirectionalThreshold = 2.0;

        private static readonly double BiDirectionalThreshold = Math.Sqrt(Math.Pow(UniDirectionalThreshold, 2) * 2);

        private readonly int _currentPage;

        private ITextExtractionStrategy strategy { get; }

        private readonly List<PathConstructionRenderInfo> _pathInfos = new List<PathConstructionRenderInfo>();

        /// <summary>
        /// Gets or sets SortedSet of all coordinates for the drawn lines.
        /// </summary>
        private readonly SortedSet<PdfCoordinate> _allCoordinates = new SortedSet<PdfCoordinate>();

        /// <summary>
        /// Gets or sets SortedSet of Vertical Lines. Adds line on each Move -> Line To, if is vertical.
        /// </summary>
        private readonly SortedSet<VerticalLineFromTop> _allVerticalLinesFromTop =
            new SortedSet<VerticalLineFromTop>();

        internal readonly SortedSet<PdfTextField> AllTextFieldsSortedSet = new SortedSet<PdfTextField>();

        internal readonly SortedDictionary<PdfCoordinate, List<VerticalLineFromTop>>
            AllAvailableTablesKeyMiddleCoordinate =
                new SortedDictionary<PdfCoordinate, List<VerticalLineFromTop>>();

        public PdfExtRenderListener(ITextExtractionStrategy strategy, int currentPage)
        {
            this.strategy = strategy;
            _currentPage = currentPage;
        }

        public void BeginTextBlock()
        {
            strategy.BeginTextBlock();
        }

        public void RenderText(TextRenderInfo renderInfo)
        {
            var bottomLeft = renderInfo.GetDescentLine().GetStartPoint();
            var topRight = renderInfo.GetAscentLine().GetEndPoint();

            var newTextField = new PdfTextField(
                new PdfCoordinate(bottomLeft[1], bottomLeft[0]),
                Math.Abs(topRight[1] - bottomLeft[1]),
                Math.Abs(bottomLeft[0] - topRight[0]),
                renderInfo.GetText(),
                _currentPage);

            AllTextFieldsSortedSet.Add(newTextField);
            strategy.RenderText(renderInfo);
        }

        public void EndTextBlock()
        {
            strategy.EndTextBlock();
        }

        public void RenderImage(ImageRenderInfo renderInfo)
        {
            strategy.RenderImage(renderInfo);
        }

        public string GetResultantText() => this.strategy.GetResultantText();

        public void ModifyPath(PathConstructionRenderInfo renderInfo)
        {
            _pathInfos.Add(renderInfo);
        }

        private PdfCoordinate GetClosestToCoordinate(PdfCoordinate coordinate)
        {
            var existsClosest = CheckExistsClosest(coordinate);
            if (!existsClosest)
            {
                throw new Exception("No closest coordinate exists");
            }

            return _allCoordinates.First(o => CalculateClosest(coordinate, o) <= BiDirectionalThreshold);
        }

        public Path RenderPath(PathPaintingRenderInfo renderInfo)
        {
            PdfCoordinate currentMoveTo = null;
            foreach (var pathConstructionRenderInfo in _pathInfos)
            {
                PdfCoordinate coordinate;
                switch (pathConstructionRenderInfo.Operation)
                {
                    case PathConstructionRenderInfo.MOVETO:
                        /*Console.Write($"MoveTo \n");*/
                        if (pathConstructionRenderInfo.SegmentData.Count > 2)
                        {
                            throw new InvalidDataException();
                        }

                        coordinate = new PdfCoordinate(pathConstructionRenderInfo.SegmentData[0],
                            pathConstructionRenderInfo.SegmentData[1]);
                        AddCoordinateIfNotExistsClose(coordinate);

                        currentMoveTo = GetClosestToCoordinate(coordinate);
                        break;
                    case PathConstructionRenderInfo.LINETO:
                        /*Console.Write($"LineTo \n");*/

                        if (pathConstructionRenderInfo.SegmentData.Count > 2)
                        {
                            throw new InvalidDataException();
                        }

                        coordinate = new PdfCoordinate(pathConstructionRenderInfo.SegmentData[0],
                            pathConstructionRenderInfo.SegmentData[1]);
                        AddCoordinateIfNotExistsClose(coordinate);

                        if (currentMoveTo == null)
                        {
                            throw new Exception("No MoveTo before LineTo");
                        }
                        else
                        {
                            var next = GetClosestToCoordinate(coordinate);
                            TryUpdateVerticalLinesDict(currentMoveTo, next);

                            currentMoveTo = next;
                        }

                        break;
                    case PathConstructionRenderInfo.RECT:
                        /*Console.Write($"Rectangle \n {ctm} {pathConstructionRenderInfo.SegmentData.Count}");*/
                        break;
                }
            }

            _pathInfos.Clear();
            return null;
        }

        public void ClipPath(int rule)
        {
        }

        /// <summary>
        /// Method returning an approximate key (vertical), if none exists, or existing one if one is within a UniDirectionalThreshold.
        /// </summary>
        /// <param name="first"> PdfCoordinate of the first point. </param>
        /// <param name="second"> PdfCoordinate of the second point. </param>
        /// <returns> A PdfCoordinate where X = 0. </returns>
        private PdfCoordinate GetMiddleCoordinateVerticalKey(PdfCoordinate first, PdfCoordinate second)
        {
            var closestFirst = GetClosestToCoordinate(first);
            var closestSecond = GetClosestToCoordinate(second);
            var keyCoordinate = new PdfCoordinate(0.0, ((closestFirst + closestSecond) / 2).Y);

            keyCoordinate = AllAvailableTablesKeyMiddleCoordinate.Keys.FirstOrDefault(o =>
                CalculateClosest(keyCoordinate, o) <= UniDirectionalThreshold) ?? keyCoordinate;

            return keyCoordinate;
        }

        private void TryUpdateVerticalLinesDict(PdfCoordinate currentMoveTo, PdfCoordinate next)
        {
            try
            {
                var newLine = new VerticalLineFromTop(currentMoveTo, next);

                var key = GetMiddleCoordinateVerticalKey(newLine.FromCoordinate, newLine.ToCoordinate);

                _allVerticalLinesFromTop.Add(newLine);

                if (!AllAvailableTablesKeyMiddleCoordinate.TryGetValue(key, out var retList))
                {
                    retList = new List<VerticalLineFromTop>();
                    retList.Add(newLine);
                    AllAvailableTablesKeyMiddleCoordinate.Add(key, retList);
                }
                else
                {
                    if (retList.Contains(newLine)) return;
                    retList.Add(newLine);
                    AllAvailableTablesKeyMiddleCoordinate.Remove(key);
                    AllAvailableTablesKeyMiddleCoordinate.Add(key, retList);
                }
            }
            catch (DataException)
            {
            }
        }

        private void AddCoordinateIfNotExistsClose(PdfCoordinate coordinate)
        {
            var existsClosest = CheckExistsClosest(coordinate);
            if (!existsClosest)
            {
                _allCoordinates.Add(coordinate);
            }
        }

        private bool CheckExistsClosest(PdfCoordinate coordinate)
        {
            return _allCoordinates.Any(o => CalculateClosest(coordinate, o) <= BiDirectionalThreshold);
        }

        private double CalculateClosest(PdfCoordinate coordinate, PdfCoordinate pdfCoordinate)
        {
            return Math.Sqrt(Math.Pow(coordinate.X - pdfCoordinate.X, 2) + Math.Pow(coordinate.Y - pdfCoordinate.Y, 2));
        }

        private GraphicsState GetGraphicsState(PathPaintingRenderInfo renderInfo)
        {
            System.Reflection.FieldInfo gsField = typeof(PathPaintingRenderInfo).GetField("gs",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var gs = gsField?.GetValue(renderInfo) as GraphicsState;
            return gs;
        }
    }
}