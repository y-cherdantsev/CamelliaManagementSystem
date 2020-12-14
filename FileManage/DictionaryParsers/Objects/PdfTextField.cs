using System;

namespace CamelliaManagementSystem.FileManage.DictionaryParsers.Objects
{
    /// <inheritdoc />
    public class PdfTextField : IComparable
    {
        public PdfTextField(PdfCoordinate rootBottomLeft, double length, double height, string unformattedContent,
            int page)
        {
            RootBottomLeft = rootBottomLeft;
            Length = length;
            Height = height;
            UnformattedContent = unformattedContent;
            Page = page;
        }

        public int Page { get; set; }

        public PdfCoordinate RootBottomLeft { get; private set; }

        public double Length { get; private set; }

        public double Height { get; private set; }

        public string UnformattedContent { get; set; }

        public static PdfTextField operator +(PdfTextField from, PdfTextField to)
        {
            return new PdfTextField(
                from.RootBottomLeft,
                from.Length + to.Length,
                from.Height + to.Height,
                from.UnformattedContent + to.UnformattedContent,
                from.Page);
        }

        /// <summary>
        /// First by page, later by root.
        /// </summary>
        public int CompareTo(object? obj)
        {
            if (!(obj is PdfTextField))
                throw new ArgumentException($"{obj} is not an instance of {typeof(PdfTextField)}.");
            var compareObject = (PdfTextField) obj;

            var pageDiff = Page.CompareTo(compareObject.Page);
            return pageDiff != 0 ? pageDiff : RootBottomLeft.CompareTo(compareObject.RootBottomLeft);
        }
    }
}