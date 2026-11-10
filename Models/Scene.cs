using PolyhedraLab.Geometry;

namespace PolyhedraLab.Models;

public sealed class Scene
{
    public List<Polyhedron> Objects { get; } = [];
    public int SelectedIndex
    {
        get; set;
    }
    public Polyhedron Selected => Objects[SelectedIndex];
    public event Action? Changed;
    public void SetModel(Polyhedron model)
    {
        Objects.Clear();
        Objects.Add(model);
        SelectedIndex = 0;
        Notify();
    }

    public void Notify() => Changed?.Invoke();

    public void OverlapDemo()
    {
        Objects.Clear();
        var a = PolyhedronFactory.Create(1);
        a.Transform(Transforms.Matrix4x4.RotationY(25));
        a.Transform(Transforms.AffineTransforms.Translation(-0.55, 0, 0));
        Objects.Add(a);
        var b = PolyhedronFactory.Create(1);
        b.Transform(Transforms.Matrix4x4.RotationX(25) * Transforms.Matrix4x4.RotationY(-25));
        b.Transform(Transforms.AffineTransforms.Translation(0.65, 0.15, 0.45));
        Objects.Add(b);
        var sphere = SurfaceBuilder.Sphere();
        sphere.Transform(Transforms.AffineTransforms.Scaling(0.7, 0.7, 0.7));
        sphere.Transform(Transforms.AffineTransforms.Translation(0, -0.7, -1));
        Objects.Add(sphere);
        SelectedIndex = 0;
        Notify();
    }
}
