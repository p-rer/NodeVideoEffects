﻿using System.Windows;
using System.Windows.Media;

namespace NodeVideoEffects.Editor
{
    public class Connector : System.Windows.Controls.Control
    {
        static Connector()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Connector), new FrameworkPropertyMetadata(typeof(Connector)));
        }

        public Point StartPoint
        {
            get => (Point)GetValue(StartPointProperty);
            set => SetValue(StartPointProperty, value);
        }

        public static readonly DependencyProperty StartPointProperty =
            DependencyProperty.Register(nameof(StartPoint), typeof(Point), typeof(Connector), new PropertyMetadata(new Point(0, 0), OnPropertyChanged));

        public Point EndPoint
        {
            get => (Point)GetValue(EndPointProperty);
            set => SetValue(EndPointProperty, value);
        }

        public static readonly DependencyProperty EndPointProperty =
            DependencyProperty.Register(nameof(EndPoint), typeof(Point), typeof(Connector), new PropertyMetadata(new Point(100, 100), OnPropertyChanged));

        public Color StartColor
        {
            get => (Color)GetValue(StartColorProperty);
            init => SetValue(StartColorProperty, value);
        }

        public static readonly DependencyProperty StartColorProperty =
            DependencyProperty.Register(nameof(StartColor), typeof(Color), typeof(Connector), new PropertyMetadata(new Color(), OnPropertyChanged));

        public Color EndColor
        {
            get => (Color)GetValue(EndColorProperty);
            init => SetValue(EndColorProperty, value);
        }

        public static readonly DependencyProperty EndColorProperty =
            DependencyProperty.Register(nameof(EndColor), typeof(Color), typeof(Connector), new PropertyMetadata(new Color(), OnPropertyChanged));

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Connector)d).InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            Pen pen = new(new LinearGradientBrush(
                    StartColor,
                    EndColor,
                    StartPoint,
                    EndPoint
                ), 2);

            var startPoint = StartPoint;
            var endPoint = EndPoint;
            Point controlPoint1;
            Point controlPoint2;
            if (endPoint.X - startPoint.X > 200)
            {
                controlPoint1 = new Point((startPoint.X + endPoint.X) / 2, startPoint.Y);
                controlPoint2 = new Point((startPoint.X + endPoint.X) / 2, endPoint.Y);
            }
            else
            {
                controlPoint1 = new Point(startPoint.X + 100, startPoint.Y);
                controlPoint2 = new Point(endPoint.X - 100, endPoint.Y);
            }

            var geometry = new PathGeometry();
            var figure = new PathFigure { StartPoint = startPoint };
            var bezierSegment = new BezierSegment(controlPoint1, controlPoint2, endPoint, true);
            figure.Segments.Add(bezierSegment);
            geometry.Figures.Add(figure);

            drawingContext.DrawGeometry(null, pen, geometry);
        }
    }
}
