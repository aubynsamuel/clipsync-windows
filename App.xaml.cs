using System.Windows;
using System.Windows.Media;

namespace ClipSyncWindows
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Force ideal text rendering
            TextOptions.TextFormattingModeProperty.OverrideMetadata(
                typeof(Window),
                new FrameworkPropertyMetadata(TextFormattingMode.Ideal));

            // Use ideal rendering for text
            TextOptions.TextRenderingModeProperty.OverrideMetadata(
                typeof(Window),
                new FrameworkPropertyMetadata(TextRenderingMode.ClearType));

            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.Default;
        }
    }
}