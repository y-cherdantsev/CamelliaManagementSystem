using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CamelliaManagementSystem.FileManage.DictionaryParsers.Objects;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace CamelliaManagementSystem.FileManage.DictionaryParsers
{
    public class PdfDictTransformer
    {
        private const string Splitter = "|||";
        
        /// <summary>
        /// If the field outlined
        /// </summary>
        private const float Treshold = 27.1f;
        
        public async Task<Dictionary<string, List<string>>> ReadPdfFileCreateListData(string fileName)
        {
            var dictionary = new Dictionary<PdfTextField, PdfTextField>();

            if (!File.Exists(fileName)) return null;
            var documentAllTextFieldsSortedSet = new SortedSet<PdfTextField>();

            var fileBytes = await File.ReadAllBytesAsync(fileName);

            // Can accept both a ReadAsync and simply a sync FileStream.
            var pdfReader = new PdfReader(fileBytes);

            PdfTextField bufferKey = null;
            PdfTextField bufferValues = null;

            for (var page = 1; page <= pdfReader.NumberOfPages; page++)
            {
                var size = pdfReader.GetPageSizeWithRotation(page);

                ITextExtractionStrategy strategy = new LocationTextExtractionStrategy();
                var listener = new PdfExtRenderListener(strategy, page);
                var readerParser = new PdfReaderContentParser(pdfReader);
                readerParser.ProcessContent(page, listener);
                foreach (var textField in listener.AllTextFieldsSortedSet)
                    documentAllTextFieldsSortedSet.Add(textField);

                AddToDictDoWhile(listener, size, dictionary, ref bufferKey, ref bufferValues,
                    documentAllTextFieldsSortedSet);
            }

            pdfReader.Close();

            TryAdd(bufferKey, bufferValues, dictionary);

            var result = new Dictionary<string, List<string>>();

            foreach (var pdfTextFields in dictionary)
            {
                var key = pdfTextFields.Key.UnformattedContent.Replace(Splitter, " ").Trim().Trim(':').Trim(';').Trim();
                var valuesList = pdfTextFields.Value?.UnformattedContent.Split(Splitter).ToList();
                valuesList?.ForEach(x => x.Trim());
                valuesList?.RemoveAll(x => x.Equals(string.Empty));
                if (result.ContainsKey(key))
                    key += $"_{result.Count(x => x.Key.StartsWith(key))}";
                result.Add(key, valuesList);
            }

            return result;
        }

        /// <summary>
        /// Tries adding to a resulting dict a key-value pair of TextFields.
        /// </summary>
        /// <param name="key"> Accepts both null and not null bufferKey. If null, but has value != null, creates mock key for that value. </param>
        /// <param name="values"></param>
        /// <param name="returnDict"></param>
        private static void TryAdd(PdfTextField key, PdfTextField values,
            IDictionary<PdfTextField, PdfTextField> returnDict)
        {
            if (key == null)
            {
                if (values == null) return;
                var newEmptyKey = new PdfTextField(
                    new PdfCoordinate(0, values.RootBottomLeft.Y),
                    0,
                    0,
                    values.Page + " -> " + values.RootBottomLeft.Y,
                    values.Page);
                returnDict.Add(newEmptyKey, values);
            }
            else
            {
                if (!returnDict.ContainsKey(key))
                    returnDict.Add(key, values);
            }
        }

        private void AddToDictDoWhile(PdfExtRenderListener listener, Rectangle size,
            IDictionary<PdfTextField, PdfTextField> returnDict, ref PdfTextField bufferKey,
            ref PdfTextField bufferValues, SortedSet<PdfTextField> fieldsSortedSet)
        {
            using var iter = ExtractListOfTuplesOrderedFromTop(
                    listener.AllAvailableTablesKeyMiddleCoordinate,
                    listener.AllTextFieldsSortedSet,
                    size,
                    1)
                .GetEnumerator();
            do
            {
                var entry = iter.Current;
                if (entry != null)
                {
                    // Console.WriteLine(entry.Item1?.UnformattedContent + " ||| " + entry.Item2?.UnformattedContent);
                    if (entry.Item1 != null)
                    {
                        if (bufferValues != null)
                        {
                            bufferValues.UnformattedContent += FindLostDataFromValueToKey(
                                bufferValues,
                                entry.Item2,
                                fieldsSortedSet);
                        }

                        TryAdd(bufferKey, bufferValues, returnDict);

                        bufferKey = entry.Item1;
                        bufferValues = entry.Item2;
                    }
                    else
                    {
                        if (bufferValues == null)
                        {
                            if (entry.Item2 != null)
                            {
                                bufferValues = entry.Item2;
                            }
                        }
                        else
                        {
                            if (entry.Item2 != null)
                            {
                                bufferValues += entry.Item2;
                            }
                        }
                    }
                }

                if (!iter.MoveNext())
                {
                    break;
                }
            } while (true);
        }

        /// <summary>
        /// Finds all textFields from the fieldsSortedSet, that are in between fromValue and toValue, have same X location, and have the same font.
        /// In case toValue is null, take everything in between fromValue and next existing key.
        /// </summary>
        private string FindLostDataFromValueToKey(PdfTextField fromValue, PdfTextField toValue,
            SortedSet<PdfTextField> fieldsSortedSet)
        {
            var retStr = string.Empty;

            var from = fromValue.UnformattedContent.Split(Splitter)[^2];
            var root = fromValue.RootBottomLeft;
            var to = string.Empty;
            if (toValue != null)
            {
                to = toValue.UnformattedContent.Split(Splitter)[0];
            }

            var filteredSet = fieldsSortedSet
                .Where(o =>
                    Math.Abs(o.RootBottomLeft.X - root.X) <= 2.0 &
                    (o.Page > fromValue.Page | (o.Page == fromValue.Page &
                                                o.RootBottomLeft.CompareTo(fromValue.RootBottomLeft) >= 0)));

            var start = false;
            foreach (var item in filteredSet)
            {
                // Console.WriteLine(item.UnformattedContent + item.RootBottomLeft.Y + " " + item.RootBottomLeft.X + " " + item.RootBottomLeft.CompareTo(fromValue.RootBottomLeft) + " " + Math.Abs(item.RootBottomLeft.X - root.X));
                if (item.UnformattedContent.Contains(from) && start == false)
                {
                    start = true;
                    continue;
                }

                if (!start) continue;

                if (to == string.Empty || item.UnformattedContent.Contains(to))
                {
                    break;
                }

                var content = item.UnformattedContent;
                retStr += content.Contains(Splitter) ? content : content + Splitter;
            }

            return retStr;
        }

        /// <summary>
        /// Extracts a List of Tuples(KeyString, ValueString) for each of the table cells that contain text. If that table row satisfies filterNofCells condition.
        /// </summary>
        private static List<Tuple<PdfTextField, PdfTextField>> ExtractListOfTuplesOrderedFromTop(
            SortedDictionary<PdfCoordinate, List<VerticalLineFromTop>> keysVertical,
            SortedSet<PdfTextField> orderedSetAllText, Rectangle size, int filterNofCells)
        {
            var result = new List<Tuple<PdfTextField, PdfTextField>>();
            var keys = keysVertical.Keys.Reverse();
            foreach (var keyCoordinate in keys)
            {
                if (!keysVertical.TryGetValue(keyCoordinate, out var lines)) continue;
                lines.Sort();
                if (lines.Count <= filterNofCells) continue;
                PdfTextField keyTextField = null;
                PdfTextField valueTextField = null;
                for (var l = 0; l < lines.Count - 1; l++)
                {
                    var rect = new Rectangle(
                        (float) lines[l].FromCoordinate.X,
                        (float) lines[l].FromCoordinate.Y,
                        (float) lines[l + 1].ToCoordinate.X,
                        (float) lines[l + 1].ToCoordinate.Y);

                    foreach (var textField in orderedSetAllText)
                    {
                        var text = IsTextFieldInsideRectangle(rect, textField, size) ? textField : null;
                        if (text == null) continue;
                        text.UnformattedContent += Splitter;
                        if (l == 0)
                        {
                            if (keyTextField == null)
                            {
                                keyTextField = text;
                            }
                            else
                            {
                                keyTextField += text;
                            }
                        }
                        else
                        {
                            if (valueTextField == null)
                            {
                                valueTextField = text;
                            }
                            else
                            {
                                valueTextField += text;
                            }
                        }
                    }
                }

                result.Add(new Tuple<PdfTextField, PdfTextField>(keyTextField, valueTextField));
            }

            return result;
        }

        private static bool IsTextFieldInsideRectangle(Rectangle outer, PdfTextField inner, Rectangle size)
        {
            var llx = (float) Math.Round(inner.RootBottomLeft.X);
            var lly = size.Height - (float) Math.Round(inner.RootBottomLeft.Y);
            var urx = (float) Math.Round(llx + inner.Length);
            var ury = (float) Math.Round(lly + inner.Height);

            var rect = new Rectangle(llx, lly, urx, ury);

            return (outer.Left <= rect.Left | Math.Abs(outer.Left - rect.Left) < Treshold)
                   & outer.Top >= rect.Top
                   & (outer.Right >= rect.Right | Math.Abs(outer.Right - rect.Right) < Treshold)
                   & outer.Bottom <= rect.Bottom;
        }
    }
}