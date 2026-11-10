namespace PolyhedraLab.Models;

/// <summary>Грань — многоугольник с циклически упорядоченными индексами общих вершин.</summary>
public sealed class Face
{
    public int[] VertexIndices
    {
        get;
    }
    public Point3D Normal
    {
        get; private set;
    }

    public void UpdateNormal(IReadOnlyList<Point3D> vertices)
    {
        // Сумма векторных произведений устойчива к коллинеарным первым вершинам.
        Point3D sum = new();
        Point3D a = vertices[VertexIndices[0]];
        for (int i = 1; i + 1 < VertexIndices.Length; i++)
            sum += Point3D.Cross(vertices[VertexIndices[i]] - a, vertices[VertexIndices[i + 1]] - a);
        Normal = sum.Length > 1e-12 ? sum * (1 / sum.Length) : new();
    }

    public Face(IEnumerable<int> indices)
    {
        VertexIndices = indices.ToArray();
        if (VertexIndices.Length < 3)
            throw new ArgumentException("У грани должно быть хотя бы три вершины.");
    }
}
