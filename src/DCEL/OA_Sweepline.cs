using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCEL
{
    public class OA_Sweepline
    {
        /// <summary>
        /// The point the sweepline is an epsilon distance before or after.
        /// This starts as null, to indicate that the sweepline has not passed any event point yet.
        /// </summary>
        public OA_EventPoint IncidentEventPoint { get; set; }
        public bool BeforeEventPoint { get; set; }

        public RBTreeSet<OA_Segment> Status { get; private set; }

        public Rational Y
        {
            get
            {
                if (IncidentEventPoint == null)
                    throw new InvalidOperationException();

                return IncidentEventPoint.Position.Y;
            }
        }

        public OA_Sweepline()
        {
            IncidentEventPoint = null;
            BeforeEventPoint = true;

            Status = new RBTreeSet<OA_Segment>(OA_Segment.Compare);
        }

        public override string ToString()
        {
            if (IncidentEventPoint == null)
                return "No event point";

            return String.Format("{0} {1}", (BeforeEventPoint ? "Before" : "After"), IncidentEventPoint);
        }
    }
}
