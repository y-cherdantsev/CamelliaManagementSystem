using System;

namespace CamelliaManagementSystem.FileManage.DictionaryParsers.Objects
{
    public class PdfCoordinate : IComparable
    {
        private double _x;
        private double _y;

        public PdfCoordinate(double x, double y)
        {
            _x = Math.Round(x);
            _y = Math.Round(y);
        }

        /// <summary>
        /// Gets or sets from left to right.
        /// </summary>
        public double X
        {
            get
            {
                return this._x;
            }

            set
            {
                this._x = Math.Round(value);
            }
        }

        /// <summary>
        /// Gets or sets from top to bottom.
        /// </summary>
        public double Y
        {
            get
            {
                return this._y;
            }

            set
            {
                this._y = Math.Round(value);
            }
        }

        public static PdfCoordinate operator +(PdfCoordinate to, PdfCoordinate add)
        {
            return new PdfCoordinate(add.X + to.X, add.Y + to.Y);
        }

        public static PdfCoordinate operator /(PdfCoordinate orig, int div)
        {
            return new PdfCoordinate(orig.X / div, orig.Y / div);
        }

        public int CompareTo(object? obj)
        {
            if (!(obj is PdfCoordinate))
                throw new ArgumentException($"{obj} is not an instance of {typeof(PdfCoordinate)}.");
            var to = obj as PdfCoordinate;

            /*
            var pageDiff = this.Page.CompareTo(to.Page);
            if (pageDiff != 0)
                return pageDiff;
                */

            var yDiff = this.Y.CompareTo(to.Y);
            if (yDiff != 0)
                return yDiff;
            else return this.X.CompareTo(to.X);
        }

        public override string ToString()
        {
            return this.X + " " + this.Y;
        }
    }
}