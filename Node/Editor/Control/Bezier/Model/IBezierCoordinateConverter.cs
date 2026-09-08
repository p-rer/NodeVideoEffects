using System.Windows;

namespace Node.Editor.Control.Bezier.Model;

public interface IBezierCoordinateConverter
{
    Point ToScreen(Point point);

    Point FromScreen(Point point);
}