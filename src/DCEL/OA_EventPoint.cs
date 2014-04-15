using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCEL
{
    public class OA_EventPoint : IComparable<OA_EventPoint>
    {
        /// <summary>
        /// The vertices that generated this event point.
        /// </summary>
        public List<OA_Source<DCEL_Vertex>> Source { get; private set; }
        
        public VecRat2 Position { get; set; }

        /// <summary>
        /// The first segment that is hit by a ray shot from this event point to the left.
        /// </summary>
        public OA_Segment LeftPick { get; set; }

        /// <summary>
        /// When the sweepline reaches this event point, what segments will be joining the status?
        /// </summary>
        public List<OA_Segment> UpperList { get; private set; }

        private OA_EventPoint()
        {
            Source = new List<OA_Source<DCEL_Vertex>>(2);
            UpperList = new List<OA_Segment>(4);
            LeftPick = null;
        }

        public OA_EventPoint(VecRat2 position)
            : this()
        {
            Position = position;
        }

        public int CompareTo(OA_EventPoint other)
        {
            if (this == other)
                return 0;

            //Decreasing Y
            int comp = other.Position.Y.CompareTo(this.Position.Y);
            if (comp != 0)
                return comp;

            //Increasing X
            return this.Position.X.CompareTo(other.Position.X);
        }

        public static int Compare(OA_EventPoint a, OA_EventPoint b)
        {
            return a.CompareTo(b);
        }

        public override string ToString()
        {
            return Position.ToString();
        }
    }
}
