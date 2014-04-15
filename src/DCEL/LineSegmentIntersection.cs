using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCEL
{
    public class LineSegmentIntersection
    {
        public VecRat2 Position { get; set; }
        public LinkedList<DCEL_HalfEdge> IncidentLineSegments { get; private set; }

        private LineSegmentIntersection(VecRat2 position, IEnumerable<DCEL_HalfEdge> incidentLineSegments)
        {
            Position = position;
            IncidentLineSegments = new LinkedList<DCEL_HalfEdge>(incidentLineSegments);
        }

        private class EventPoint : IComparable<EventPoint>
        {
            public VecRat2 Position { get; set; }
            public DCEL_Vertex Source { get; set; }

            public EventPoint(DCEL_Vertex source)
            {
                Source = source;
                if (source != null)
                    Position = Source.Position;
            }

            public int CompareTo(EventPoint other)
            {
                if (this == other)
                    return 0;

                if (this.Position.Y == other.Position.Y)
                    return this.Position.X.CompareTo(other.Position.X);
                else
                    return other.Position.Y.CompareTo(this.Position.Y);
            }

            public static implicit operator DCEL_Vertex(EventPoint eventPoint)
            {
                return eventPoint.Source;
            }

            public static implicit operator VecRat2(EventPoint eventPoint)
            {
                return eventPoint.Position;
            }

            public override string ToString()
            {
                return Position.ToString();
            }
        }

        private class Segment : IComparable<Segment>
        {
            public EventPoint Upper, Lower;
            public DCEL_HalfEdge Source;
            public Sweepline Sweepline;
            public Segment(DCEL_HalfEdge source, Sweepline sweepline)
            {
                Source = source;
                EventPoint origin = new EventPoint(source.Origin);
                EventPoint destination = new EventPoint(source.Destination);
                int comp = origin.CompareTo(destination);
                if (comp < 0)
                {
                    Upper = origin;
                    Lower = destination;
                }
                else if (comp > 0)
                {
                    Upper = destination;
                    Lower = origin;
                }
                else
                {
                    throw new InvalidOperationException("Trivial line segment.");
                }
                Sweepline = sweepline;
            }

            //correct
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

            //correct
            public Rational Height
            {
                get { return Upper.Position.Y - Lower.Position.Y; }
            }

            public Rational Slope()
            {
                return (Upper.Position.Y - Lower.Position.Y) / (Upper.Position.X - Lower.Position.X);
            }

            //correct, assuming this segment is currently in the status, i.e., is currently intersecting the sweepline
            public Rational SweeplineIntersectionX
            {
                get
                {
                    if (!IntersectsSweepline)
                    {
                        throw new InvalidOperationException();
                    }

                    if (Height == 0)
                        return Sweepline.IncidentEventPoint.Position.X; //Assuming this horizontal segment is currently in the status
                    else
                        return Rational.Lerp(Lower.Position.X, Upper.Position.X, (Sweepline.Y - Lower.Position.Y) / Height);
                }
            }

            //public Rational NormalizedDownwardDirectionX
            //{
            //    get { return Vector2.Normalize(Lower.Position - Upper.Position).X; }
            //}

            //correct, assuming this segment is currently in the status, i.e., is currently intersecting the sweepline
            public static int CompareSweeplineIntersectionToCurrentEventPoint(Segment segment)
            {
                // returns negative if segment intersects left of event point
                // returns zero if segment intersects event point
                // returns positive if segment intersects right of event point
                return segment.SweeplineIntersectionX.CompareTo(segment.Sweepline.IncidentEventPoint.Position.X);
            }

            //correct
            public static bool LowerIsCurrentEventPoint(Segment segment)
            {
                return segment.Lower.CompareTo(segment.Sweepline.IncidentEventPoint) == 0;
            }

            public static bool IntersectsSweeplineLeftOfEventPoint(Segment segment)
            {
                return segment.SweeplineIntersectionX <= segment.Sweepline.IncidentEventPoint.Position.X;
            }

            public static bool IntersectsSweeplineRightOfEventPoint(Segment segment)
            {
                return segment.SweeplineIntersectionX >= segment.Sweepline.IncidentEventPoint.Position.X;
            }

            public SegRat2 ToSegRat2()
            {
                return new SegRat2(Upper.Position, Lower.Position, true);
            }

            ///// <summary>
            ///// Assumes that s1 and s2 are not colinear--or that, if they are, they meet at one of their endpoints and do not overlap elsewhere.
            ///// </summary>
            //public static bool ComputeIntersection(Segment s1, Segment s2, out VecRat2 v)
            //{
            //    int signA = MathAid.TurnTest(s1.Upper, s1.Lower, s2.Upper) * MathAid.TurnTest(s1.Upper, s1.Lower, s2.Lower);
            //    int signB = MathAid.TurnTest(s2.Upper, s2.Lower, s1.Upper) * MathAid.TurnTest(s2.Upper, s2.Lower, s1.Lower);
            //    bool intersect = signA <= 0 && signB <= 0;
            //    if (intersect)
            //    {
            //        if (signA == 0 && signB == 0)
            //        {
            //            //It must be that the two segments meet at an endpoint.
            //            if (s1.Upper.Position == s2.Upper.Position || s1.Upper.Position == s2.Lower.Position)
            //            {
            //                v = s1.Upper.Position;
            //            }
            //            else
            //            {
            //                v = s1.Lower.Position;
            //            }
            //        }
            //        else
            //        {
            //            v = MathAid.LineIntersection(s1.Upper, s1.Lower, s2.Upper, s2.Lower);
            //        }
            //    }
            //    else
            //    {
            //        v = VecRat2.Zero;
            //    }
            //    return intersect;
            //}

            public int CompareTo(Segment other)
            {
                if (!(this.IntersectsSweepline && other.IntersectsSweepline))
                {
                    throw new InvalidOperationException();
                }

                if (this == other)
                    return 0;

                Rational thisX = this.SweeplineIntersectionX;
                Rational otherX = other.SweeplineIntersectionX;
                int comp = thisX.CompareTo(otherX);
                if (comp != 0)
                    return comp;

                //The downward ordering
                Turn turn;
                turn = MathAid.TurnTestDiscrete(this.Upper.Position, new VecRat2(thisX, Sweepline.Y), other.Upper.Position);
                if (turn == Turn.Linear)
                    turn = MathAid.TurnTestDiscrete(this.Lower.Position, new VecRat2(thisX, Sweepline.Y), other.Lower.Position);

                if (turn == Turn.Right)
                    comp = -1;
                else if (turn == Turn.Left)
                    comp = 1;
                else
                    //throw new Exception("Two segments overlapping at more than a single point!");
                    comp = 0;
                //this.NormalizedDownwardDirectionX.CompareTo(other.NormalizedDownwardDirectionX);

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

                comp = this.Upper.Position.Y.CompareTo(other.Upper.Position.Y);

                if (comp != 0)
                    return comp;

                comp = this.Upper.Position.X.CompareTo(other.Upper.Position.X);

                if (comp != 0)
                    return comp;

                comp = other.Lower.Position.Y.CompareTo(this.Lower.Position.Y);

                if (comp != 0)
                    return comp;

                comp = other.Lower.Position.X.CompareTo(this.Lower.Position.X);

                return comp;
            }

            public static implicit operator DCEL_HalfEdge(Segment segment)
            {
                return segment.Source;
            }

            public override string ToString()
            {
                return Source.ToString();
            }
        }

        private class Sweepline
        {
            /// <summary>
            /// The point the sweepline is an epsilon distance before or after.
            /// This starts as null, to indicate that the sweepline has not passed any event point yet.
            /// </summary>
            public EventPoint IncidentEventPoint { get; set; }
            public Rational Y
            {
                get
                {
                    if (IncidentEventPoint == null)
                        throw new InvalidOperationException();
                    else
                        return IncidentEventPoint.Position.Y;
                }
            }
            public bool BeforeEventPoint { get; set; }

            public Sweepline()
            {
                IncidentEventPoint = null;
                BeforeEventPoint = true;
            }
        }

        public static LinkedList<LineSegmentIntersection> FindIntersections(IEnumerable<DCEL_HalfEdge> lineSegments)
        {
            LinkedList<LineSegmentIntersection> result = new LinkedList<LineSegmentIntersection>();

            Sweepline sweepline = new Sweepline();
            RBTreeMap<EventPoint, LinkedList<Segment>> eventQueue = new RBTreeMap<EventPoint, LinkedList<Segment>>((x,y)=>x.CompareTo(y));

            var segments = lineSegments.Where(lineSegment => lineSegment.Origin.Position != lineSegment.Destination.Position)
                .Select(lineSegment => new Segment(lineSegment, sweepline));

            //segments = FixOverlappingSegments(segments);

            foreach (var segment in segments)
            {
                var node = eventQueue.Find(segment.Upper);
                LinkedList<Segment> upperList = (node != null ? node.Value : null);
                if (upperList == null)
                {
                    upperList = new LinkedList<Segment>();
                    eventQueue.Add(new RBTreeMapNode<EventPoint, LinkedList<Segment>>(new EventPoint(segment.Upper), upperList));
                }
                upperList.AddLast(segment);
                
                if (!eventQueue.ContainsKey(segment.Lower))
                {
                    eventQueue.Add(new RBTreeMapNode<EventPoint, LinkedList<Segment>>(new EventPoint(segment.Lower), new LinkedList<Segment>()));
                }
            }

            RBTreeSet<Segment> status = new RBTreeSet<Segment>((x,y)=>x.CompareTo(y));

            while (eventQueue.Count > 0)
            {
                var pair = eventQueue.MinNode;
                eventQueue.RemoveMin();

                EventPoint nextEventPoint = pair.Key;
                LinkedList<Segment> upperList = pair.Value;

                LineSegmentIntersection reportedIntersection;
                HandleEventPoint(nextEventPoint, upperList, sweepline, eventQueue, status, out reportedIntersection);
                if (reportedIntersection != null)
                    result.AddLast(reportedIntersection);
            }

            return result;
        }

        private static void HandleEventPoint(EventPoint eventPoint, LinkedList<Segment> upperList,
            Sweepline sweepline, RBTreeMap<EventPoint, LinkedList<Segment>> eventQueue, RBTreeSet<Segment> status, out LineSegmentIntersection reportedIntersection)
        {
            reportedIntersection = null;

            sweepline.IncidentEventPoint = eventPoint;
            sweepline.BeforeEventPoint = true;

            //1.
            //upperList brought in as part of the input. None of its elements currently intersect the sweepline!

            //2.
            IEnumerable<Segment> middleList, lowerList;
            status.FindRange(Segment.CompareSweeplineIntersectionToCurrentEventPoint).Select(node => node.Key)
                .Partition(Segment.LowerIsCurrentEventPoint, out lowerList, out middleList);

            //3.
            if (upperList.Count + middleList.Count() + lowerList.Count() > 1)
            {
                //4.
                reportedIntersection = new LineSegmentIntersection(
                    eventPoint.Position,
                    upperList.Concat(middleList).Concat(lowerList).Select(segment => segment.Source));
            }

            //5.
            foreach (var lower in lowerList)
                status.Remove(lower);
            foreach (var middle in middleList)
                status.Remove(middle);

            sweepline.BeforeEventPoint = false;

            //6.

            foreach (var middle in middleList)
                status.Add(new RBTreeSetNode<Segment>(middle));

            foreach (var upper in upperList)
                status.Add(new RBTreeSetNode<Segment>(upper));
            

            //7.
            //Just a comment in the book

            //8.
            if (upperList.IsEmpty() && middleList.IsEmpty())
            {
                //9.
                var predecessorNode = status.FindMax(Segment.IntersectsSweeplineLeftOfEventPoint);
                var successorNode = status.FindMin(Segment.IntersectsSweeplineRightOfEventPoint);

                //10.
                if (predecessorNode != null && successorNode != null)
                {
                    FindNewEvent(predecessorNode.Key, successorNode.Key, eventPoint, eventQueue);
                }
            }
            else
            {
                //11.
                var leftmostIntersectingNode = status.FindMin(Segment.IntersectsSweeplineRightOfEventPoint);

                //14.
                var rightmostIntersectingNode = status.FindMax(Segment.IntersectsSweeplineLeftOfEventPoint);

                //12.
                var leftmostPredecessorNode = leftmostIntersectingNode.Predecessor;

                //15.
                var rightmostSuccessorNode = rightmostIntersectingNode.Successor;

                //13.
                if (leftmostPredecessorNode != null)
                {
                    //RemoveFutureEvent(leftmostPredecessor, rightmostIntersecting, eventPoint, eventQueue);
                    FindNewEvent(leftmostPredecessorNode.Key, leftmostIntersectingNode.Key, eventPoint, eventQueue);
                }

                //16.
                if (rightmostSuccessorNode != null)
                {
                    //RemoveFutureEvent(leftmostIntersecting, rightmostSuccessor, eventPoint, eventQueue);
                    FindNewEvent(rightmostIntersectingNode.Key, rightmostSuccessorNode.Key, eventPoint, eventQueue);
                }
            }
        }

        private static void FindNewEvent(Segment s1, Segment s2, EventPoint eventPoint, RBTreeMap<EventPoint, LinkedList<Segment>> eventQueue)
        {
            //If two segments intersect below the sweep line, or on it and
            //to the right of the current event point, and the intersection
            //is not yet present as an event in Q:
            //then insert a new event point in Q for this intersection
            VecRat2 pointIntersection = new VecRat2();
            SegRat2 segmentIntersection = new SegRat2();
            //bool intersectionExists = Segment.ComputeIntersection(s1, s2, out pointIntersection);
            //if (intersectionExists)
            SegmentIntersectionType result = GeomAid.SegmentIntersection(s1.ToSegRat2(), s2.ToSegRat2(), ref pointIntersection, ref segmentIntersection);
            if (result == SegmentIntersectionType.Point)
            {
                EventPoint newEventPoint = new EventPoint(null);
                newEventPoint.Position = pointIntersection;
                if (eventPoint.CompareTo(newEventPoint) < 0 && !eventQueue.ContainsKey(newEventPoint))
                {
                    LinkedList<Segment> upperList = new LinkedList<Segment>();
                    eventQueue.Add(new RBTreeMapNode<EventPoint,LinkedList<Segment>>(newEventPoint, upperList));
                }
            }
            else if (result == SegmentIntersectionType.Segment)
            {
                throw new NotImplementedException("Didn't think this would ever happen?!");
            }
        }

        //public static bool DoRemovingOfFutureEventsOptimization = false;

        //private static void RemoveFutureEvent(Segment s1, Segment s2, EventPoint eventPoint, RBTreeMap<EventPoint, LinkedList<Segment>> eventQueue)
        //{
        //    if (!DoRemovingOfFutureEventsOptimization)
        //        return;

        //    //If two segments intersect below the sweep line, or on it and
        //    //to the right of the current event point, and the intersection
        //    //is already present as an event in Q:
        //    //then remove it
        //    VecRat2 intersection;
        //    bool intersectionExists = Segment.ComputeIntersection(s1, s2, out intersection);
        //    if (intersectionExists)
        //    {
        //        EventPoint newEventPoint = new EventPoint(null);
        //        newEventPoint.Position = intersection;

        //        if (eventPoint.CompareTo(newEventPoint) < 0 && eventQueue.ContainsKey(newEventPoint))
        //        {
        //            eventQueue.Remove(newEventPoint);
        //        }
        //    }
        //}
    }
}
