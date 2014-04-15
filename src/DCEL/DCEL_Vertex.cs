using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCEL
{
    public class DCEL_Vertex : DCEL_Element, IComparable<DCEL_Vertex>
    {
        /// <summary>
        /// The position of this vertex.
        /// </summary>
        public VecRat2 Position { get; set; }

        /// <summary>
        /// A pointer to an arbitrary half-edge that has this vertex as its origin.
        /// </summary>
        public DCEL_HalfEdge IncidentEdge { get; set; }

        public DCEL_Vertex(VecRat2 position)
            : base()
        {
            Position = position;
            IncidentEdge = null;
        }

        public int CompareTo(DCEL_Vertex other)
        {
            return VecRat2.CompareReadingOrder(this.Position, other.Position);
        }

        public override string ToString()
        {
            return Position.ToString();
        }

        public IEnumerable<DCEL_HalfEdge> IncidentEdges
        {
            get
            {
                DCEL_HalfEdge e = IncidentEdge;
                do
                {
                    yield return e;
                    e = e.Twin.Next;
                }
                while (e != IncidentEdge);
            }
        }

        public override string TypeName
        {
            get { return "Vertex"; }
        }

        public override IEnumerable<Tuple<DCEL_Element, String>> IncidentElements
        {
            get
            {
                return IncidentEdges.Select(e => new Tuple<DCEL_Element, String>(e, "IncidentEdge"));
            }
        }
    }
}
