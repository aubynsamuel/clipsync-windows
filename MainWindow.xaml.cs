using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ClipSyncWindows
{
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<BluetoothDeviceInfo> _devices = new();
        private BluetoothListener? _listener; // Make nullable
        private CancellationTokenSource? _cancellationTokenSource; // Make nullable
        private bool _isServiceRunning = false;
        private static readonly Guid ServiceUuid = new Guid("8ce255c0-200a-11e0-ac64-0800200c9a66");

        public MainWindow()
        {
            InitializeComponent();
            DevicesListView.ItemsSource = _devices;
            
            RefreshDevicesButton.Click += RefreshDevicesButton_Click;
            StartServiceButton.Click += StartServiceButton_Click;
            StopServiceButton.Click += StopServiceButton_Click;
            ShareButton.Click += ShareButton_Click;
            
            LoadPairedDevices();
            
            // Event handler for selection change
            DevicesListView.SelectionChanged += (s, e) => 
            {
                ShareButton.IsEnabled = DevicesListView.SelectedItems.Count > 0;
            };
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
                
                StatusTextBlock.Text = "Service: Listening for Android devices...";
                
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
                    var jsonText = reader.ReadToEnd();
                    
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
                                ShowSimpleNotification("ClipSync", $"Received: {TruncateText(clipboardData.Clip, 50)}");
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
                        using var writer = new StreamWriter(stream);
                        
                        writer.Write(jsonData);
                        writer.Flush();
                        
                        // Keep connection open briefly
                        Thread.Sleep(1000);
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

        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;
                
            return text.Substring(0, maxLength) + "...";
        }

        private void ShowSimpleNotification(string title, string message)
        {
            try
            {
                // Create a simple popup window instead of using NotifyIcon
                var notification = new Window
                {
                    Title = title,
                    Width = 300,
                    Height = 100,
                    WindowStyle = WindowStyle.None,
                    ResizeMode = ResizeMode.NoResize,
                    Topmost = true,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                var textBlock = new TextBlock
                {
                    Text = message,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(10),
                    VerticalAlignment = VerticalAlignment.Center
                };

                notification.Content = textBlock;
                notification.Show();

                // Auto-close after 3 seconds
                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(3)
                };
                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    notification.Close();
                };
                timer.Start();
            }
            catch
            {
                // Notifications are optional, so ignore errors
            }
        }
    }

    public class ClipboardData
    {
        [JsonProperty("clip")]
        public string Clip { get; set; } = string.Empty; // Initialize with empty string
        
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; } = string.Empty; // Initialize with empty string
    }
}