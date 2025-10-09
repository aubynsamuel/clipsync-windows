
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Windows.Media.Effects;

namespace ClipSyncWindows.Services
{

    public static class NotificationHelper
    {
        public static void ShowSimpleNotification(string title, string message)
        {
            try
            {
                var theme = ThemeManager.Instance;

                // Create window shell
                var toast = new Window
                {
                    Width = 300,
                    Height = 100,
                    WindowStyle = WindowStyle.None,
                    AllowsTransparency = true,
                    Background = Brushes.Transparent,
                    ShowInTaskbar = false,
                    Topmost = true,
                    ShowActivated = false,
                    Opacity = 0 // start invisible for fade-in
                };

                // Build content with rounded-corner card + shadow - theme aware
                var border = new Border
                {
                    CornerRadius = new CornerRadius(8),
                    Background = theme.CardBackground,
                    Padding = new Thickness(12),
                    Effect = new DropShadowEffect
                    {
                        Color = theme.IsDarkTheme ? Colors.Black : Colors.Gray,
                        BlurRadius = 10,
                        ShadowDepth = 0,
                        Opacity = theme.IsDarkTheme ? 0.4 : 0.2
                    }
                };

                var stack = new StackPanel();

                stack.Children.Add(new TextBlock
                {
                    Text = title,
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 14,
                    Foreground = theme.PrimaryText,
                    Margin = new Thickness(0, 0, 0, 4)
                });

                stack.Children.Add(new TextBlock
                {
                    Text = message,
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 12,
                    Foreground = theme.SecondaryText
                });

                border.Child = stack;
                toast.Content = border;

                // Position at bottom-right of the primary screen's work area
                var wa = SystemParameters.WorkArea;
                toast.Left = wa.Right - toast.Width - 16;
                toast.Top = wa.Bottom - toast.Height - 16;

                // Fade+slide in
                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
                toast.BeginAnimation(UIElement.OpacityProperty, fadeIn);

                var slideIn = new DoubleAnimation(wa.Bottom + toast.Height, toast.Top,
                                    TimeSpan.FromMilliseconds(300))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                toast.BeginAnimation(Window.TopProperty, slideIn);

                toast.Show();

                // Auto‑close after delay with fade‑out
                var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
                timer.Tick += (s, e) =>
                {
                    timer.Stop();

                    var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
                    fadeOut.Completed += (_, __) => toast.Close();
                    toast.BeginAnimation(UIElement.OpacityProperty, fadeOut);
                };
                timer.Start();
            }
            catch
            {
                // best-effort only
            }
        }
    }
}
