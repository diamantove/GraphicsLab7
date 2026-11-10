namespace PolyhedraLab.Models;

/// <summary>Точка или вектор в правой декартовой системе координат (Y вверх).</summary>
public readonly record struct Point3D(double X, double Y, double Z)
{
    public static Point3D operator +(Point3D a, Point3D b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Point3D operator -(Point3D a, Point3D b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Point3D operator *(Point3D p, double k) => new(p.X * k, p.Y * k, p.Z * k);
    public double Length => Math.Sqrt(Dot(this, this));
    public bool IsFinite => double.IsFinite(X) && double.IsFinite(Y) && double.IsFinite(Z);

    public static Point3D Average(IReadOnlyList<Point3D> points)
    {
        if (points.Count == 0)
            throw new ArgumentException("Нельзя найти центр пустого набора точек.");
        Point3D sum = new();
        foreach (Point3D point in points)
            sum += point;
        return sum * (1.0 / points.Count);
    }

    public static double Dot(Point3D a, Point3D b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    public static Point3D Cross(Point3D a, Point3D b) => new(
        a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
}
