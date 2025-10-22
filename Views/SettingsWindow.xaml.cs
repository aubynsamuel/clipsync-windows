using ClipSyncWindows.ViewModels;
using System.Windows;
using System.Windows.Media.Animation;

namespace ClipSyncWindows.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel vm)
            {
                vm.Save();
            }
            CloseWithAnimation();
        }

        private void CloseWithAnimation()
        {
            var animation = new DoubleAnimation(0, new Duration(TimeSpan.FromSeconds(0.2)));
            animation.Completed += (s, a) => Close();
            BeginAnimation(OpacityProperty, animation);
        }
    }
}
