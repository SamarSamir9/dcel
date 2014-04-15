using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCEL
{
    public class DCEL_Face : DCEL_Element
    {
        /// <summary>
        /// A pointer to some half-edge on the outer boundary of this face.
        /// For an unbounded face, this pointer is null.
        /// </summary>
        public DCEL_HalfEdge OuterComponent { get; set; }

        /// <summary>
        /// A list of pointers to arbitrary half-edges on the boundaries of holes in this face.
        /// There is exactly one pointer for each hole.
        /// </summary>
        public LinkedList<DCEL_HalfEdge> InnerComponents { get; private set; }

        /// <summary>
        /// Returns whether this face is bounded, that is whether OuterComponent is not null.
        /// </summary>
        public bool Bounded
        {
            get { return OuterComponent != null; }
        }

        public DCEL_Face()
            : base()
        {
            OuterComponent = null;
            InnerComponents = new LinkedList<DCEL_HalfEdge>();
        }

        public override string TypeName
        {
            get { return "Face"; }
        }

        public IEnumerable<DCEL_HalfEdge> IncidentHalfEdges
        {
            get
            {
                var query = InnerComponents.SelectMany(inner => inner.CycleNext);
                if (Bounded)
                    query = query.Concat(OuterComponent.CycleNext);
                return query;
            }
        }

        public Rational Area()
        {
            if (Bounded)
                return OuterComponent.CycleArea() - InnerComponents.Aggregate<DCEL_HalfEdge, Rational>(0, (area, inner) => area + inner.CycleArea());
            else
                return 0;
        }

        public IEnumerable<DCEL_Face> AdjacentFaces()
        {
            RBTreeSet<DCEL_Face> faceSet = new RBTreeSet<DCEL_Face>(DCEL_Element.Compare);
            foreach (DCEL_HalfEdge halfEdge in IncidentHalfEdges)
                if (halfEdge.Twin.IncidentFace != this)
                    faceSet.Add(new RBTreeSetNode<DCEL_Face>(halfEdge.Twin.IncidentFace));
            return faceSet.Keys;
        }

        public override IEnumerable<Tuple<DCEL_Element, String>> IncidentElements
        {
            get
            {
                if (OuterComponent != null)
                {
                    foreach (DCEL_HalfEdge e in OuterComponent.CycleNext)
                    {
                        yield return new Tuple<DCEL_Element, String>(e, "OuterComponent");
                    }
                }

                int i = 0;
                foreach (DCEL_HalfEdge inner in InnerComponents)
                {
                    foreach (DCEL_HalfEdge e in inner.CycleNext)
                    {
                        yield return new Tuple<DCEL_Element, String>(e, String.Format("InnerComponent[{0}]", i));
                    }
                    i++;
                }
            }
        }

        public override string ToString()
        {
            return String.Format("Face[{0}]", ID);
        }
    }
}
