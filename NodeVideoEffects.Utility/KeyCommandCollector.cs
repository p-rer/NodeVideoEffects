using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using YukkuriMovieMaker.Settings;

namespace NodeVideoEffects.Utility;

public static class KeyCommandCollector
{
    public static Dictionary<KeyGestureEx, ICommand> Collect()
    {
        var result = new Dictionary<KeyGestureEx, ICommand>();

        var current = Keyboard.FocusedElement as DependencyObject;
        if (current == null) return result;

        var visited = new HashSet<DependencyObject>();

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        while (current is not null && !visited.Contains(current))
        {
            visited.Add(current);

            CollectFromInputBindings(current, result);
            CollectFromCommandBindings(current, result);

            current = GetParent(current);
        }

        return result;
    }

    private static void CollectFromInputBindings(DependencyObject obj, Dictionary<KeyGestureEx, ICommand> dict)
    {
        if (obj is not UIElement uie) return;
        foreach (var keyBinding in uie.InputBindings.OfType<KeyBinding>())
        {
            if (keyBinding.Gesture is not KeyGestureEx gesture) continue;
            dict.TryAdd(gesture, keyBinding.Command);
        }
    }

    private static void CollectFromCommandBindings(DependencyObject obj, Dictionary<KeyGestureEx, ICommand> dict)
    {
        if (obj is not UIElement uie) return;
        foreach (var cb in uie.CommandBindings.Cast<CommandBinding>())
        {
            if (cb.Command is not RoutedCommand rc) continue;
            foreach (var g in rc.InputGestures.OfType<KeyGestureEx>()) dict.TryAdd(g, rc);
        }
    }

    private static DependencyObject GetParent(DependencyObject obj)
    {
        var visual = VisualTreeHelper.GetParent(obj);
        return visual ?? LogicalTreeHelper.GetParent(obj);
    }
}