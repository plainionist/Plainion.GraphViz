﻿using System;
using System.Windows;
using System.Windows.Media;
using Plainion.Graphs;

namespace Plainion.GraphViz.Visuals
{
    abstract class AbstractElementVisual
    {
        private bool myIsSelected;

        public DrawingVisual Visual { get; protected set; }

        public void Select(bool select)
        {
            if (Visual == null)
            {
                return;
            }

            ApplyToGeometries(Visual.Drawing, g => SelectDrawing(g, select));
        }

        protected void ApplyToGeometries(DrawingGroup drawingGroup, Action<GeometryDrawing> action)
        {
            foreach (var drawing in drawingGroup.Children)
            {
                var childGroup = drawing as DrawingGroup;
                if (childGroup != null)
                {
                    ApplyToGeometries(childGroup, action);
                    continue;
                }

                var geometry = drawing as GeometryDrawing;
                if (geometry != null)
                {
                    action(geometry);
                    continue;
                }
            }
        }

        private void SelectDrawing(GeometryDrawing drawing, bool select)
        {
            if (select == myIsSelected)
            {
                return;
            }

            if (select)
            {
                drawing.Brush = Brushes.Red;
                drawing.Pen.Brush = Brushes.Red;
                drawing.Pen.Thickness *= 2;

                myIsSelected = true;
            }
            else
            {
                var brush = GetBorderBrush();
                drawing.Brush = brush;
                drawing.Pen.Brush = brush;
                drawing.Pen.Thickness /= 2;

                myIsSelected = false;
            }
        }

        protected abstract Brush GetBorderBrush();

        public static IGraphItem GetGraphItem(DependencyObject obj)
        {
            return (IGraphItem)obj.GetValue(GraphItemProperty);
        }

        public static void SetGraphItem(DependencyObject obj, IGraphItem item)
        {
            obj.SetValue(GraphItemProperty, item);
        }

        public static readonly DependencyProperty GraphItemProperty = DependencyProperty.RegisterAttached("GraphItem",
            typeof(IGraphItem), typeof(AbstractElementVisual), new FrameworkPropertyMetadata(null));
    }
}
