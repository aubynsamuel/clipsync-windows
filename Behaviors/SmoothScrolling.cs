using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ClipSyncWindows.Behaviors
{
    public class SmoothScrolling : DependencyObject
    {
        public static readonly DependencyProperty EnableSmoothScrollingProperty =
            DependencyProperty.RegisterAttached("EnableSmoothScrolling", typeof(bool), typeof(SmoothScrolling), new PropertyMetadata(false, OnEnableSmoothScrollingChanged));

        public static bool GetEnableSmoothScrolling(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableSmoothScrollingProperty);
        }

        public static void SetEnableSmoothScrolling(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableSmoothScrollingProperty, value);
        }

        public static readonly DependencyProperty IsScrollingProperty =
            DependencyProperty.RegisterAttached("IsScrolling", typeof(bool), typeof(SmoothScrolling), new PropertyMetadata(false));

        public static bool GetIsScrolling(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsScrollingProperty);
        }

        public static void SetIsScrolling(DependencyObject obj, bool value)
        {
            obj.SetValue(IsScrollingProperty, value);
        }

        private static void OnEnableSmoothScrollingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ItemsControl itemsControl)
            {
                if ((bool)e.NewValue)
                {
                    itemsControl.Loaded += ItemsControl_Loaded;
                    itemsControl.Unloaded += ItemsControl_Unloaded;
                }
                else
                {
                    itemsControl.Loaded -= ItemsControl_Loaded;
                    itemsControl.Unloaded -= ItemsControl_Unloaded;
                }
            }
            else if (d is ScrollViewer scrollViewer)
            {
                if ((bool)e.NewValue)
                {
                    scrollViewer.PreviewMouseWheel += ScrollViewer_PreviewMouseWheel;
                }
                else
                {
                    scrollViewer.PreviewMouseWheel -= ScrollViewer_PreviewMouseWheel;
                }
            }
        }

        private static void ItemsControl_Loaded(object sender, RoutedEventArgs e)
        {
            var itemsControl = sender as ItemsControl;
            var scrollViewer = GetScrollViewer(itemsControl);
            if (scrollViewer != null)
            {
                scrollViewer.PreviewMouseWheel += ScrollViewer_PreviewMouseWheel;
            }
        }

        private static void ItemsControl_Unloaded(object sender, RoutedEventArgs e)
        {
            var itemsControl = sender as ItemsControl;
            var scrollViewer = GetScrollViewer(itemsControl);
            if (scrollViewer != null)
            {
                scrollViewer.PreviewMouseWheel -= ScrollViewer_PreviewMouseWheel;
            }
        }

        private static ScrollViewer GetScrollViewer(DependencyObject root)
        {
            if (root is ScrollViewer viewer) return viewer;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is ScrollViewer scrollViewer) return scrollViewer;

                var result = GetScrollViewer(child);
                if (result != null) return result;
            }
            return null;
        }

        // Physics State
        private static double _velocity = 0;
        private static double _currentVerticalOffset = 0;
        private static bool _isAnimating = false;
        private static ScrollViewer _activeScrollViewer;
        
        // Constants
        private const double Friction = 0.95; // Decay factor (0.0 to 1.0) - higher is more 'slippery'
        private const double StopThreshold = 0.1; // Velocity below this stops animation
        private const double ScrollSensitivity = 0.05; // Multiplier for mouse wheel delta

        private static void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer == null) return;

            e.Handled = true;

            // Initialize offset on first interaction or if logic drifted
            if (!_isAnimating)
            {
                _currentVerticalOffset = scrollViewer.VerticalOffset;
                _activeScrollViewer = scrollViewer;
                SetIsScrolling(scrollViewer, true);
                CompositionTarget.Rendering += OnUpdate;
                _isAnimating = true;
            }

            // Add velocity (accumulate momentum)
            // e.Delta is usually 120 or -120. 
            // Negative delta = scroll down (positive offset increase)
            // Positive delta = scroll up (negative offset increase)
            double deltaVelocity = -e.Delta * ScrollSensitivity;
            _velocity += deltaVelocity;
        }

        private static void OnUpdate(object sender, EventArgs e)
        {
            if (_activeScrollViewer == null || !_isAnimating)
            {
                StopAnimation();
                return;
            }

            // Apply velocity
            _currentVerticalOffset += _velocity;

            // Apply friction
            _velocity *= Friction;

            // Clamp to bounds
            if (_currentVerticalOffset < 0)
            {
                _currentVerticalOffset = 0;
                _velocity = 0; // Stop hitting top
            }
            else if (_currentVerticalOffset > _activeScrollViewer.ScrollableHeight)
            {
                _currentVerticalOffset = _activeScrollViewer.ScrollableHeight;
                _velocity = 0; // Stop hitting bottom
            }

            // Update ScrollViewer
            _activeScrollViewer.ScrollToVerticalOffset(_currentVerticalOffset);

            // Check if stopped
            if (Math.Abs(_velocity) < StopThreshold)
            {
                StopAnimation();
            }
        }

        private static void StopAnimation()
        {
            if (_isAnimating)
            {
                CompositionTarget.Rendering -= OnUpdate;
                _isAnimating = false;
                _velocity = 0;
                if (_activeScrollViewer != null)
                {
                    SetIsScrolling(_activeScrollViewer, false);
                    _activeScrollViewer = null;
                }
            }
        }
    }
}
