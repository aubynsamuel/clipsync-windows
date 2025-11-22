using InTheHand.Net.Sockets;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using ClipSyncWindows.Models;
using ClipSyncWindows.Services;
using ClipSyncWindows.ViewModels;

namespace ClipSyncWindows.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly ObservableCollection<BluetoothDeviceInfo> _devices = [];
        private BluetoothListener? _listener;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isServiceRunning = false;
        private static readonly Guid ServiceUuid = new("8ce255c0-200a-11e0-ac64-0800200c9a66");
        
        // Clipboard monitoring for auto-sync
        private System.Windows.Threading.DispatcherTimer? _clipboardMonitorTimer;
        private string? _lastClipboardContent;
        private DateTime _lastAutoSyncTime = DateTime.MinValue;
        private bool _isReceivingClipboard = false;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            _viewModel.CloseSettingsRequested += (s, e) => CloseSettings();
            DataContext = _viewModel;
            DevicesListView.ItemsSource = _devices;

            RefreshDevicesButton.Click += RefreshDevicesButton_Click;
            StartServiceButton.Click += StartServiceButton_Click;
            StopServiceButton.Click += StopServiceButton_Click;
            ShareButton.Click += ShareButton_Click;
            ThemeToggleButton.Click += ThemeToggleButton_Click;
            SettingsButton.Click += SettingsButton_Click;

            LoadPairedDevices();

            // Event handler for selection change
            DevicesListView.SelectionChanged += (s, e) =>
            {
                ShareButton.IsEnabled = DevicesListView.SelectedItems.Count > 0;
            };

            // Handle Escape key to close settings
            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Escape && SettingsOverlay.Visibility == Visibility.Visible)
                {
                    CloseSettings();
                }
            };

            // Initialize clipboard monitoring timer
            _clipboardMonitorTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _clipboardMonitorTimer.Tick += ClipboardMonitor_Tick;
            
            // Load settings and start monitoring if enabled
            var settings = SettingsService.LoadSettings();
            if (settings.AutoSyncEnabled)
            {
                _clipboardMonitorTimer.Start();
            }
        }

        private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ToggleTheme();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var storyboard = (System.Windows.Media.Animation.Storyboard)FindResource("OpenSettingsStoryboard");
            storyboard.Begin();
        }

        private void SettingsOverlay_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Only close if clicking the overlay background, not the sidebar itself
            if (e.OriginalSource == sender)
            {
                CloseSettings();
            }
        }

        private void CloseSettings()
        {
            var storyboard = (System.Windows.Media.Animation.Storyboard)FindResource("CloseSettingsStoryboard");
            storyboard.Begin();
        }

        private void RefreshDevicesButton_Click(object sender, RoutedEventArgs e)
        {
            LoadPairedDevices();
        }

        private void LoadPairedDevices()
        {
            try
            {
                StatusTextBlock.Text = "Loading paired devices...";
                _devices.Clear();

                using var client = new BluetoothClient();
                var devices = client.PairedDevices;

                foreach (var dev in devices)
                {
                    _devices.Add(dev);
                }

                StatusTextBlock.Text = $"Paired devices loaded: {_devices.Count}";
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Error loading devices: {ex.Message}";
                MessageBox.Show($"Failed to load Bluetooth devices: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void StartServiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isServiceRunning) return;

            try
            {
                StartServiceButton.IsEnabled = false;
                RefreshDevicesButton.IsEnabled = false;
                StopServiceButton.IsEnabled = true;

                StatusTextBlock.Text = "Starting service...";

                _cancellationTokenSource = new CancellationTokenSource();
                var token = _cancellationTokenSource.Token;

                _listener = new BluetoothListener(ServiceUuid);
                _listener.Start();
                _isServiceRunning = true;

                StatusTextBlock.Text = "Service: Listening for bluetooth devices...";

                await Task.Run(() => ListeningLoop(token), token);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting Bluetooth service: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StopListeningService();
            }
        }

        private void StopServiceButton_Click(object sender, RoutedEventArgs e)
        {
            StopListeningService();
        }

        private void StopListeningService()
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                _listener?.Stop();
                _isServiceRunning = false;

                Dispatcher.Invoke(() =>
                {
                    StartServiceButton.IsEnabled = true;
                    RefreshDevicesButton.IsEnabled = true;
                    StopServiceButton.IsEnabled = false;
                    StatusTextBlock.Text = "Service stopped";
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    StatusTextBlock.Text = $"Error stopping service: {ex.Message}";
                });
            }
        }

        private void ListeningLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_listener == null)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }

                    using var client = _listener.AcceptBluetoothClient();
                    Dispatcher.Invoke(() =>
                    {
                        StatusTextBlock.Text = $"Connected to: {client.RemoteMachineName}";
                    });

                    using var stream = client.GetStream();
                    using var reader = new StreamReader(stream);

                    var jsonText = reader.ReadLine() ?? "";

                    try
                    {
                        var clipboardData = JsonConvert.DeserializeObject<ClipboardData>(jsonText);

                        if (clipboardData?.Clip != null)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                _isReceivingClipboard = true;
                                Clipboard.SetText(clipboardData.Clip);
                                _lastClipboardContent = clipboardData.Clip;
                                StatusTextBlock.Text = "Received clipboard text & copied!";
                                NotificationHelper.ShowSimpleNotification("ClipSync", $"ClipText Received: \n {TruncateText(clipboardData.Clip, 50)}");
                                _isReceivingClipboard = false;
                            });
                        }
                    }
                    catch (JsonException)
                    {
                        if (!string.IsNullOrEmpty(jsonText))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                Clipboard.SetText(jsonText);
                                StatusTextBlock.Text = "Received raw text & copied!";
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (!token.IsCancellationRequested)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            StatusTextBlock.Text = $"Listener error: {ex.Message}";
                        });
                        Thread.Sleep(1000);
                    }
                }
            }
        }

        private async void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Clipboard.ContainsText())
            {
                StatusTextBlock.Text = "Clipboard is empty!";
                return;
            }

            var clipboardText = Clipboard.GetText();
            if (string.IsNullOrEmpty(clipboardText))
            {
                StatusTextBlock.Text = "Clipboard is empty!";
                return;
            }

            StatusTextBlock.Text = "Sending clipboard...";
            ShareButton.IsEnabled = false;

            var selectedDevices = DevicesListView.SelectedItems.Cast<BluetoothDeviceInfo>().ToList();
            int successCount = 0;

            foreach (var device in selectedDevices)
            {
                try
                {
                    var clipData = new ClipboardData
                    {
                        Clip = clipboardText,
                        Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()
                    };

                    var jsonData = JsonConvert.SerializeObject(clipData);

                    await Task.Run(() =>
                    {
                        using var client = new BluetoothClient();
                        client.Connect(device.DeviceAddress, ServiceUuid);

                        using var stream = client.GetStream();
                        using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                        writer.Write(jsonData + "\n");
                        Thread.Sleep(500);
                        client.Close();
                    });

                    successCount++;
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        StatusTextBlock.Text = $"Error sending to {device.DeviceName}: {ex.Message}";
                    });
                }
            }

            ShareButton.IsEnabled = true;
            if (successCount > 0)
            {
                StatusTextBlock.Text = $"Clipboard shared with {successCount}/{selectedDevices.Count} devices";
            }
        }

        private void ClipboardMonitor_Tick(object? sender, EventArgs e)
        {
            var settings = SettingsService.LoadSettings();
            
            // Stop monitoring if auto-sync is disabled
            if (!settings.AutoSyncEnabled)
            {
                _clipboardMonitorTimer?.Stop();
                return;
            }

            // Skip if we're currently receiving clipboard from another device
            if (_isReceivingClipboard) return;

            // Skip if no devices are selected
            if (DevicesListView.SelectedItems.Count == 0) return;

            try
            {
                if (!Clipboard.ContainsText()) return;

                var currentClipboard = Clipboard.GetText();
                
                // Skip empty or whitespace-only content
                if (string.IsNullOrWhiteSpace(currentClipboard)) return;

                // Check if clipboard has changed
                if (currentClipboard != _lastClipboardContent)
                {
                    // Debounce: ensure at least 500ms since last auto-sync
                    var timeSinceLastSync = DateTime.Now - _lastAutoSyncTime;
                    if (timeSinceLastSync.TotalMilliseconds >= 500)
                    {
                        _lastClipboardContent = currentClipboard;
                        _lastAutoSyncTime = DateTime.Now;
                        _ = AutoSyncClipboard(currentClipboard);
                    }
                }
            }
            catch
            {
                // Ignore clipboard access errors
            }
        }

        private async Task AutoSyncClipboard(string clipboardText)
        {
            var selectedDevices = DevicesListView.SelectedItems.Cast<BluetoothDeviceInfo>().ToList();
            int successCount = 0;

            foreach (var device in selectedDevices)
            {
                try
                {
                    var clipData = new ClipboardData
                    {
                        Clip = clipboardText,
                        Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()
                    };

                    var jsonData = JsonConvert.SerializeObject(clipData);

                    await Task.Run(() =>
                    {
                        using var client = new BluetoothClient();
                        client.Connect(device.DeviceAddress, ServiceUuid);

                        using var stream = client.GetStream();
                        using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                        writer.Write(jsonData + "\n");
                        Thread.Sleep(500);
                        client.Close();
                    });

                    successCount++;
                }
                catch
                {
                    // Silently fail for auto-sync
                }
            }

            if (successCount > 0)
            {
                Dispatcher.Invoke(() =>
                {
                    StatusTextBlock.Text = $"Auto-synced to {successCount}/{selectedDevices.Count} device(s)";
                });
            }
        }

        private static string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return string.Concat(text.AsSpan(0, maxLength), "...");
        }
    }
}