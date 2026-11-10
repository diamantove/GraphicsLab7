namespace PolyhedraLab.Transforms;

public static class AffineTransforms
{
    public static Matrix4x4 Translation(double x, double y, double z) => new(new double[,] {
        {1,0,0,x}, {0,1,0,y}, {0,0,1,z}, {0,0,0,1} });

    public static Matrix4x4 Scaling(double x, double y, double z) => new(new double[,] {
        {x,0,0,0}, {0,y,0,0}, {0,0,z,0}, {0,0,0,1} });

    public static Matrix4x4 Reflection(int plane) => plane switch
    {
        0 => Scaling(1, 1, -1),
        1 => Scaling(1, -1, 1),
        2 => Scaling(-1, 1, 1),
        _ => throw new ArgumentException("Выберите плоскость отражения.")
    };
}
