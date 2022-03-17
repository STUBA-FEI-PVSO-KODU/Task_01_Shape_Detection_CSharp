using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Shape_Detection_CSharp
{
    /// <summary>
    /// Defines a hough transformation class.
    /// </summary>
    public class HoughTransformation
    {
        #region Constants
        /// <summary>
        /// Represents a constant value for conversion of degrees into radians.
        /// </summary>
        public const double DEG2RAD = Math.PI / 180.0;
        /// <summary>
        /// Represents a constant value for conversion of radians into degrees.
        /// </summary>
        public const double RAD2DEG = 180.0 / Math.PI;
        #endregion
        #region Properties
        /// <summary>
        /// Represents the image width.
        /// </summary>
        public int ImageWidth { get; private set; }
        /// <summary>
        /// Represents the image height.
        /// </summary>
        public int ImageHeight { get; private set; }
        /// <summary>
        /// Represents the hough height (-R -> +R). 
        /// </summary>
        public double HoughHeight { get; private set; }
        /// <summary>
        /// Represents the accumulator width (180°).
        /// </summary>
        public int AccumulatorWidth { get; private set; }
        /// <summary>
        /// Represents the accumulator height.
        /// </summary>
        public int AccumulatorHeight { get; private set; }
        /// <summary>
        /// Represents an accumulator of the <see cref="HoughTransformation"/> instance.
        /// </summary>
        /// <remarks>
        /// ACCUMULATOR: An Accumulator is nothing more than a 2 dimensional matrix with θmax columns and rmax rows.
        /// So each cell, or bin, represents a unique coordinate (r, θ) and thus one unique line in Hough Space.
        /// The accumulator bins contain a count of pixels for every possible line. 
        /// Go over the bins and only keep the ones that are over a certain threshold (for example, you can decide to only consider it a real line if it contains at least 80 pixels). 
        /// The result is a set of coordinates(r, θ), one for every straight line in the image thats above the threshold.
        /// And these polar coordinates easily can be computed back to 2 points in the original image space, just solve the equation for x and y (r = x.cos(θ) + y.sin(θ)).
        /// </remarks>
        public List<int> Accumulator { get; private set; }
        /// <summary>
        /// Represents an image center X position.
        /// </summary>
        public int CenterX { get; private set; }
        /// <summary>
        /// Represents an image center Y position.
        /// </summary>
        public int CenterY { get; private set; }
        #endregion
        #region Constructors
        /// <summary>
        /// Creates a new <see cref="HoughTransformation"/> instance.
        /// </summary>
        public HoughTransformation()
        {
            ImageWidth = -1;
            ImageHeight = -1;
            AccumulatorHeight = -1;
            AccumulatorWidth = -1;
            Accumulator = new List<int>();
            CenterX = -1;
            CenterY = -1;
        }
        #endregion
        #region Methods
        /// <summary>
        /// Transform provided image data into hough space.
        /// </summary>
        /// <remarks>
        /// The method loops over all pixels in the edge image and increments the accumulator at the computed (r, θ).
        /// </remarks>
        /// <param name="data">Represents a list of image data as <see cref="byte"/> values.</param>
        /// <param name="width">Represents an image width.</param>
        /// <param name="height">Represents an image height.</param>
        /// <returns>Returns true if result of transformation was successful, otherwise false.</returns>
        public bool Transform(List<byte> data, int width, int height, byte pixelMinValue = 250)
        {
            var result = false;
            if (data != null && width > 0 && height > 0)
            {
                ImageWidth = width;
                ImageHeight = height;
                HoughHeight = Math.Sqrt(width * width + height * height) / 2;
                AccumulatorHeight = (int)(HoughHeight * 2.0);
                AccumulatorWidth = 360;
                Accumulator.Clear();
                var size = AccumulatorHeight * AccumulatorWidth;
                Accumulator.Capacity = size;
                for (int i = 0; i < size; i++)
                {
                    Accumulator.Add(0);
                }
                CenterX = width / 2;
                CenterY = height / 2;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (data[(y * width) + x] > pixelMinValue)
                        {
                            for (int t = 0; t < AccumulatorWidth; t++)
                            {
                                double r = ((x - CenterX) * Math.Cos(t * DEG2RAD)) + ((y - CenterY) * Math.Sin(t * DEG2RAD));
                                var index = (int)(Math.Round(r + HoughHeight) * AccumulatorWidth) + t;
                                Accumulator[index]++;
                            }
                        }
                    }
                }
                result = Accumulator.Count > 0;
            }
            return result;
        }
        /// <summary>
        /// Checks provided value if it is local maximum.
        /// </summary>
        /// <param name="value">Represents a value which checks if local maximum.</param>
        /// <param name="r">Represents index of accumulator height (radius).</param>
        /// <param name="t">Represents index of accumulator width (thetha).</param>
        /// <param name="localSpace">Represents a 2d local space, which is check.</param>
        /// <returns>Returns true if provided value is maximum, otherwise false.</returns>
        public bool IsLocalMax(int value, int r, int t, Vector2 localSpace)
        {
            var result = false;
            if (localSpace != Vector2.Zero && r >= 0 && t >= 0)
            {
                var x2 = (int)Math.Floor(localSpace.X / 2);
                var y2 = (int)Math.Floor(localSpace.Y / 2);
                var localMax = value;
                for (int ly = -y2; ly <= y2; ly++)
                {
                    for (int lx = -x2; lx <= x2; lx++)
                    {
                        if ((ly + r >= 0 && ly + r < AccumulatorHeight) && (lx + t >= 0 && lx + t < AccumulatorWidth))
                        {
                            var localVal = Accumulator[((r + ly) * AccumulatorWidth) + (t + lx)];
                            if (localVal > localMax)
                            {
                                localMax = localVal;
                                lx = x2 + 1;
                                ly = y2 + 1;
                            }
                        }
                    }
                }
                result = value >= localMax;
            }
            return result;
        }
        /// <summary>
        /// Try to extract lines from accumulator via provided threshold.
        /// </summary>
        /// <remarks>
        /// The methods loops over the accumulator and consider every bin which value is on or above the threshold.
        /// Than we check if that bin is a local maximum.
        /// If we decide the line at coordinates (r, θ) is a valid one we compute them back to two points in the image space.
        /// </remarks>
        /// <param name="threshold">Represents a threshold value, which defines if we consider accumulator value as line.</param>
        /// <returns>Returns a list of extracted lines, which fulfill provided threshold value.</returns>
        public List<Position> GetLines(int threshold)
        {
            var result = new List<Position>();
            if (Accumulator.Count > 0 && threshold > 0)
            {
                for (int r = 0; r < AccumulatorHeight; r++)
                {
                    for (int t = 0; t < AccumulatorWidth; t++)
                    {
                        var accValue = Accumulator[(r * AccumulatorWidth) + t];
                        if ((int)accValue >= threshold)
                        {
                            if (IsLocalMax(accValue, r, t, new Vector2(9, 9)))
                            {
                                /*int x1, y1, x2, y2;
                                if ((t >= 45 && t <= 135 )|| (t >= 45 + 180 && t <= 135 + 180))
                                {
                                    //y = (r - x cos(t)) / sin(t) 
                                    x1 = 0;
                                    y1 = (int)((r - (AccumulatorHeight / 2) - ((x1 - (ImageWidth / 2)) * Math.Cos(t * DEG2RAD))) / Math.Sin(t * DEG2RAD) + (ImageHeight / 2));
                                    x2 = ImageWidth - 0;
                                    y2 = (int)((r - (AccumulatorHeight / 2) - ((x2 - (ImageWidth / 2)) * Math.Cos(t * DEG2RAD))) / Math.Sin(t * DEG2RAD) + (ImageHeight / 2));
                                }
                                else
                                {
                                    //x = (r - y sin(t)) / cos(t);  
                                    y1 = 0;
                                    x1 = (int)((r - (AccumulatorHeight / 2) - ((y1 - (ImageHeight / 2)) * Math.Sin(t * DEG2RAD))) / Math.Cos(t * DEG2RAD) + (ImageWidth / 2));
                                    y2 = ImageHeight - 0;
                                    x2 = (int)((r - (AccumulatorHeight / 2) - ((y2 - (ImageHeight / 2)) * Math.Sin(t * DEG2RAD))) / Math.Cos(t * DEG2RAD) + (ImageWidth / 2));
                                }*/
                                var a = Math.Cos(t * DEG2RAD);
                                var b = Math.Sin(t * DEG2RAD);
                                var x0 = (a * ((double)r - HoughHeight)) + CenterX;
                                var y0 = (b * ((double)r - HoughHeight)) + CenterY;
                                var x1 = (int)(x0 + ImageWidth * (-b));
                                var y1 = (int)(y0 + ImageHeight * (a));
                                var x2 = (int)(x0 - ImageWidth * (-b));
                                var y2 = (int)(y0 - ImageHeight * (a));
                                var line = new Position(x1, y1, x2, y2);
                                //var line = new Position(y1,x1, y2,x2);
                                result.Add(line);
                            }
                        }
                    }
                }
            }
            return result;
        }

        public List<int> GetAccu(out int w, out int h)
        {
            w = AccumulatorWidth;
            h = AccumulatorHeight;
            return Accumulator;
        }

        #endregion
    }
}
