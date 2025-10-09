using InTheHand.Net.Sockets;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using ClipSyncWindows.Models;
using ClipSyncWindows.Services;
using ClipSyncWindows.ViewModels;

namespace ClipSyncWindows
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly ObservableCollection<BluetoothDeviceInfo> _devices = [];
        private BluetoothListener? _listener;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isServiceRunning = false;
        private static readonly Guid ServiceUuid = new("8ce255c0-200a-11e0-ac64-0800200c9a66");

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
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
            };
        }

        private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ToggleTheme();
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
                    // Null check before accepting
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

                    // Read the JSON message
                    var jsonText = reader.ReadLine() ?? "";

                    try
                    {
                        var clipboardData = JsonConvert.DeserializeObject<ClipboardData>(jsonText);

                        if (clipboardData?.Clip != null)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                Clipboard.SetText(clipboardData.Clip);
                                StatusTextBlock.Text = "Received clipboard text & copied!";

                                // Instead of using WinForms notification, use a simpler approach
                                NotificationHelper.ShowSimpleNotification("ClipSync", $"ClipText Received: \n {TruncateText(clipboardData.Clip, 50)}");
                            });
                        }
                    }
                    catch (JsonException)
                    {
                        // If JSON parsing fails, try using the text directly
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

                        // Short delay before retrying
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
}