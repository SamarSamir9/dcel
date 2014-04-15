using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCEL
{
    public class OA_Segment : IComparable<OA_Segment>
    {
        /// <summary>
        /// The Upper->Lower source half edges that generated this segment.
        /// During the sweep, it is the portion of the half edge below the sweepline.
        /// </summary>
        public List<OA_Source<DCEL_HalfEdge>> Source { get; private set; }

        public OA_EventPoint Upper { get; set; }
        public OA_EventPoint Lower { get; set; }

        public OA_Sweepline Sweepline { get; set; }

        private OA_Segment()
        {
            Source = new List<OA_Source<DCEL_HalfEdge>>(2);
        }

        public OA_Segment(OA_Sweepline sweepline)
            : this()
        {
            Sweepline = sweepline;
        }

        public void SetEndpoints(OA_EventPoint a, OA_EventPoint b)
        {
            int comp = a.CompareTo(b);
            if (comp == 0)
                throw new InvalidOperationException("Trivial line segment.");

            if (comp < 0)
            {
                Upper = a;
                Lower = b;
            }
            else
            {
                Upper = b;
                Lower = a;
            }
        }

        public bool IntersectsSweepline
        {
            get
            {
                if (Sweepline.BeforeEventPoint)
                    return Upper.CompareTo(Sweepline.IncidentEventPoint) < 0 && Sweepline.IncidentEventPoint.CompareTo(Lower) <= 0;
                else
                    return Upper.CompareTo(Sweepline.IncidentEventPoint) <= 0 && Sweepline.IncidentEventPoint.CompareTo(Lower) < 0;
            }
        }

        public Rational Height
        {
            get { return Upper.Position.Y - Lower.Position.Y; }
        }

        public Rational Slope()
        {
            return (Upper.Position.Y - Lower.Position.Y) / (Upper.Position.X - Lower.Position.X);
        }

        //Assumes this segment is currently in the status.
        public Rational SweeplineIntersectionX
        {
            get
            {
                if (!IntersectsSweepline)
                    throw new InvalidOperationException();

                if (Height == 0)
                    return Sweepline.IncidentEventPoint.Position.X;
                else
                    return Rational.Lerp(Lower.Position.X, Upper.Position.X, (Sweepline.Y - Lower.Position.Y) / Height);
            }
        }

        //correct, assuming this segment is currently in the status, i.e., is currently intersecting the sweepline
        public static int CompareSweeplineIntersectionToCurrentEventPoint(OA_Segment segment)
        {
            // returns negative if segment intersects left of event point
            // returns zero if segment intersects event point
            // returns positive if segment intersects right of event point
            return segment.SweeplineIntersectionX.CompareTo(segment.Sweepline.IncidentEventPoint.Position.X);
        }

        public static bool LowerIsCurrentEventPoint(OA_Segment segment)
        {
            return segment.Lower.CompareTo(segment.Sweepline.IncidentEventPoint) == 0;
        }

        public static bool IntersectsSweeplineLeftOfEventPointInclusive(OA_Segment segment)
        {
            return segment.SweeplineIntersectionX <= segment.Sweepline.IncidentEventPoint.Position.X;
        }

        public static bool IntersectsSweeplineLeftOfEventPointExclusive(OA_Segment segment)
        {
            return segment.SweeplineIntersectionX < segment.Sweepline.IncidentEventPoint.Position.X;
        }

        public static bool IntersectsSweeplineRightOfEventPointInclusive(OA_Segment segment)
        {
            return segment.SweeplineIntersectionX >= segment.Sweepline.IncidentEventPoint.Position.X;
        }

        public SegRat2 ToSegRat2()
        {
            return new SegRat2(Upper.Position, Lower.Position, true);
        }

        public int CompareTo(OA_Segment other)
        {
            if (!(this.IntersectsSweepline && other.IntersectsSweepline))
                throw new InvalidOperationException();

            if (this == other)
                return 0;

            Rational thisX = this.SweeplineIntersectionX;
            Rational otherX = other.SweeplineIntersectionX;
            int comp = thisX.CompareTo(otherX);
            if (comp != 0)
                return comp;

            //The downward ordering
            Turn turn = MathAid.TurnTestDiscrete(this.Upper.Position, new VecRat2(thisX, Sweepline.Y), other.Upper.Position);
            if (turn == Turn.Linear)
                turn = MathAid.TurnTestDiscrete(this.Lower.Position, new VecRat2(thisX, Sweepline.Y), other.Lower.Position);

            if (turn == Turn.Right)
                comp = -1;
            else if (turn == Turn.Left)
                comp = 1;
            else
                //Two segments overlapping at more than a single point
                comp = 0;

            //We know thisX==otherX
            //We want the downward ordering if these segments cross to the left of the current event point
            //However, if:
            //    a) the sweepline is before the event point and these segements pass through the event point
            // or b) these segments cross to the right of the current event point
            //Then we want to reverse this ordering!
            if ((Sweepline.BeforeEventPoint && thisX == Sweepline.IncidentEventPoint.Position.X)
                || thisX > Sweepline.IncidentEventPoint.Position.X)
                comp *= -1;

            if (comp != 0)
                return comp;

            comp = this.Upper.CompareTo(other.Upper);

            if (comp != 0)
                return comp;

            comp = this.Lower.CompareTo(other.Lower);

            return comp;

            //comp = this.Upper.Position.Y.CompareTo(other.Upper.Position.Y);

            //if (comp != 0)
            //    return comp;

            //comp = this.Upper.Position.X.CompareTo(other.Upper.Position.X);

            //if (comp != 0)
            //    return comp;

            //comp = other.Lower.Position.Y.CompareTo(this.Lower.Position.Y);

            //if (comp != 0)
            //    return comp;

            //comp = other.Lower.Position.X.CompareTo(this.Lower.Position.X);

            //return comp;
        }

        public static int Compare(OA_Segment a, OA_Segment b)
        {
            return a.CompareTo(b);
        }

        public static int CompareEndpoints(OA_Segment a, OA_Segment b)
        {
            int comp = a.Upper.CompareTo(b.Upper);
            if (comp != 0)
                return comp;

            return a.Lower.CompareTo(b.Lower);
        }

        public static OA_Segment JoinSource(OA_Segment a, OA_Segment b)
        {
            a.Source.AddRange(b.Source);
            return a;
        }

        public override string ToString()
        {
            return String.Format("{0}->{1}", Upper, Lower);
        }
    }
}
