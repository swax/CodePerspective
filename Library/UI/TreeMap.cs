using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace XLibrary
{

    /// <summary>
    /// TreeMap will create a TreeMap layout based a series of input values
    /// </summary>
    public class TreeMap
    {
        public readonly List<Sector> Results = new List<Sector>();

        PointF Pos;
        SizeF WorkArea;
        SizeF OriginalArea;

        float CurrentHeight;
        bool DrawingVertically;


        public TreeMap(NodeModel root, NodeModel exclude, SizeF size)
        {
            var values = from n in root.Nodes
                        where n.Show && n != exclude
                        select n;

            WorkArea = size;
            OriginalArea = size;

            float totalArea = WorkArea.Width * WorkArea.Height;
            float totalValue = root.Value; // values.Sum(value => value.Value);

            var prepared = from v in values
                           orderby v.Value descending
                           let area = totalArea * (v.Value / totalValue)
                           select new Sector(v, area);

            Squarify(1, prepared.ToList(), new List<Sector>(), GetWidth());
        }

        private void Squarify(int count, IList<Sector> values, List<Sector> currentRow, float width)
        {
            count++;

            // hacky mcHack
            //if (count == 10000)
            //    return;

            List<Sector> nextIterationPreview = currentRow.ToList();

            if (values.Count == 0)
            {
                LayoutRow(currentRow);
                return;
            }
            else if (values.Count > 1)
                nextIterationPreview.Add(values[0]);

            float currentAspectRatio = CalculateAspectRatio(currentRow, width);
            float nextAspectRatio = CalculateAspectRatio(nextIterationPreview, width);

            if (currentAspectRatio == 0 || 
                (1 <= nextAspectRatio && nextAspectRatio < currentAspectRatio))
            {
                currentRow.Add(values[0]);
                values.RemoveAt(0);

                CurrentHeight = currentRow.Sum(s => s.Area) / width;

                if (float.IsNaN(CurrentHeight))
                    CurrentHeight = 0;

                Squarify(count, values, currentRow, width);
            }
            else
            {
                // Row has reached it's optimum size
                LayoutRow(currentRow);

                // Start the next row, by passing an empty list of row values and recalculating the current width
                Squarify(count, values, new List<Sector>(), GetWidth());
            }
        }

        private void LayoutRow(IEnumerable<Sector> rowSectors)
        {
            PointF startingPoint = Pos;

            if (!DrawingVertically)// && WorkArea.Height != CurrentHeight)
                Pos.Y = WorkArea.Height - CurrentHeight;

            // Draw each sector in the current row
            foreach (Sector sector in rowSectors)
            {
                // Calculate Width & Height
                float width;
                float height;

                if (DrawingVertically)
                {
                    width = CurrentHeight;
                    height = sector.Area / CurrentHeight;
                }
                else
                {
                    width = sector.Area / CurrentHeight;
                    height = CurrentHeight;
                }

                if (float.IsNaN(width))
                    width = 0;
                if (float.IsNaN(height))
                    height = 0;

                sector.Rect = new RectangleF(Pos.X, Pos.Y, width, height);

                /*if (sector.Rect.X > OriginalArea.Width ||
                    sector.Rect.X + sector.Rect.Width > OriginalArea.Width)
                {
                    sector.Rect.X = 0;
                }

                if (sector.Rect.Y > OriginalArea.Height ||
                   sector.Rect.Y + sector.Rect.Height > OriginalArea.Height)
                {
                    sector.Rect.Y = 0;
                }*/


                Results.Add(sector);

                // Reposition brush for the next sector
                if (DrawingVertically)
                    Pos.Y += height;
                else
                    Pos.X += width;
            }

            // Finished drawing all sectors in the row
            // Reposition the brush ready for the next row
            if (DrawingVertically)
            {
                // x increase by width (in vertical, currentHeight is width)
                // y reset to starting position
                Pos.X += CurrentHeight;
                Pos.Y = startingPoint.Y;
                WorkArea.Width -= CurrentHeight;
            }
            else
            {
                Pos.X = startingPoint.X;
                Pos.Y = startingPoint.Y;
                WorkArea.Height -= CurrentHeight;
            }

            CurrentHeight = 0;
        }

        private float CalculateAspectRatio(List<Sector> row, float width)
        {
            if (row.Count == 0)
                return 0;

            float totalArea = row.Sum(s => s.Area);
            float totalAreaSqaured = (float) Math.Pow(totalArea, 2);

            float widthSquared = (float)Math.Pow(width, 2);
            float maxArea = row.Max(s => s.Area);
            float minArea = row.Min(s => s.Area);

            float ratio1 = (widthSquared * maxArea) / totalAreaSqaured;
            float ratio2 = totalAreaSqaured / (widthSquared * minArea);

            return Math.Max(ratio1, ratio2);
        }

        private float GetWidth()
        {
            DrawingVertically = WorkArea.Height < WorkArea.Width;

            return DrawingVertically ? WorkArea.Height : WorkArea.Width;
        }
    }

    public class Sector
    {
        public NodeModel OriginalValue;
        public float Area;
        public RectangleF Rect;

        public Sector(NodeModel value, float area)
        {
            OriginalValue = value;
            Area = area;
        }
    }

    public static class RectangleExtensions
    {
        public static RectangleF Contract(RectangleF rect, float amount)
        {
            RectangleF contacted = new RectangleF();

            // if width less than 1, set to 1
            // if width is less than one, then flo
            if (rect.Width < amount * 2.0f + 1.0f)
            {
                contacted.X = rect.X + rect.Width / 2.0f;
                contacted.Width = 1.0f;
            }
            else // new width will be greater than 1
            {
                contacted.X = rect.X + amount;
                contacted.Width = rect.Width - amount * 2.0f;
            }

            if (rect.Height < amount * 2.0f + 1.0f)
            {
                contacted.Y = rect.Y + rect.Height / 2.0f;
                contacted.Height = 1.0f;
            }
            else
            {
                contacted.Y = rect.Y + amount;
                contacted.Height = rect.Height - amount * 2.0f;
            }

            return contacted;
        }

    }


    /*public struct RectangleD
    {
        public double X;
        public double Y;
        public double Width;
        public double Height;

        public SizeD Size { get { return new SizeD() { Width = Width, Height = Height }; } }

        public RectangleD(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public RectangleD Contract(double amount)
        {
            // if width less than 1, set to 1
            // if width is less than one, then flo
            if (Width < amount * 2 + 1)
            {
                X += Width / 2;
                Width = 1;
            }
            else // new width will be greater than 1
            {
                X += amount;
                Width -= amount * 2;
            }

            if (Height < amount * 2 + 1)
            {
                Y += Height / 2;
                Height = 1;
            }
            else
            {
                Y += amount;
                Height -= amount * 2;
            }

            return this;
        }

        public RectangleF ToRectangleF()
        {
            return new RectangleF((float)X, (float)Y, (float)Width, (float)Height);
        }

        public bool Contains(double x, double y)
        {
            return ((((X <= x) && (x < (X + Width))) && (Y <= y)) && (y < (Y + Height)));
        }

    }

    public struct PointD
    {
        public double X;
        public double Y;
    }

    public struct SizeD
    {
        public double Width;
        public double Height;
    }*/
}
