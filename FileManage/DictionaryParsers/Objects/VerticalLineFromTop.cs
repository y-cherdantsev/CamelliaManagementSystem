using System;
using System.Data;

namespace CamelliaManagementSystem.FileManage.DictionaryParsers.Objects
{
    /// <summary>
    /// Contains From -> To coordinates of a vertical line, starting from the top.
    /// </summary>
    public class VerticalLineFromTop : IComparable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VerticalLineFromTop"/> class.
        /// Constructs 1 vertical line if horizontal uniDirectionalThreshold holds.
        /// </summary>
        /// <exception cref="DataException"> If conditions for VerticalLine aren't satisfied. </exception>
        public VerticalLineFromTop(PdfCoordinate moveTo, PdfCoordinate lineTo)
        {
            if (Math.Abs(lineTo.X - moveTo.X) > PdfExtRenderListener.UniDirectionalThreshold)
            {
                throw new DataException($"At {typeof(VerticalLineFromTop)}. Message: Should be vertical!");
            }

            if (Math.Abs(lineTo.Y - moveTo.Y) < PdfExtRenderListener.UniDirectionalThreshold)
            {
                throw new DataException($"At {typeof(VerticalLineFromTop)}. Message: Should be longer!");
            }

            if (Math.Abs(lineTo.Y - moveTo.Y) > PdfExtRenderListener.VerticalThreshold)
            {
                throw new DataException($"At {typeof(VerticalLineFromTop)}. Message: Should be shorter!");
            }

            if (moveTo.CompareTo(lineTo) > 0)
            {
                FromCoordinate = lineTo;
                ToCoordinate = moveTo;
            }
            else
            {
                FromCoordinate = moveTo;
                ToCoordinate = lineTo;
            }
        }

        public PdfCoordinate FromCoordinate { get; private set; }

        public PdfCoordinate ToCoordinate { get; private set; }

        public int CompareTo(object? obj)
        {
            if (!(obj is VerticalLineFromTop))
                throw new ArgumentException($"At {typeof(VerticalLineFromTop)}. Message: {obj} is not an instance of {typeof(VerticalLineFromTop)}.");
            var to = obj as VerticalLineFromTop;

            var fromDiff = this.FromCoordinate.CompareTo(to.FromCoordinate);
            if (fromDiff != 0)
                return fromDiff;
            else return this.ToCoordinate.CompareTo(to.ToCoordinate);
        }

        public override bool Equals(object? obj)
        {
            if (!(obj is VerticalLineFromTop))
                return false;
            var to = obj as VerticalLineFromTop;

            return this.CompareTo(obj) == 0;
        }

        public override int GetHashCode()
        {
            return FromCoordinate.GetHashCode() ^ ToCoordinate.GetHashCode();
        }

        public override string ToString()
        {
            return "From " + FromCoordinate + " to " + ToCoordinate;
        }
    }
}