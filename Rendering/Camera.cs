using PolyhedraLab.Models;
using PolyhedraLab.Transforms;

namespace PolyhedraLab.Rendering;

public sealed class Camera
{
    public Point3D Position { get; set; } = new(5, 3.5, 7);
    public Point3D Target
    {
        get; set;
    }
    public bool Perspective { get; set; } = true;
    public double FieldOfView { get; set; } = 55;
    public double ViewSize { get; set; } = 6;
    public double Near { get; } = 0.1;
    public double Far { get; } = 10000;
    public Point3D Direction => Unit(Target - Position);

    private static Point3D Unit(Point3D v)
    {
        if (v.Length < 1e-9)
            throw new ArgumentException("Положение камеры и цель должны различаться.");
        return v * (1 / v.Length);
    }

    public Point3D ToView(Point3D p)
    {
        Point3D forward = Direction;
        Point3D up = Math.Abs(forward.Y) > 0.99 ? new(0, 0, 1) : new(0, 1, 0);
        Point3D right = Unit(Point3D.Cross(forward, up));
        up = Point3D.Cross(right, forward);
        Point3D delta = p - Position;
        return new(Point3D.Dot(delta, right), Point3D.Dot(delta, up), Point3D.Dot(delta, forward));
    }

    public Matrix4x4 Projection(double aspect)
    {
        if (Perspective)
        {
            double f = 1 / Math.Tan(FieldOfView * Math.PI / 360);
            return new(new double[,] { {f/aspect,0,0,0}, {0,f,0,0},
                {0,0,Far/(Far-Near),-Near*Far/(Far-Near)}, {0,0,1,0} });
        }
        return new(new double[,] { {2/(ViewSize*aspect),0,0,0}, {0,2/ViewSize,0,0},
            {0,0,1/(Far-Near),-Near/(Far-Near)}, {0,0,0,1} });
    }

    public void Orbit(double degrees)
    {
        Position = Matrix4x4.AroundPoint(Target, Matrix4x4.RotationY(degrees)).TransformPoint(Position);
    }

    public void Fit(IEnumerable<Polyhedron> models)
    {
        var points = models.SelectMany(m => m.Vertices).ToArray();
        if (points.Length == 0)
            return;
        Target = Point3D.Average(points);
        double radius = Math.Max(0.5, points.Max(p => (p - Target).Length));
        Position = Target + new Point3D(0.5, 0.35, 0.8) * (radius * 3.6);
        ViewSize = radius * 2.8;
    }
}
