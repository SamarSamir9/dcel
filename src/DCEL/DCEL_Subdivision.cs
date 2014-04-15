using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace DCEL
{
    public class DCEL_Subdivision
    {
        private static int NextID = 0;

        public int ID { get; private set; }
        public RBTreeSet<DCEL_Vertex> Vertices { get; private set; }
        public RBTreeSet<DCEL_HalfEdge> HalfEdges { get; private set; }
        public RBTreeSet<DCEL_Face> Faces { get; private set; }

        public DCEL_Face UnboundedFace { get; set; }

        public DCEL_Subdivision()
        {
            ID = NextID++;
            Vertices = new RBTreeSet<DCEL_Vertex>(DCEL_Element.Compare);
            HalfEdges = new RBTreeSet<DCEL_HalfEdge>(DCEL_Element.Compare);
            Faces = new RBTreeSet<DCEL_Face>(DCEL_Element.Compare);
            UnboundedFace = new DCEL_Face();
        }

        /// <summary>
        /// Create a subdivision from a single segment (u, v).
        /// </summary>
        public DCEL_Subdivision(VecRat2 u, VecRat2 v)
            : this()
        {
            if (u == v)
                throw new Exception("Tried to create a DCELSubdivision with a segment of length 0.");

            DCEL_Vertex vertex_u = new DCEL_Vertex(u);
            DCEL_Vertex vertex_v = new DCEL_Vertex(v);
            DCEL_HalfEdge halfedge_uv = new DCEL_HalfEdge();
            DCEL_HalfEdge halfedge_vu = new DCEL_HalfEdge();
            DCEL_Face face = new DCEL_Face();

            vertex_u.IncidentEdge = halfedge_uv;
            vertex_v.IncidentEdge = halfedge_vu;

            halfedge_uv.Origin = vertex_u;
            halfedge_uv.Twin = halfedge_vu;
            halfedge_uv.IncidentFace = face;
            halfedge_uv.Prev = halfedge_vu;
            halfedge_uv.Next = halfedge_vu;

            halfedge_vu.Origin = vertex_v;
            halfedge_vu.Twin = halfedge_uv;
            halfedge_vu.IncidentFace = face;
            halfedge_vu.Prev = halfedge_uv;
            halfedge_vu.Next = halfedge_uv;

            face.InnerComponents.AddLast(halfedge_uv);

            Vertices.Add(new RBTreeSetNode<DCEL_Vertex>(vertex_u));
            Vertices.Add(new RBTreeSetNode<DCEL_Vertex>(vertex_u));
            HalfEdges.Add(new RBTreeSetNode<DCEL_HalfEdge>(halfedge_uv));
            HalfEdges.Add(new RBTreeSetNode<DCEL_HalfEdge>(halfedge_vu));
            Faces.Add(new RBTreeSetNode<DCEL_Face>(face));

            UnboundedFace = face;
        }

        public static DCEL_Subdivision MakeClosedPolygon(

        public override string ToString()
        {
            return String.Format("Subdivision[{0}]", ID);
        }

        public void Clear()
        {
            Vertices.Clear();
            HalfEdges.Clear();
            Faces.Clear();
            UnboundedFace = null;
        }

        public void WriteToFile(String filename)
        {
            IEnumerable<DCEL_Element> elements =
                Vertices.Keys.Cast<DCEL_Element>()
                .Concat(HalfEdges.Keys.Cast<DCEL_Element>())
                .Concat(Faces.Keys.Cast<DCEL_Element>());

            using (XmlWriter writer = XmlWriter.Create(filename))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Elements");
                foreach (DCEL_Element element in elements)
                {
                    writer.WriteStartElement(element.TypeName);
                    writer.WriteAttributeString("Element", element.ToString());
                    int maxLength = element.IncidentElements.Max(tuple => tuple.Item2.Length);
                    foreach (Tuple<DCEL_Element, String> incidentElement in element.IncidentElements)
                    {
                        writer.WriteStartElement("Incident");
                        writer.WriteAttributeString("Element", String.Format("{0} {1}",
                            incidentElement.Item2.PadRight(maxLength).Replace(' ', '.'),
                            (incidentElement.Item1 != null ? incidentElement.Item1.ToString() : "NULL")));
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        public void WriteToFile_Faces(String filename)
        {
            using (XmlWriter writer = XmlWriter.Create(filename))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Faces");
                foreach (DCEL_Face face in Faces.Keys)
                {
                    writer.WriteStartElement(face.TypeName);
                    writer.WriteAttributeString("Element", face.ToString());

                    if (face.OuterComponent != null)
                    {
                        writer.WriteStartElement("OuterComponent");
                        foreach (DCEL_HalfEdge halfEdge in face.OuterComponent.CycleNext)
                        {
                            writer.WriteStartElement(halfEdge.TypeName);
                            writer.WriteAttributeString("Element", halfEdge.ToString());
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }

                    foreach (DCEL_HalfEdge innerComponent in face.InnerComponents)
                    {
                        writer.WriteStartElement("InnerComponent");
                        foreach (DCEL_HalfEdge halfEdge in innerComponent.CycleNext)
                        {
                            writer.WriteStartElement(halfEdge.TypeName);
                            writer.WriteAttributeString("Element", halfEdge.ToString());
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }
    }
}
