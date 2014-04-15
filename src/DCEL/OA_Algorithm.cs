using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace DCEL
{
    public class OA_Algorithm
    {
        /// <summary>
        /// Merge a collection of subdivisions into a single subdivision, destroying the input subdivisions in the process.
        /// </summary>
        public static DCEL_Subdivision Overlay(IEnumerable<DCEL_Subdivision> subdivisions)
            //Func<DCEL_Vertex, DCEL_Vertex, DCEL_Vertex> vertexJoinTag,
            //Func<DCEL_HalfEdge, DCEL_HalfEdge, DCEL_HalfEdge> halfEdgeJoinTag,
            //Func<DCEL_Face, DCEL_Face, DCEL_Face> faceJoinTag,
            //Func<DCEL_HalfEdge, DCEL_Vertex> halfEdgeSplitTag,
            //Func<DCEL_Face, DCEL_HalfEdge> faceSplitTag)
        {
            if (subdivisions.IsEmpty())
                throw new InvalidOperationException("Must input at least one Subdivision.");

            while (subdivisions.CountGreaterThan(1))
                subdivisions = subdivisions.CollapsePairs(OA_Algorithm.Overlay).ToList();

            return subdivisions.First();
        }

        /// <summary>
        /// Merge subdivisionA and subdivisionB into a single subdivision, destroying subdivisionA and subdivisionB in the process.
        /// </summary>
        public static DCEL_Subdivision Overlay(DCEL_Subdivision subdivisionA, DCEL_Subdivision subdivisionB)
        {
            OA_Algorithm algorithm = new OA_Algorithm(new[] { subdivisionA, subdivisionB });

            subdivisionA.WriteToFile("subdivisionA_pre_splitting.xml");
            subdivisionB.WriteToFile("subdivisionB_pre_splitting.xml");

            subdivisionA.WriteToFile_Faces("subdivisionA_pre_splitting_faces.xml");
            subdivisionB.WriteToFile_Faces("subdivisionB_pre_splitting_faces.xml");

            algorithm.phase = Phase.SplittingPhase;

            algorithm.Initialize();
            algorithm.Sweep();

            subdivisionA.WriteToFile("subdivisionA_post_splitting.xml");
            subdivisionB.WriteToFile("subdivisionB_post_splitting.xml");

            subdivisionA.WriteToFile_Faces("subdivisionA_post_splitting_faces.xml");
            subdivisionB.WriteToFile_Faces("subdivisionB_post_splitting_faces.xml");

            algorithm.phase = Phase.MergingPhase;

            algorithm.Initialize();
            algorithm.Sweep();

            algorithm.outputSubdivision.WriteToFile("outputSubdivision_post_merging.xml");

            algorithm.phase = Phase.FacingPhase;

            algorithm.CreateFaces();

            algorithm.outputSubdivision.WriteToFile("outputSubdivision_final.xml");
            algorithm.outputSubdivision.WriteToFile_Faces("outputSubdivision_final_faces.xml");

            //The inputSubdivisions have been destroyed, so clear them.
            foreach (DCEL_Subdivision inputSubdivision in algorithm.inputSubdivisions)
                inputSubdivision.Clear();

            return algorithm.outputSubdivision;
        }

        
        private List<DCEL_Subdivision> inputSubdivisions;
        private DCEL_Subdivision outputSubdivision;

        private Phase phase;

        private OA_Sweepline sweepline;
        private RBTreeSet<OA_EventPoint> eventQueue;

        private RBTreeMap<DCEL_Vertex, DCEL_HalfEdge> leftPicksMap;

        private enum Phase
        {
            SplittingPhase,
            MergingPhase,
            FacingPhase
        }

        private OA_Algorithm(IEnumerable<DCEL_Subdivision> inputSubdivisions)
        {
            this.inputSubdivisions = new List<DCEL_Subdivision>(inputSubdivisions);

            this.leftPicksMap = new RBTreeMap<DCEL_Vertex, DCEL_HalfEdge>(DCEL_Element.Compare);
        }

        private void Initialize()
        {
            sweepline = new OA_Sweepline();
            eventQueue = new RBTreeSet<OA_EventPoint>(OA_EventPoint.Compare);
            outputSubdivision = new DCEL_Subdivision();

            foreach (DCEL_Subdivision inputSubdivision in inputSubdivisions)
            {
                foreach (DCEL_HalfEdge inputHalfEdge in inputSubdivision.HalfEdges.Keys)
                {
                    OA_EventPoint upper = new OA_EventPoint(inputHalfEdge.Origin.Position);
                    OA_EventPoint lower = new OA_EventPoint(inputHalfEdge.Destination.Position);

                    //We want to take only one of the twins from every pair of half edges.
                    if (upper.CompareTo(lower) > 0)
                        continue;

                    RBTreeSetNode<OA_EventPoint> upper_node = new RBTreeSetNode<OA_EventPoint>(upper);
                    RBTreeSetNode<OA_EventPoint> lower_node = new RBTreeSetNode<OA_EventPoint>(lower);

                    //If we didn't add the newly created event point, it already existed.
                    if (!eventQueue.Add(ref upper_node)) upper = upper_node.Key;
                    if (!eventQueue.Add(ref lower_node)) lower = lower_node.Key;

                    OA_Segment segment = new OA_Segment(sweepline);
                    segment.Source.Add(new OA_Source<DCEL_HalfEdge>(inputSubdivision, inputHalfEdge));
                    segment.Upper = upper;
                    segment.Lower = lower;
                    //***May be adding a duplicate if segment is in both subdivisions!
                    upper.UpperList.Add(segment);

                    if (!upper.Source.Any(OA_Source<DCEL_Vertex>.IsFrom(inputSubdivision)))
                        upper.Source.Add(new OA_Source<DCEL_Vertex>(inputSubdivision, inputHalfEdge.Origin));

                    if (!lower.Source.Any(OA_Source<DCEL_Vertex>.IsFrom(inputSubdivision)))
                        lower.Source.Add(new OA_Source<DCEL_Vertex>(inputSubdivision, inputHalfEdge.Destination));
                }
            }

            foreach (OA_EventPoint eventPoint in eventQueue.Keys)
            {
                //***Remove those duplicates here, joining their source lists.
                eventPoint.UpperList.RemoveDuplicates(OA_Segment.CompareEndpoints, OA_Segment.JoinSource);
            }
        }

        private void Sweep()
        {
            while (eventQueue.Count > 0)
                HandleEventPoint(eventQueue.RemoveMin().Key);
        }

        private void AssertValidSweeplineStatus()
        {
            foreach (OA_Segment segment in sweepline.Status.Keys)
                Debug.Assert(segment.IntersectsSweepline);

            sweepline.Status.Keys.ConsecutivePairs((a, b) =>
                Debug.Assert(a.SweeplineIntersectionX <= b.SweeplineIntersectionX));
        }

        private void HandleEventPoint(OA_EventPoint eventPoint)
        {
            AssertValidSweeplineStatus();

            //Sweep to before the event point
            sweepline.IncidentEventPoint = eventPoint;
            sweepline.BeforeEventPoint = true;

            AssertValidSweeplineStatus();

            if (phase == Phase.MergingPhase)
                SetLeftPick(eventPoint);

            //Determine all segments that contain the event point, and partition them
            List<OA_Segment> upperList = eventPoint.UpperList;
            List<OA_Segment> middleList;
            List<OA_Segment> lowerList;
            sweepline.Status
                .FindRange(OA_Segment.CompareSweeplineIntersectionToCurrentEventPoint).Select(node => node.Key)
                .Partition(OA_Segment.LowerIsCurrentEventPoint, out lowerList, out middleList);

            //upperList gets added, middleList gets reversed, lowerList gets removed
            EventPointTransition(upperList, middleList, lowerList);

            AssertValidSweeplineStatus();

            switch (phase)
            {
                case Phase.SplittingPhase:
                    HandleIntersection_SplittingPhase(eventPoint, upperList, middleList, lowerList);
                    break;
                case Phase.MergingPhase:
                    HandleIntersection_MergingPhase(eventPoint, upperList, middleList, lowerList);
                    break;
                default:
                    throw new Exception();
            }

            AssertValidSweeplineStatus();

            if (upperList.IsEmpty() && middleList.IsEmpty())
            {
                RBTreeSetNode<OA_Segment> predecessorNode = sweepline.Status.FindMax(OA_Segment.IntersectsSweeplineLeftOfEventPointInclusive);
                RBTreeSetNode<OA_Segment> successorNode = sweepline.Status.FindMin(OA_Segment.IntersectsSweeplineRightOfEventPointInclusive);
                if (predecessorNode != null && successorNode != null)
                {
                    FindNewEventPoint(predecessorNode.Key, successorNode.Key, eventPoint);
                }
            }
            else
            {
                var leftmostIntersectingNode = sweepline.Status.FindMin(OA_Segment.IntersectsSweeplineRightOfEventPointInclusive);
                var leftmostPredecessorNode = leftmostIntersectingNode.Predecessor;
                if (leftmostPredecessorNode != null)
                {
                    FindNewEventPoint(leftmostPredecessorNode.Key, leftmostIntersectingNode.Key, eventPoint);
                }

                var rightmostIntersectingNode = sweepline.Status.FindMax(OA_Segment.IntersectsSweeplineLeftOfEventPointInclusive);
                var rightmostSuccessorNode = rightmostIntersectingNode.Successor;
                if (rightmostSuccessorNode != null)
                {
                    FindNewEventPoint(rightmostIntersectingNode.Key, rightmostSuccessorNode.Key, eventPoint);
                }
            }
        }

        private void SetLeftPick(OA_EventPoint eventPoint)
        {
            RBTreeSetNode<OA_Segment> leftPick_node = sweepline.Status.FindMax(OA_Segment.IntersectsSweeplineLeftOfEventPointExclusive);
            if (leftPick_node != null)
                eventPoint.LeftPick = leftPick_node.Key;
            else
                eventPoint.LeftPick = null;
        }

        private Comparison<OA_Segment> AngleComparisonLowerList(VecRat2 origin)
        {
            return delegate(OA_Segment a, OA_Segment b)
            {
                return GeomAid.TurnTest(origin, b.Upper.Position, a.Upper.Position);
            };
        }

        private Comparison<OA_Segment> AngleComparisonUpperList(VecRat2 origin)
        {
            return delegate(OA_Segment a, OA_Segment b)
            {
                return GeomAid.TurnTest(origin, b.Lower.Position, a.Lower.Position);
            };
        }

        /// <summary>
        /// Compared HalfEdges should have Origin = origin and Destination > origin.
        /// </summary>
        private Func<DCEL_HalfEdge, DCEL_HalfEdge, int> AngleComparisonUpperList_HalfEdge(VecRat2 origin)
        {
            return delegate(DCEL_HalfEdge a, DCEL_HalfEdge b)
            {
                return GeomAid.TurnTest(origin, b.Destination.Position, a.Destination.Position);
            };
        }

        private Dictionary<DCEL_Subdivision, DCEL_Vertex> CreateSourceLookup(OA_EventPoint eventPoint)
        {
            Dictionary<DCEL_Subdivision, DCEL_Vertex> sourceLookup = new Dictionary<DCEL_Subdivision, DCEL_Vertex>(2);

            foreach (OA_Source<DCEL_Vertex> source in eventPoint.Source)
            {
                sourceLookup.Add(source.Subdivision, source.Element);
            }

            foreach (DCEL_Subdivision inputSubdivision in inputSubdivisions)
            {
                if (!sourceLookup.ContainsKey(inputSubdivision))
                    sourceLookup.Add(inputSubdivision, null);
            }

            return sourceLookup;
        }

        private void HandleIntersection_SplittingPhase(OA_EventPoint eventPoint, List<OA_Segment> upperList, List<OA_Segment> middleList, List<OA_Segment> lowerList)
        {
            //if (upperList.Count() + middleList.Count() + lowerList.Count() <= 1)
            if (middleList.Count == 0)
                return;

            //If there are any subdivisions which don't have a vertex at this location,
            //then we will create one momentarily. For the moment, set the
            //(key, value) = (subdivision, null).
            Dictionary<DCEL_Subdivision, DCEL_Vertex> vertexLookup = CreateSourceLookup(eventPoint);

            //Make all segments meet at a vertex here, within each subdivision.
            //HalfEdges associated with segments in the middleList will be transformed
            //into HalfEdges associated with segments in the upperList. (With the associated
            //segments transforming as well.)
            foreach (OA_Segment middle in middleList)
            {
                foreach (OA_Source<DCEL_HalfEdge> source in middle.Source)
                {
                    DCEL_Subdivision subdivision = source.Subdivision;
                    DCEL_HalfEdge halfEdge = source.Element;
                    DCEL_Vertex vertex = vertexLookup[subdivision];

                    //Create the vertex on-demand if it doesn't exist
                    if (vertex == null)
                    {
                        vertex = new DCEL_Vertex(eventPoint.Position);
                        vertexLookup[subdivision] = vertex;
                        subdivision.Vertices.Add(new RBTreeSetNode<DCEL_Vertex>(vertex));
                    }

                    SplitEdge(subdivision, halfEdge, vertex);

                    if (!eventPoint.Source.Any(OA_Source<DCEL_Vertex>.IsFrom(subdivision)))
                        eventPoint.Source.Add(new OA_Source<DCEL_Vertex>(subdivision, vertex));
                }
            }
        }

        private void HandleIntersection_MergingPhase(OA_EventPoint eventPoint, List<OA_Segment> upperList, List<OA_Segment> middleList, List<OA_Segment> lowerList)
        {
            if (middleList.Count != 0)
                throw new Exception("MergingPhase: Something went wrong in the SplittingPhase, because there is an intersection with nonempty middleList.");

            //Dictionary<DCEL_Subdivision, DCEL_Vertex> vertexLookup = CreateSourceLookup(eventPoint);
            //DCEL_Vertex mergedVertex = vertexLookup.Values.First();

            DCEL_Vertex mergedVertex = eventPoint.Source.First().Element;

            lowerList.Sort(AngleComparisonLowerList(mergedVertex.Position));
            upperList.Sort(AngleComparisonUpperList(mergedVertex.Position));

            //JoinNext all consecutive pairs around circle, and join origins to mergedVertex
            {
                //Outgoing half edges in CCW order
                List<DCEL_HalfEdge> bothList = new List<DCEL_HalfEdge>();
                bothList.AddRange(lowerList.Select(segment => segment.Source.First().Element.Twin));
                bothList.AddRange(upperList.Select(segment => segment.Source.First().Element));

                if (bothList.Count > 1)
                {
                    DCEL_HalfEdge e, e_next;
                    for (int i = 1; i < bothList.Count; i++)
                    {
                        e = bothList[i].Twin;
                        e_next = bothList[i - 1];
                        DCEL_Helper.JoinNext(e, e_next);
                    }

                    e = bothList.First().Twin;
                    e_next = bothList.Last();
                    DCEL_Helper.JoinNext(e, e_next);
                }
                else
                {
                    DCEL_HalfEdge e = bothList[0].Twin;
                    DCEL_HalfEdge e_next = e.Twin;
                    DCEL_Helper.JoinNext(e, e_next);
                }

                //Set the origins to the mergedVertex
                foreach (DCEL_HalfEdge e in bothList)
                    e.Origin = mergedVertex;
                mergedVertex.IncidentEdge = bothList.First();
            }

            outputSubdivision.Vertices.Add(new RBTreeSetNode<DCEL_Vertex>(mergedVertex));

            foreach (OA_Segment upper in upperList)
            {
                DCEL_HalfEdge e = upper.Source.First().Element;
                outputSubdivision.HalfEdges.Add(new RBTreeSetNode<DCEL_HalfEdge>(e));
                outputSubdivision.HalfEdges.Add(new RBTreeSetNode<DCEL_HalfEdge>(e.Twin));

                //HACK?/////////////////////////////////////////
                e.IncidentFace = null;
                e.Twin.IncidentFace = null;
                //HACK?/////////////////////////////////////////
            }

            //Store the left pick for the mergedVertex
            if (eventPoint.LeftPick != null)
                leftPicksMap.Add(new RBTreeMapNode<DCEL_Vertex, DCEL_HalfEdge>(mergedVertex, eventPoint.LeftPick.Source.First().Element));
            else
                leftPicksMap.Add(new RBTreeMapNode<DCEL_Vertex, DCEL_HalfEdge>(mergedVertex, null));
        }

        /// <summary>
        /// halfEdge should be Upper->Lower so that the persistent half edges become:
        ///  o halfEdge         (Upper->Lower) ==> (vertex->Lower)
        ///  o halfEdge.Twin    (Lower->Upper) ==> (Lower->vertex)
        /// And the two newly created half edges are (vertex->Upper) and (Upper->vertex).
        /// </summary>
        private static void SplitEdge(DCEL_Subdivision subdivision, DCEL_HalfEdge halfEdge, DCEL_Vertex vertex)
        {
            Debug.Assert(VecRat2.CompareReadingOrder(halfEdge.Origin.Position, halfEdge.Destination.Position) < 0);

            DCEL_HalfEdge e1 = halfEdge;
            DCEL_HalfEdge e2 = e1.Twin;

            DCEL_HalfEdge e1_prev = e1.Prev;
            DCEL_HalfEdge e2_next = e2.Next;

            DCEL_Vertex e1_origin = e1.Origin;

            DCEL_HalfEdge e1_top = new DCEL_HalfEdge();
            DCEL_HalfEdge e2_top = new DCEL_HalfEdge();

            DCEL_Helper.JoinTwin(e1_top, e2_top);
            e1_top.IncidentFace = e1.IncidentFace;
            e2_top.IncidentFace = e2.IncidentFace;

            DCEL_Helper.JoinIncidentEdge(vertex, e2_top);
            DCEL_Helper.JoinIncidentEdge(vertex, e1);
            DCEL_Helper.JoinIncidentEdge(e1_origin, e1_top);

            if (e2_next == e1)
            {
                DCEL_Helper.JoinNext(e2, e2_top);
                DCEL_Helper.JoinNext(e2_top, e1_top);
                DCEL_Helper.JoinNext(e1_top, e1);
            }
            else
            {
                DCEL_Helper.JoinPrevNext(e2, e2_top, e2_next);
                DCEL_Helper.JoinPrevNext(e1_prev, e1_top, e1);
            }

            if (subdivision != null)
            {
                subdivision.HalfEdges.Add(new RBTreeSetNode<DCEL_HalfEdge>(e1_top));
                subdivision.HalfEdges.Add(new RBTreeSetNode<DCEL_HalfEdge>(e2_top));
            }
        }

        private void EventPointTransition(List<OA_Segment> upperList, List<OA_Segment> middleList, List<OA_Segment> lowerList)
        {
            //Move the sweepline from before the event point to after the event point
            foreach (OA_Segment lower in lowerList)
                sweepline.Status.Remove(lower);
            foreach (OA_Segment middle in middleList)
                sweepline.Status.Remove(middle);

            sweepline.BeforeEventPoint = false;

            foreach (OA_Segment middle in middleList)
                sweepline.Status.Add(new RBTreeSetNode<OA_Segment>(middle));
            foreach (OA_Segment upper in upperList)
                sweepline.Status.Add(new RBTreeSetNode<OA_Segment>(upper));
        }

        private void FindNewEventPoint(OA_Segment segment1, OA_Segment segment2, OA_EventPoint eventPoint)
        {
            VecRat2 pointIntersection = new VecRat2();
            SegRat2 segmentIntersection = new SegRat2();

            SegmentIntersectionType result = GeomAid.SegmentIntersection(segment1.ToSegRat2(), segment2.ToSegRat2(), ref pointIntersection, ref segmentIntersection);
            if (result == SegmentIntersectionType.Point)
            {
                OA_EventPoint newEventPoint = new OA_EventPoint(pointIntersection);
                if (eventPoint.CompareTo(newEventPoint) < 0)
                {
                    //Add the new event point if it isn't already in the event queue
                    eventQueue.Add(new RBTreeSetNode<OA_EventPoint>(newEventPoint));
                }
            }
            else if (result == SegmentIntersectionType.Segment)
            {
                throw new NotImplementedException("Didn't think this would ever happen?!");
            }
        }

        private enum CycleType
        {
            Interior,
            Exterior
        }

        private static CycleType GetCycleType(DCEL_HalfEdge cycle)
        {
            if (GeomAid.TurnTest(cycle.Prev.Origin.Position, cycle.Origin.Position, cycle.Destination.Position) > 0)
                return CycleType.Interior;
            else
                return CycleType.Exterior;
        }

        private class CycleData
        {
            public bool IsRoot { get; set; }
            public CycleType CycleType { get; set; }
            public List<DCEL_HalfEdge> Children { get; private set; }
            public CycleData(CycleType cycleType)
            {
                IsRoot = true;
                CycleType = cycleType;
                Children = new List<DCEL_HalfEdge>(0);
            }
        }

        private class HalfEdgeData
        {
            public DCEL_HalfEdge Cycle { get; set; }
            public bool Visited { get { return Cycle != null; } }
            public HalfEdgeData()
            {
                Cycle = null;
            }
        }

        private void CreateFaces()
        {
            RBTreeMap<DCEL_HalfEdge, HalfEdgeData> halfEdgesData = new RBTreeMap<DCEL_HalfEdge, HalfEdgeData>(DCEL_Element.Compare);

            foreach (DCEL_HalfEdge halfEdge in outputSubdivision.HalfEdges.Keys)
                halfEdgesData.Add(new RBTreeMapNode<DCEL_HalfEdge,HalfEdgeData>(halfEdge, new HalfEdgeData()));

            RBTreeMap<DCEL_HalfEdge, CycleData> cyclesData = new RBTreeMap<DCEL_HalfEdge, CycleData>(DCEL_Element.Compare);

            List<bool> visited = outputSubdivision.HalfEdges.Keys.Select(e => false).ToList();

            DCEL_Face unboundedFace = new DCEL_Face();
            outputSubdivision.UnboundedFace = unboundedFace;
            outputSubdivision.Faces.Add(new RBTreeSetNode<DCEL_Face>(unboundedFace));

            foreach (var halfEdgeNode in halfEdgesData.Nodes)
            {
                DCEL_HalfEdge halfEdge = halfEdgeNode.Key;
                if (halfEdgeNode.Value.Visited)
                    continue;

                DCEL_HalfEdge cycle = halfEdge.CycleNext.Min((a, b) => a.Origin.CompareTo(b.Origin));
                {
                    cycle = cycle.CycleNext.Where(e => e.Origin == cycle.Origin).Max(AngleComparisonUpperList_HalfEdge(cycle.Origin.Position));
                }

                CycleType cycleType = GetCycleType(cycle);

                if (cycleType == CycleType.Interior)
                {
                    DCEL_Face interiorFace = new DCEL_Face();
                    outputSubdivision.Faces.Add(new RBTreeSetNode<DCEL_Face>(interiorFace));

                    interiorFace.OuterComponent = cycle;
                    foreach (DCEL_HalfEdge cycleHalfEdge in cycle.CycleNext)
                        cycleHalfEdge.IncidentFace = interiorFace;
                }

                cyclesData.Add(new RBTreeMapNode<DCEL_HalfEdge, CycleData>(cycle, new CycleData(cycleType)));

                foreach (DCEL_HalfEdge cycleHalfEdge in halfEdge.CycleNext)
                {
                    HalfEdgeData data = halfEdgesData[cycleHalfEdge].Value;
                    data.Cycle = cycle;
                }
            }

            foreach (var cycleNode in cyclesData.Nodes)
            {
                if (cycleNode.Value.CycleType == CycleType.Interior)
                    continue;

                DCEL_HalfEdge cycle = cycleNode.Key;

                DCEL_HalfEdge leftPick = leftPicksMap[cycle.Origin].Value;
                if (leftPick == null)
                {
                    unboundedFace.InnerComponents.AddLast(cycle);
                    foreach (DCEL_HalfEdge cycleHalfEdge in cycle.CycleNext)
                        cycleHalfEdge.IncidentFace = unboundedFace;
                    continue;
                }

                DCEL_HalfEdge leftPickCycle = halfEdgesData[leftPick].Value.Cycle;
                CycleType leftPickCycleType = cyclesData[leftPickCycle].Value.CycleType;

                //if (leftPickCycleType == CycleType.Interior)
                //{
                //    leftPickCycle.IncidentFace.InnerComponents.AddLast(cycle);
                //    foreach (DCEL_HalfEdge cycleHalfEdge in cycle.CycleNext)
                //        cycleHalfEdge.IncidentFace = leftPickCycle.IncidentFace;
                //}

                cyclesData[leftPickCycle].Value.Children.Add(cycle);
                cycleNode.Value.IsRoot = false;
            }



            foreach (var cycleChildrenNode in cyclesData.Nodes)
            {
                if (!cycleChildrenNode.Value.IsRoot)
                    continue;

                DCEL_HalfEdge parentCycle = cycleChildrenNode.Key;
                CycleType parentCycleType = cyclesData[parentCycle].Value.CycleType;

                DCEL_Face face = parentCycle.IncidentFace;

                List<DCEL_HalfEdge> childCycles = cycleChildrenNode.Value.Children;

                Stack<DCEL_HalfEdge> unattachedCycles = new Stack<DCEL_HalfEdge>();
                foreach (DCEL_HalfEdge childCycle in childCycles)
                    unattachedCycles.Push(childCycle);

                while (!unattachedCycles.IsEmpty())
                {
                    DCEL_HalfEdge exteriorCycle = unattachedCycles.Pop();
                    face.InnerComponents.AddLast(exteriorCycle);
                    foreach (DCEL_HalfEdge exteriorCycleHalfEdge in exteriorCycle.CycleNext)
                        exteriorCycleHalfEdge.IncidentFace = face;
                    foreach (DCEL_HalfEdge child in cyclesData[exteriorCycle].Value.Children)
                        unattachedCycles.Push(child);
                }
            }
        }
    }
}
