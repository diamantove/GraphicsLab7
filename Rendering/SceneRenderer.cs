using PolyhedraLab.Models;

namespace PolyhedraLab.Rendering;

public sealed class SceneRenderer
{
    public bool CullBackFaces { get; set; } = true;
    public bool UseZBuffer { get; set; } = true;
    public int VisibleFaces
    {
        get; private set;
    }
    public int TotalFaces
    {
        get; private set;
    }
    public int[] Pixels { get; private set; } = [];
    public double[] Depth { get; private set; } = [];
    private int width, height;

    public void Render(IReadOnlyList<Polyhedron> models, Camera camera, int w, int h)
    {
        width = Math.Max(1, w);
        height = Math.Max(1, h);
        if (Pixels.Length != width * height)
        {
            Pixels = new int[width * height];
            Depth = new double[width * height];
        }
        Array.Fill(Pixels, unchecked((int)0xFFF3F5F8));
        Array.Fill(Depth, double.PositiveInfinity);
        VisibleFaces = 0;
        TotalFaces = 0;
        var projection = camera.Projection((double)width / height);
        int[] colors = [0x589CDB, 0xE58B50, 0x70B777, 0xB984CF, 0xD3B349];
        for (int m = 0; m < models.Count; m++)
        {
            var model = models[m];
            var view = model.Vertices.Select(camera.ToView).ToArray();
            foreach (var face in model.Faces)
            {
                TotalFaces++;
                if (face.Normal.Length < 1e-12)
                    continue;
                Point3D center = Point3D.Average(face.VertexIndices.Select(i => model.Vertices[i]).ToArray());
                Point3D toEye = camera.Perspective ? camera.Position - center : camera.Direction * -1;
                if (CullBackFaces && !model.TwoSided && Point3D.Dot(face.Normal, toEye) <= 0)
                    continue;
                VisibleFaces++;
                double light = 0.3 + 0.7 * Math.Abs(Point3D.Dot(face.Normal, new Point3D(0.36, 0.8, 0.48)));
                int color = Shade(colors[m % colors.Length], light);
                foreach (var (a, b, c) in Triangulator.Triangulate(face, model.Vertices))
                {
                    // Отсекаем до перспективного деления, в координатах камеры.
                    var polygon = Clip([view[a], view[b], view[c]], camera.Near, true);
                    polygon = Clip(polygon, camera.Far, false);
                    if (polygon.Count < 3)
                        continue;
                    var screen = polygon.Select(p => Screen(projection.TransformPoint(p))).ToArray();
                    for (int i = 1; i + 1 < screen.Length; i++)
                        Triangle(screen[0], screen[i], screen[i + 1], color);
                }
            }
        }
    }

    public static List<Point3D> Clip(List<Point3D> points, double plane, bool keepGreater)
    {
        var result = new List<Point3D>();
        if (points.Count == 0)
            return result;
        Point3D a = points[^1];
        bool Inside(Point3D p) => keepGreater ? p.Z >= plane : p.Z <= plane;
        foreach (var b in points)
        {
            bool insideA = Inside(a), insideB = Inside(b);
            if (insideA != insideB)
            {
                double t = (plane - a.Z) / (b.Z - a.Z);
                result.Add(a + (b - a) * t);
            }
            if (insideB)
                result.Add(b);
            a = b;
        }
        return result;
    }

    private Point3D Screen(Point3D p) => new((p.X + 1) * width / 2, (1 - p.Y) * height / 2, p.Z);
    private static double Edge(Point3D a, Point3D b, double x, double y) => (b.X - a.X) * (y - a.Y) - (b.Y - a.Y) * (x - a.X);

    private void Triangle(Point3D a, Point3D b, Point3D c, int color)
    {
        double area = Edge(a, b, c.X, c.Y);
        if (Math.Abs(area) < 1e-12)
            return;
        int x0 = (int)Math.Clamp(Math.Floor(Math.Min(a.X, Math.Min(b.X, c.X))), 0, width - 1);
        int x1 = (int)Math.Clamp(Math.Ceiling(Math.Max(a.X, Math.Max(b.X, c.X))), 0, width - 1);
        int y0 = (int)Math.Clamp(Math.Floor(Math.Min(a.Y, Math.Min(b.Y, c.Y))), 0, height - 1);
        int y1 = (int)Math.Clamp(Math.Ceiling(Math.Max(a.Y, Math.Max(b.Y, c.Y))), 0, height - 1);
        for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
            {
                double u = Edge(b, c, x + 0.5, y + 0.5) / area;
                double v = Edge(c, a, x + 0.5, y + 0.5) / area;
                double t = 1 - u - v;
                if (u < -1e-10 || v < -1e-10 || t < -1e-10)
                    continue;
                // Интерполируется глубина ПОСЛЕ деления на w: она линейна на экране.
                double z = u * a.Z + v * b.Z + t * c.Z;
                int index = y * width + x;
                if (!UseZBuffer || z < Depth[index])
                {
                    Depth[index] = z;
                    Pixels[index] = color;
                }
            }
    }

    private static int Shade(int rgb, double light)
    {
        int r = Math.Clamp((int)((rgb >> 16 & 255) * light), 0, 255);
        int g = Math.Clamp((int)((rgb >> 8 & 255) * light), 0, 255);
        int b = Math.Clamp((int)((rgb & 255) * light), 0, 255);
        return unchecked((int)0xFF000000) | r << 16 | g << 8 | b;
    }
}
