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
        private readonly List<Sector> outputSectors = new List<Sector>();

        private double brushX;

        private double brushY;

        private double workAreaHeight;

        private double workAreaWidth;

        private double currentHeight;

        private bool isDrawingVertically;

        /// <summary>
        /// Plots the specified values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="workArea">The work area.</param>
        /// <returns>A list of Sectors detailing how they should be layed out to create the TreeMap.</returns>
        public List<Sector> Plot(IEnumerable<InputValue> values, SizeD size)
        {
            Reset(size);

            Squarify(1, PrepareSectors(values).ToList(), new List<Sector>(), GetWidth());

            return outputSectors;
        }

        private void Reset(SizeD size)
        {
            outputSectors.Clear();
            brushX = 0;
            brushY = 0;
            workAreaWidth = size.Width;
            workAreaHeight = size.Height;
            currentHeight = 0;

        }

        /// <summary>
        /// Calculates each Sectors area.
        /// </summary>
        /// <remarks>
        /// Calculate the total area available given the workArea size.
        /// Calculate the percentage (of the sum of all input values), each individual input value represents.
        /// Use that percentage to assign a percentage of the totalArea available to that sector.
        /// </remarks>
        /// <param name="values">Input values.</param>
        /// <returns>The prepared sections</returns>
        private IEnumerable<Sector> PrepareSectors(IEnumerable<InputValue> values)
        {
            double totalArea = workAreaWidth * workAreaHeight;
            double sumOfValues = values.Sum(value => value.Value);
 
            List<Sector> sectors = new List<Sector>();

            foreach (InputValue inputValue in values.OrderByDescending(v => v.Value))
            {
                double percentage = (inputValue.Value / sumOfValues) * 100;
                double area = (totalArea / 100) * percentage;

                yield return new Sector()
                        {
                            OriginalValue = inputValue,
                            Area = area,
                            Percentage = percentage
                        };
            }
        }

        /// <summary>
        /// Recursively processes the values list provided, laying them out into rows of squares.
        /// </summary>
        /// <param name="values">The input values.</param>
        /// <param name="currentRow">The sectors in the current row.</param>
        /// <param name="width">The width of the current row.</param>
        private void Squarify(int count, IList<Sector> values, List<Sector> currentRow, double width)
        {
            if (width < 0 || width == 0 || width == double.NaN || values.Count == 0)
                return;

            count++;

            // hacky mcHack
            if (count == 50)
            {
                return;
            }

            List<Sector> nextIterationPreview = currentRow.ToList();

            if (values.Count > 1)
            {
                nextIterationPreview.Add(values[0]);
            }

            double currentAspectRatio = CalculateAspectRatio(currentRow, width);
            double nextAspectRatio = CalculateAspectRatio(nextIterationPreview, width);

            if (currentAspectRatio == 0 ||(nextAspectRatio < currentAspectRatio && nextAspectRatio >= 1))
            {
                currentRow.Add(values[0]);
                values.RemoveAt(0);
                currentHeight = CalculateHeight(currentRow, width);

                if (values.Count > 0)
                {
                    Squarify(count, values, currentRow, width);
                }
                else
                {
                    LayoutRow(currentRow);
                }
            }
            else
            {
                // Row has reached it's optimum size
                LayoutRow(currentRow);

                // Start the next row, by passing an empty list of row values and recalculating the current width
                Squarify(count, values, new List<Sector>(), GetWidth());
            }
        }

        /// <summary>
        /// Layouts the row.
        /// </summary>
        /// <param name="rowSectors">The row sectors.</param>
        private void LayoutRow(IEnumerable<Sector> rowSectors)
        {
            PointD brushStartingPoint = new PointD() { X = brushX, Y = brushY };

            if (!isDrawingVertically)
            {
                if (workAreaHeight != currentHeight)
                {
                    brushY = workAreaHeight - currentHeight;
                }
            }

            // Draw each sector in the current row
            foreach (Sector sector in rowSectors)
            {
                // Calculate Width & Height
                double width;
                double height;

                if (isDrawingVertically)
                {
                    width = currentHeight;
                    height = sector.Area / currentHeight;
                }
                else
                {
                    width = sector.Area / currentHeight;
                    height = currentHeight;
                }

                sector.Rect = new RectangleD(brushX, brushY, width, height);

                outputSectors.Add(sector);

                // Reposition brush for the next sector
                if (isDrawingVertically)
                {
                    brushY += height;
                }
                else
                {
                    brushX += width;
                }
            }

            // Finished drawing all sectors in the row
            // Reposition the brush ready for the next row
            if (isDrawingVertically)
            {
                // x increase by width (in vertical, currentHeight is width)
                // y reset to starting position
                brushX += currentHeight;
                brushY = brushStartingPoint.Y;
                workAreaWidth -= currentHeight;
            }
            else
            {
                brushX = brushStartingPoint.X;
                brushY = brushStartingPoint.Y;
                workAreaHeight -= currentHeight;
            }

            currentHeight = 0;
        }

        /// <summary>
        /// Calculates the aspect ratio.
        /// </summary>
        /// <param name="currentRow">The current row.</param>
        /// <param name="width">The width.</param>
        /// <returns>The Aspect Ratio</returns>
        private double CalculateAspectRatio(List<Sector> currentRow, double width)
        {
            double sumOfAreas = 0;
            currentRow.ForEach(sector => sumOfAreas += sector.Area);

            double maxArea = int.MinValue;
            double minArea = int.MaxValue;

            foreach (Sector sector in currentRow)
            {
                if (sector.Area > maxArea)
                {
                    maxArea = sector.Area;
                }

                if (sector.Area < minArea)
                {
                    minArea = sector.Area;
                }
            }

            double widthSquared = (double) Math.Pow(width, 2);
            double sumOfAreasSqaured = (double) Math.Pow(sumOfAreas, 2);

            double ratio1 = (widthSquared * maxArea) / sumOfAreasSqaured;
            double ratio2 = sumOfAreasSqaured / (widthSquared * minArea);

            return Math.Max(ratio1, ratio2);
        }

        /// <summary>
        /// Calculates the height.
        /// </summary>
        /// <param name="currentRow">The current row.</param>
        /// <param name="width">The width of the current row.</param>
        /// <returns>The height of the current row.</returns>
        private double CalculateHeight(List<Sector> currentRow, double width)
        {
            double sum = 0;
            currentRow.ForEach(sector => sum += sector.Area);
            return sum / width;
        }

        /// <summary>
        /// Establishes whether to work vertically or horizontally and returns the relevant width
        /// </summary>
        /// <remarks>
        /// When working vertically, "width" is the actual height of the work space.
        /// </remarks>
        /// <returns>Width of the current Row</returns>
        private double GetWidth()
        {
            if (workAreaHeight > workAreaWidth)
            {
                isDrawingVertically = false;
                return workAreaWidth;
            }

            isDrawingVertically = true;
            return workAreaHeight;
        }
    }

    public class Sector
    {
        public InputValue OriginalValue;

        public double Area;

        public double Percentage;

        public RectangleD Rect;
    }

    public struct RectangleD
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
            RectangleD contracted = this;

            contracted.X += amount;
            contracted.Y += amount;

            contracted.Width -= amount * 2;
            contracted.Height -= amount * 2;

            if (contracted.Width < 0 || contracted.Height < 0)
            {
                contracted.Width = 0;
                contracted.Height = 0;
            }

            return contracted;
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
    }
}
