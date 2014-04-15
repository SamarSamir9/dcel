using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DCEL.Scripts
{
    public class SubdivisionGraphics : IDisposable
    {
        private readonly Graphics graphics;
        private readonly Brush brush;
        private readonly Pen pen;
        private readonly Func<Color> colorGenerator;
        private readonly float vertexRadius;

        private static readonly List<Color> colors = typeof(Color)
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(x => x.PropertyType == typeof(Color))
            .Select(x => x.GetValue(null))
            .Cast<Color>()
            .ToList();

        public SubdivisionGraphics(Graphics graphics, float scale = 100)
        {
            this.graphics = graphics;
            this.brush = new SolidBrush(Color.Black);
            this.pen = new Pen(brush, 1 / scale);

            int nextColorIndex = 0;
            colorGenerator = () => colors[nextColorIndex++ % colors.Count];

            this.vertexRadius = 2 / scale;
        }

        private PointF P(DCEL_Vertex v)
        {
            return new PointF((float)v.Position.X, (float)v.Position.Y);
        }

        public void DrawFace(DCEL_Face face)
        {
            using (var gp = new GraphicsPath())
            {
                if (face.OuterComponent != null)
                {
                    gp.AddPolygon(face.OuterComponent.CycleNext.Select(e => P(e.Origin)).ToArray());
                }
                foreach (var inner in face.InnerComponents)
                {
                    gp.AddPolygon(inner.CycleNext.Select(e => P(e.Origin)).ToArray());
                }
                using (var colorBrush = new SolidBrush(colorGenerator()))
                {
                    graphics.FillPath(colorBrush, gp);
                }
            }
        }

        public void DrawHalfEdge(DCEL_HalfEdge edge)
        {
            graphics.DrawLine(pen, P(edge.Origin), P(edge.Destination));
        }

        public void DrawVertex(DCEL_Vertex vertex)
        {
            graphics.FillEllipse(brush, (float)vertex.Position.X - vertexRadius, (float)vertex.Position.Y - vertexRadius, 0.04f, 0.04f);
        }

        public void Dispose()
        {
            pen.Dispose();
            brush.Dispose();
        }
    }
}
