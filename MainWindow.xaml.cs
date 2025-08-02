using InTheHand.Net.Sockets;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.ComponentModel;

namespace ClipSyncWindows
{
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<BluetoothDeviceInfo> _devices = [];
        private BluetoothListener? _listener;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isServiceRunning = false;
        private static readonly Guid ServiceUuid = new("8ce255c0-200a-11e0-ac64-0800200c9a66");
        private SystemTrayManager? _systemTrayManager;

        public MainWindow()
        {
            InitializeComponent();
            DevicesListView.ItemsSource = _devices;

            RefreshDevicesButton.Click += RefreshDevicesButton_Click;
            StartServiceButton.Click += StartServiceButton_Click;
            StopServiceButton.Click += StopServiceButton_Click;
            ShareButton.Click += ShareButton_Click;
            ThemeToggleButton.Click += ThemeToggleButton_Click;

            LoadPairedDevices();

            // Event handler for selection change
            DevicesListView.SelectionChanged += (s, e) =>
            {
                ShareButton.IsEnabled = DevicesListView.SelectedItems.Count > 0;
                _systemTrayManager?.UpdateDevicesList();
            };

            // Initialize system tray manager
            _systemTrayManager = new SystemTrayManager(this, _devices);

            // Handle window state changes for smart tray behavior
            StateChanged += MainWindow_StateChanged;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_StateChanged(object? sender, EventArgs e)
        {
            // Only minimize to tray if user explicitly minimizes (not when closing)
            if (WindowState == WindowState.Minimized)
            {
                _systemTrayManager?.HideToTray();
            }
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            // Clean up system tray and stop services
            _systemTrayManager?.Dispose();
            StopListeningService();
        }

        private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ThemeManager.Instance.ToggleTheme();
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
                
                // Update system tray device list
                _systemTrayManager?.UpdateDevicesList();
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Error loading devices: {ex.Message}";
                System.Windows.MessageBox.Show($"Failed to load Bluetooth devices: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void StartServiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isServiceRunning)
            {
                return;
            }

            try
            {
                // Configure buttons
                StartServiceButton.IsEnabled = false;
                RefreshDevicesButton.IsEnabled = false;
                StopServiceButton.IsEnabled = true;

                StatusTextBlock.Text = "Starting service...";

                _cancellationTokenSource = new CancellationTokenSource();
                var token = _cancellationTokenSource.Token;

                // Start listener
                _listener = new BluetoothListener(ServiceUuid);
                _listener.Start();
                _isServiceRunning = true;

                StatusTextBlock.Text = "Service: Listening for bluetooth devices...";

                // Run listening loop on a separate task
                await Task.Run(() => ListeningLoop(token), token);
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation, do nothing
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error starting Bluetooth service: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private async Task ListeningLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // Null check before accepting
                    if (_listener == null)
                    {
                        await Task.Delay(1000, token);
                        continue;
                    }

                    using var client = _listener.AcceptBluetoothClient();
                    Dispatcher.Invoke(() =>
                    {
                        StatusTextBlock.Text = $"Connected to: {client.RemoteMachineName}";
                    });

                    using var stream = client.GetStream();
                    using var reader = new StreamReader(stream);

                    // Read the JSON message
                    var jsonText = reader.ReadLine() ?? "";

                    try
                    {
                        var clipboardData = JsonConvert.DeserializeObject<ClipboardData>(jsonText);

                        if (clipboardData?.Clip != null)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                System.Windows.Clipboard.SetText(clipboardData.Clip);
                                StatusTextBlock.Text = "Received clipboard text & copied!";

                                // Instead of using WinForms notification, use a simpler approach
                                NotificationHelper.ShowSimpleNotification("ClipSync", $"ClipText Received: \n {TruncateText(clipboardData.Clip, 50)}");
                            });
                        }
                    }
                    catch (JsonException)
                    {
                        // Ignore malformed JSON
                    }
                }
                catch (Exception ex)
                {
                    if (token.IsCancellationRequested)
                    {
                        break; // Exit if service was stopped
                    }

                    Dispatcher.Invoke(() =>
                    {
                        StatusTextBlock.Text = $"Listener error: {ex.Message}";
                    });

                    // Short delay before retrying
                    await Task.Delay(1000, token);
                }
            }
        }

        private async void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            if (!System.Windows.Clipboard.ContainsText())
            {
                StatusTextBlock.Text = "Clipboard is empty!";
                return;
            }

            var clipboardText = System.Windows.Clipboard.GetText();
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
                    // Create JSON message matching the Android app's format
                    var clipData = new ClipboardData
                    {
                        Clip = clipboardText,
                        Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()
                    };

                    var jsonData = JsonConvert.SerializeObject(clipData);

                    // Send data
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

        private static string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return string.Concat(text.AsSpan(0, maxLength), "...");
        }
    }
            
    public class ClipboardData
    {
        [JsonProperty("clip")]
        public string Clip { get; set; } = string.Empty;

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; } = string.Empty;
    }
}