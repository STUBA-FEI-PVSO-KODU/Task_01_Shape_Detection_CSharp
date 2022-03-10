using NumSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shape_Detection_CSharp
{
    /// <summary>
    /// 
    /// </summary>
    public class HoughTransformation2
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
        public List<List<double>> Accumulator { get; private set; }

        public List<double> Rhos { get; private set; }

        public List<double> Thetas { get; private set; }
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
        /// Creates a new <see cref="HoughTransformation2"/> instance.
        /// </summary>
        public HoughTransformation2()
        {
            ImageWidth = -1;
            ImageHeight = -1;
            AccumulatorHeight = -1;
            AccumulatorWidth = -1;
            Accumulator = new List<List<double>>();
            Rhos = new List<double>();
            Thetas = new List<double>();
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
        public bool Transform(List<byte> data, int width, int height, byte numRhos = 180, byte numThetas = 180, byte pixelMinValue = 250)
        {
            var result = false;
            if (data != null && width > 0 && height > 0)
            {
                ImageWidth = width;
                ImageHeight = height;
                HoughHeight = Math.Sqrt(width * width + height * height);
                AccumulatorHeight = (int)(HoughHeight * 2.0);
                AccumulatorWidth = 180;
                var dtheta = AccumulatorWidth / numThetas;
                var drho = AccumulatorHeight / numRhos;
                Rhos = new List<double>(AccumulatorHeight);
                for (int i = (int)-HoughHeight; i < HoughHeight; i += drho) { Rhos.Add(i); }
                Thetas = new List<double>(AccumulatorWidth);
                for (int i = 0; i < AccumulatorWidth; i += dtheta) { Thetas.Add(i); }
                Accumulator = new List<List<double>>(Rhos.Count);
                for (int i = 0; i < Rhos.Count; i++)
                {
                    var list = new List<double>(Rhos.Count);
                    for (int j = 0; j < Rhos.Count; j++)
                    {
                        var val = 0.0;
                        list.Add(val);
                    }
                    Accumulator.Add(list);
                }
                CenterX = width / 2;
                CenterY = height / 2;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var value = data[x * height + y];
                        if (value > 0 && value > pixelMinValue)
                        {
                            for (int theta = 0; theta < AccumulatorWidth; theta++)
                            {
                                double r = ((x - CenterX) * Math.Cos(theta * DEG2RAD)) + ((y - CenterY) * Math.Sin(theta * DEG2RAD));
                                var indexR = -1;
                                for (int i = 0; i < Rhos.Count - 1; i++)
                                {
                                    if (Rhos[i] <= r && r < Rhos[i + 1])
                                    {
                                        indexR = i;
                                        break;
                                    }
                                }
                                Accumulator[indexR][theta] += 1;
                            }
                        }
                    }
                }
                result = Accumulator.Count > 0;
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
        public List<Position> GetLines(int threshold = 220)
        {
            var result = new List<Position>();
            if (Accumulator.Count > 0 && threshold > 0)
            {
                for (int rIndex = 0; rIndex < Accumulator.Count; rIndex++)
                {
                    for (int thetaIndex = 0; thetaIndex < Accumulator[0].Count; thetaIndex++)
                    {
                        var accValue = Accumulator[rIndex][thetaIndex];
                        if (accValue > threshold)
                        {
                            var rho = Rhos[rIndex];
                            var theta = Thetas[thetaIndex];
                            var a = Math.Cos(theta * DEG2RAD);
                            var b = Math.Sin(theta * DEG2RAD);
                            var x0 = (a * rho) + CenterX;
                            var y0 = (b * rho) + CenterY;
                            var x1 = (int)(x0 + ImageWidth * (-b));
                            var y1 = (int)(y0 + ImageHeight * (a));
                            var x2 = (int)(x0 - ImageWidth * (-b));
                            var y2 = (int)(y0 - ImageHeight * (a));
                            var line = new Position(x1, y1, x2, y2);
                            result.Add(line);
                        }
                    }
                }
            }
            return result;
        }

        public List<List<double>> GetAccu(out int w, out int h)
        {
            w = AccumulatorWidth;
            h = AccumulatorHeight;
            return Accumulator;
        }

        #endregion
    }
}
