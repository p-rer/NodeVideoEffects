﻿using System.Windows;
using System.Windows.Controls;

namespace NodeVideoEffects
{
    public enum DockLocation
    {
        TopLeft,
        Top,
        TopRight,

        Left,
        Center,
        Right,

        BottomLeft,
        Bottom,
        BottomRight
    }

    public class DockablePanel : ContentControl
    {
        static DockablePanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockablePanel),
                new FrameworkPropertyMetadata(typeof(DockablePanel)));
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                "Title",
                typeof(string),
                typeof(DockablePanel),
                new PropertyMetadata(string.Empty));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty DockLocationProperty =
            DependencyProperty.Register(
                "DockLocation",
                typeof(DockLocation),
                typeof(DockablePanel),
                new PropertyMetadata(DockLocation.Center));

        public DockLocation DockLocation
        {
            get => (DockLocation)GetValue(DockLocationProperty);
            set => SetValue(DockLocationProperty, value);
        }

        public static readonly DependencyProperty PriorityProperty =
            DependencyProperty.Register(
                "Priority",
                typeof(int),
                typeof(DockablePanel),
                new PropertyMetadata(0));

        public int Priority
        {
            get => (int)GetValue(PriorityProperty);
            set => SetValue(PriorityProperty, value);
        }
    }
}
