using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace PolyhedraLab.UI;

public static class Ui
{
    public static TextBox Input(Panel panel, string label, string value)
    {
        Label(panel, label);
        var box = new TextBox { Text = value };
        panel.Children.Add(box);
        return box;
    }

    public static void Label(Panel panel, string text) => panel.Children.Add(new TextBlock
    {
        Text = text,
        TextWrapping = TextWrapping.Wrap,
        Margin = new Thickness(3, 7, 3, 2)
    });

    public static ComboBox Choice(Panel panel, string label, params string[] items)
    {
        Label(panel, label);
        var box = new ComboBox { ItemsSource = items, SelectedIndex = 0 };
        panel.Children.Add(box);
        return box;
    }

    public static CheckBox Check(Panel panel, string label, bool value)
    {
        var box = new CheckBox { Content = label, IsChecked = value };
        panel.Children.Add(box);
        return box;
    }

    public static void Button(Panel panel, string title, Action action)
    {
        var button = new Button { Content = title };
        button.Click += (_, _) => Guard(action);
        panel.Children.Add(button);
    }

    public static void Guard(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or OverflowException or System.IO.IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(ex.Message, "Проверьте параметры", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    public static double Number(string text)
    {
        if (!double.TryParse(text.Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double number) || !double.IsFinite(number))
            throw new ArgumentException($"«{text}» — не число.");
        return number;
    }

    public static int Integer(TextBox box)
    {
        double value = Number(box.Text);
        if (value != Math.Truncate(value) || value < 1 || value > 100000)
            throw new ArgumentException("Введите положительное целое число.");
        return (int)value;
    }

    public static double[] Numbers(TextBox box, int count)
    {
        var parts = box.Text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != count)
            throw new ArgumentException($"Введите {count} чисел через пробел.");
        return parts.Select(Number).ToArray();
    }

    public static ScrollViewer Scroll(StackPanel panel) => new()
    {
        Content = panel,
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
    };
}
