using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCEL
{
    public static class DCEL_Helper
    {
        public static void JoinIncidentEdge(DCEL_Vertex e_origin, DCEL_HalfEdge e)
        {
            e_origin.IncidentEdge = e;
            e.Origin = e_origin;
        }

        public static void JoinPrevNext(DCEL_HalfEdge e_prev, DCEL_HalfEdge e, DCEL_HalfEdge e_next)
        {
            JoinNext(e_prev, e);
            JoinNext(e, e_next);
        }

        public static void JoinNext(DCEL_HalfEdge e, DCEL_HalfEdge e_next)
        {
            e.Next = e_next;
            e_next.Prev = e;
        }

        public static void JoinTwin(DCEL_HalfEdge e, DCEL_HalfEdge e_twin)
        {
            e.Twin = e_twin;
            e_twin.Twin = e;
        }
    }
}
