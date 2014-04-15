using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCEL.Scripts
{
    public static class SubdivisionRenderer
    {
        public static void Render(DCEL_Subdivision subdivision, Action<Bitmap> action)
        {
            const float scale = 100;

            var vertices = subdivision.Vertices.Keys;
            var xmin = vertices.Select(x => (float)x.Position.X).Min() - 1;
            var xmax = vertices.Select(x => (float)x.Position.X).Max() + 1;
            var ymin = vertices.Select(x => (float)x.Position.Y).Min() - 1;
            var ymax = vertices.Select(x => (float)x.Position.Y).Max() + 1;



            using (var bitmap = new Bitmap((int)(scale * (xmax - xmin)), (int)(scale * (ymax - ymin))))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.ScaleTransform(1, -1);
                    graphics.ScaleTransform(scale, scale);
                    graphics.TranslateTransform(-xmin, -ymax);

                    graphics.SmoothingMode = SmoothingMode.AntiAlias;

                    graphics.Clear(Color.LightGray);

                    using (var subdivisionGraphics = new SubdivisionGraphics(graphics, scale))
                    {
                        foreach (var face in subdivision.Faces.Keys)//.Where(f => f.Bounded))
                        {
                            subdivisionGraphics.DrawFace(face);
                        }

                        foreach (var edge in subdivision.HalfEdges.Keys.Where(e => e.Origin.CompareTo(e.Destination) > 0))
                        {
                            subdivisionGraphics.DrawHalfEdge(edge);
                        }

                        foreach (var vertex in subdivision.Vertices.Keys)
                        {
                            subdivisionGraphics.DrawVertex(vertex);
                        }
                    }
                }

                action(bitmap);
            }
        }
    }
}
