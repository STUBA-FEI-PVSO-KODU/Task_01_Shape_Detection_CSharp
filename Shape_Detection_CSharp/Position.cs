using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Shape_Detection_CSharp
{
    /// <summary>
    /// Defines a position class.
    /// </summary>
    public class Position
    {
        #region Properties
        /// <summary>
        /// Represents a start point as <see cref="Vector2"/>.
        /// </summary>
        public Vector2 Start { get; private set; }
        /// <summary>
        /// Represents an end point as <see cref="Vector2"/>.
        /// </summary>
        public Vector2 End { get; private set;}
        #endregion
        #region Constructors
        /// <summary>
        /// Creates a new <see cref="Position"/> instance. 
        /// </summary>
        /// <param name="start">Represents a start point as <see cref="Vector2"/>.</param>
        /// <param name="end">Represents an end point as <see cref="Vector2"/>.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public Position(Vector2 start, Vector2 end)
        {
            if(start == Vector2.Zero)
                throw new ArgumentNullException(nameof(start));
            if(end == Vector2.Zero)
                throw new ArgumentNullException(nameof(end));
            Start = start;
            End = end;
        }
        /// <summary>
        /// Creates a new <see cref="Position"/> instance. 
        /// </summary>
        /// <param name="startX">Represents a start point X position.</param>
        /// <param name="startY">Represents a start point Y position.</param>
        /// <param name="endX">Represents a end point X position.</param>
        /// <param name="endY">Represents a end point Y position.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Position(int startX, int startY, int endX, int endY)
        {
            /*if(startX < 0)
                startX = 0;
            if(startY < 0)
                startY = 0;
            if(endX < 0)
                endX = 0;
            if (endY < 0)
                endY = 0;*/
            Start = new Vector2(startX, startY);
            End = new Vector2(endX, endY);
        }
        #endregion
    }
}
