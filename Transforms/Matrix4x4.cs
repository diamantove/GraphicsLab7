using PolyhedraLab.Models;

namespace PolyhedraLab.Transforms;

/// <summary>Однородные координаты: столбец (x,y,z,1). В A*B сначала действует B.</summary>
public sealed class Matrix4x4
{
    private readonly double[,] values;
    public double this[int row, int column] => values[row, column];

    public Matrix4x4(double[,] values)
    {
        if (values.GetLength(0) != 4 || values.GetLength(1) != 4)
            throw new ArgumentException("Матрица должна иметь размер 4×4.");
        this.values = (double[,])values.Clone();
    }

    public static Matrix4x4 Identity() => new(new double[,] {
        {1,0,0,0}, {0,1,0,0}, {0,0,1,0}, {0,0,0,1} });

    public static Matrix4x4 operator *(Matrix4x4 a, Matrix4x4 b)
    {
        var result = new double[4, 4];
        for (int row = 0; row < 4; row++)
            for (int col = 0; col < 4; col++)
                for (int k = 0; k < 4; k++)
                    result[row, col] += a[row, k] * b[k, col];
        return new Matrix4x4(result);
    }

    public Point3D TransformPoint(Point3D p)
    {
        // Обычное умножение матрицы на столбец (x, y, z, 1).
        double[] input = [p.X, p.Y, p.Z, 1];
        var result = new double[4];
        for (int row = 0; row < 4; row++)
            for (int k = 0; k < 4; k++)
                result[row] += values[row, k] * input[k];
        double w = result[3];
        if (Math.Abs(w) < 1e-12)
            throw new ArgumentException("Невозможно выполнить деление на нулевую однородную координату.");
        // У аффинных преобразований w = 1; у перспективы нужно деление на w.
        return new(result[0] / w, result[1] / w, result[2] / w);
    }

    public static Matrix4x4 RotationX(double degrees)
    {
        double a = degrees * Math.PI / 180, c = Math.Cos(a), s = Math.Sin(a);
        return new(new double[,] { { 1, 0, 0, 0 }, { 0, c, -s, 0 }, { 0, s, c, 0 }, { 0, 0, 0, 1 } });
    }

    public static Matrix4x4 RotationY(double degrees)
    {
        double a = degrees * Math.PI / 180, c = Math.Cos(a), s = Math.Sin(a);
        return new(new double[,] { { c, 0, s, 0 }, { 0, 1, 0, 0 }, { -s, 0, c, 0 }, { 0, 0, 0, 1 } });
    }

    public static Matrix4x4 RotationZ(double degrees)
    {
        double a = degrees * Math.PI / 180, c = Math.Cos(a), s = Math.Sin(a);
        return new(new double[,] { { c, -s, 0, 0 }, { s, c, 0, 0 }, { 0, 0, 1, 0 }, { 0, 0, 0, 1 } });
    }

    public static Matrix4x4 AroundPoint(Point3D center, Matrix4x4 operation) =>
        AffineTransforms.Translation(center.X, center.Y, center.Z) * operation * AffineTransforms.Translation(-center.X, -center.Y, -center.Z);

}
