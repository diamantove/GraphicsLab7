using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PolyhedraLab.Geometry;
using PolyhedraLab.Models;
using PolyhedraLab.Rendering;
using PolyhedraLab.UI;

namespace PolyhedraLab;

public partial class MainWindow : Window
{
    private readonly Scene scene = new();
    private readonly Camera camera = new();
    private readonly SceneRenderer renderer = new();
    private WriteableBitmap? bitmap;
    private bool ready;

    public MainWindow()
    {
        InitializeComponent();
        scene.SetModel(PolyhedronFactory.Create(1));
        camera.Fit(scene.Objects);
        void Fit()
        {
            camera.Fit(scene.Objects);
            Draw();
        }
        Panels.Items.Add(new TabItem { Header = "Модели", Content = new ModelPanel(scene, Fit) });
        Panels.Items.Add(new TabItem { Header = "Построение", Content = new ConstructionPanel(scene, Fit) });
        Panels.Items.Add(new TabItem { Header = "Вид и камера", Content = new ViewPanel(camera, renderer, Draw) });
        scene.Changed += Draw;
        Loaded += (_, _) => { ready = true; Draw(); };
        Viewport.SizeChanged += (_, _) => Draw();
    }

    private void Draw()
    {
        if (!ready)
            return;
        // Ограничение разрешения сохраняет отзывчивость программного рендера на 4K.
        int width = Math.Max(1, (int)Viewport.ActualWidth), height = Math.Max(1, (int)Viewport.ActualHeight);
        double scale = Math.Min(1, 1100.0 / Math.Max(width, height));
        width = Math.Max(1, (int)(width * scale));
        height = Math.Max(1, (int)(height * scale));
        renderer.Render(scene.Objects, camera, width, height);
        if (bitmap == null || bitmap.PixelWidth != width || bitmap.PixelHeight != height)
        {
            bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            Picture.Source = bitmap;
        }
        bitmap.WritePixels(new Int32Rect(0, 0, width, height), renderer.Pixels, width * 4, 0);
        Status.Text = $"Объектов: {scene.Objects.Count} | Выбран: {scene.Selected.Name} | Вершин: {scene.Selected.Vertices.Count}, граней: {scene.Selected.Faces.Count}\n" +
            $"После отсечения по нормали: {renderer.VisibleFaces} / {renderer.TotalFaces} | Z-буфер: {(renderer.UseZBuffer ? "вкл." : "выкл.")}";
    }
}
