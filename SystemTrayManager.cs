using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Forms;
using InTheHand.Net.Sockets;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace ClipSyncWindows
{
    public class SystemTrayManager : IDisposable
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly MainWindow _mainWindow;
        private readonly ObservableCollection<BluetoothDeviceInfo> _devices;
        private ToolStripMenuItem? _shareMenuItem;
        private bool _disposed = false;

        public SystemTrayManager(MainWindow mainWindow, ObservableCollection<BluetoothDeviceInfo> devices)
        {
            _mainWindow = mainWindow;
            _devices = devices;

            _notifyIcon = new NotifyIcon
            {
                Icon = new System.Drawing.Icon("app.ico"),
                Text = "ClipSync Windows - Clipboard Sharing",
                Visible = true // Start visible for immediate access
            };

            _notifyIcon.DoubleClick += OnTrayIconDoubleClick;
            CreateContextMenu();
        }

        private void CreateContextMenu()
        {
            var contextMenu = new ContextMenuStrip();

            // Quick Share - only if clipboard has text
            var shareItem = new ToolStripMenuItem("Share Clipboard")
            {
                Enabled = System.Windows.Clipboard.ContainsText()
            };
            _shareMenuItem = shareItem;
            shareItem.Click += OnQuickShare;
            contextMenu.Items.Add(shareItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            // Device selection submenu
            var devicesMenu = new ToolStripMenuItem("Select Devices");
            UpdateDevicesMenu(devicesMenu);
            contextMenu.Items.Add(devicesMenu);

            contextMenu.Items.Add(new ToolStripSeparator());

            // Show/Hide main window
            var showHideItem = new ToolStripMenuItem("Show ClipSync");
            showHideItem.Click += OnShowHide;
            contextMenu.Items.Add(showHideItem);

            contextMenu.Items.Add(new ToolStripSeparator());
            
            // Exit
            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += OnExit;
            contextMenu.Items.Add(exitItem);

            _notifyIcon.ContextMenuStrip = contextMenu;

            // The menu will be updated manually when needed.
        }

        private void UpdateDevicesMenu(ToolStripMenuItem devicesMenu)
        {
            devicesMenu.DropDownItems.Clear();

            if (_devices.Count == 0)
            {
                devicesMenu.DropDownItems.Add(new ToolStripMenuItem("No paired devices") { Enabled = false });
                return;
            }

            foreach (var device in _devices)
            {
                var deviceItem = new ToolStripMenuItem(device.DeviceName)
                {
                    CheckOnClick = true,
                    Tag = device
                };
                
                // Check if device is selected in main window
                deviceItem.Checked = _mainWindow.DevicesListView.SelectedItems.Contains(device);
                
                deviceItem.CheckedChanged += OnDeviceSelectionChanged;
                devicesMenu.DropDownItems.Add(deviceItem);
            }
        }

        private void OnDeviceSelectionChanged(object? sender, EventArgs e)
        {
            if (sender is not ToolStripMenuItem menuItem || menuItem.Tag is not BluetoothDeviceInfo device)
                return;

            _mainWindow.Dispatcher.Invoke(() =>
            {
                if (menuItem.Checked)
                {
                    if (!_mainWindow.DevicesListView.SelectedItems.Contains(device))
                        _mainWindow.DevicesListView.SelectedItems.Add(device);
                }
                else
                {
                    _mainWindow.DevicesListView.SelectedItems.Remove(device);
                }
            });
        }

        private void UpdateShareMenuItem()
        {
            if (_shareMenuItem != null)
            {
                _shareMenuItem.Enabled = System.Windows.Clipboard.ContainsText() && _mainWindow.DevicesListView.SelectedItems.Count > 0;
            }
        }

        private async void OnQuickShare(object? sender, EventArgs e)
        {
            if (!System.Windows.Clipboard.ContainsText())
            {
                ShowBalloonTip("Clipboard is empty!", ToolTipIcon.Warning);
                return;
            }

            var selectedDevices = _mainWindow.DevicesListView.SelectedItems.Cast<BluetoothDeviceInfo>().ToList();
            if (selectedDevices.Count == 0)
            {
                ShowBalloonTip("No devices selected!", ToolTipIcon.Warning);
                return;
            }

            // ShowBalloonTip("Sharing clipboard...", ToolTipIcon.Info);

            try
            {
                await ShareClipboardToDevices(selectedDevices);
            }
            catch (Exception ex)
            {
                ShowBalloonTip($"Share failed: {ex.Message}", ToolTipIcon.Error);
            }
        }

        private async Task ShareClipboardToDevices(List<BluetoothDeviceInfo> devices)
        {
            var clipboardText = System.Windows.Clipboard.GetText();
            if (string.IsNullOrEmpty(clipboardText))
                return;

            int successCount = 0;
            var serviceUuid = new Guid("8ce255c0-200a-11e0-ac64-0800200c9a66");

            foreach (var device in devices)
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
                        client.Connect(device.DeviceAddress, serviceUuid);

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
                    // Continue with other devices
                }
            }

            if(successCount > 0){
                // var message = $"Clipboard shared with {successCount}/{devices.Count} devices";
                // ShowBalloonTip(message, ToolTipIcon.Info);
            }
            else{
                var message = "Failed to share clipboard";
                ShowBalloonTip(message, ToolTipIcon.Error);
            }
        }

        private void OnTrayIconDoubleClick(object? sender, EventArgs e)
        {
            ShowMainWindow();
        }

        private void OnShowHide(object? sender, EventArgs e)
        {
            if (_mainWindow.IsVisible && _mainWindow.WindowState != WindowState.Minimized)
            {
                HideToTray();
            }
            else
            {
                ShowMainWindow();
            }
        }

        private void OnExit(object? sender, EventArgs e)
        {
            _mainWindow.Close();
        }

        public void ShowMainWindow()
        {
            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Activate();
            _notifyIcon.Visible = false;

            // Update context menu text
            if (_notifyIcon.ContextMenuStrip?.Items[4] is ToolStripMenuItem showHideItem)
            {
                showHideItem.Text = "Hide to Tray";
            }
        }

        public void HideToTray()
        {
            _mainWindow.Hide();
            _notifyIcon.Visible = true;
            // ShowBalloonTip("ClipSync minimized to tray", ToolTipIcon.Info);

            // Update context menu text
            if (_notifyIcon.ContextMenuStrip?.Items[4] is ToolStripMenuItem showHideItem)
            {
                showHideItem.Text = "Open ClipSync";
            }
        }

        public void UpdateDevicesList()
        {
            if (_notifyIcon.ContextMenuStrip?.Items[2] is ToolStripMenuItem devicesMenu)
            {
                UpdateDevicesMenu(devicesMenu);
            }
            UpdateShareMenuItem();
        }

        private void ShowBalloonTip(string message, ToolTipIcon icon)
        {
            _notifyIcon.ShowBalloonTip(200, "ClipSync", message, icon);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _notifyIcon?.Dispose();
                _disposed = true;
            }
        }
    }
}
