using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCEL
{
    public class DCEL_HalfEdge : DCEL_Element
    {
        /// <summary>
        /// A pointer to the origin vertex of this half-edge.
        /// </summary>
        public DCEL_Vertex Origin { get; set; }

        /// <summary>
        /// Provides access to the destination of this half-edge, which is the origin of its twin.
        /// </summary>
        public DCEL_Vertex Destination
        {
            get { return Twin.Origin; }
        }

        /// <summary>
        /// A pointer to the twin half-edge of this half-edge.
        /// </summary>
        public DCEL_HalfEdge Twin { get; set; }

        /// <summary>
        /// A pointer to the face incident to this half-edge.
        /// </summary>
        public DCEL_Face IncidentFace { get; set; }

        /// <summary>
        /// Pointer to the next half-edge in the circular linked list of half-edges defining the boundary of IncidentFace.
        /// </summary>
        public DCEL_HalfEdge Next { get; set; }

        /// <summary>
        /// Pointer to the previous half-edge in the circular linked list of half-edges defining the boundary of IncidentFace.
        /// </summary>
        public DCEL_HalfEdge Prev { get; set; }

        public DCEL_HalfEdge()
            : base()
        {
        }

        public IEnumerable<DCEL_HalfEdge> CycleNext
        {
            get
            {
                DCEL_HalfEdge e = this;
                do
                {
                    yield return e;
                    e = e.Next;
                }
                while (e != this);
            }
        }

        public IEnumerable<DCEL_HalfEdge> CyclePrev
        {
            get
            {
                DCEL_HalfEdge e = this;
                do
                {
                    yield return e;
                    e = e.Prev;
                }
                while (e != this);
            }
        }

        public Rational CycleArea()
        {
            Rational twiceSignedArea = 0;
            VecRat2 origin = Origin.Position;
            foreach (DCEL_HalfEdge halfEdge in CycleNext)
                twiceSignedArea += GeomAid.TriangleTwiceSignedArea(origin, halfEdge.Origin.Position, halfEdge.Destination.Position);
            return Rational.Abs(twiceSignedArea) / 2;
        }

        public override string ToString()
        {
            return String.Format("{0}->{1}", Origin, Destination);
        }

        public override string TypeName
        {
            get { return "HalfEdge"; }
        }

        public override IEnumerable<Tuple<DCEL_Element, String>> IncidentElements
        {
            get
            {
                yield return new Tuple<DCEL_Element, String>(Origin, "Origin");
                yield return new Tuple<DCEL_Element, String>(Destination, "Destination");
                yield return new Tuple<DCEL_Element, String>(Twin, "Twin");
                yield return new Tuple<DCEL_Element, String>(IncidentFace, "IncidentFace");
                yield return new Tuple<DCEL_Element, String>(Next, "Next");
                yield return new Tuple<DCEL_Element, String>(Prev, "Prev");
            }
        }
    }
}
