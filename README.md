# ClipSync Windows

A modern Windows application that enables seamless clipboard sharing between Windows computers and Android devices via Bluetooth.

## 🚀 Features

- **Cross-Platform Clipboard Sharing**: Share clipboard content between Windows and Android devices
- **Bluetooth Connectivity**: Uses Bluetooth for secure, local network sharing
- **Modern UI**: Clean, responsive interface with dark/light theme support
- **Real-time Sync**: Instant clipboard synchronization when devices are connected
- **Multi-Device Support**: Connect and share with multiple devices simultaneously
- **Automatic Discovery**: Finds and lists paired Bluetooth devices automatically

## 📋 Requirements

### System Requirements

- **OS**: Windows 10 version 1903 or later / Windows 11
- **Framework**: .NET 9.0 Runtime
- **Bluetooth**: Bluetooth 4.0+ adapter
- **RAM**: 512 MB minimum
- **Storage**: 50 MB available space

### Android Companion App

- Android 5.0 (API level 21) or higher
- Bluetooth enabled
- ClipSync Android app (available separately)

## 📦 Installation

### Option 1: Download Release (Recommended)

1. Go to the [Releases](https://github.com/yourusername/ClipSyncWindows/releases) page
2. Download the latest `ClipSync-Windows-Setup.exe`
3. Run the installer and follow the setup wizard
4. Launch ClipSync from the Start Menu

### Option 2: Build from Source

1. **Prerequisites**:
   - Visual Studio 2022 or later
   - .NET 9.0 SDK
   - Git

2. **Clone and Build**:

   ```bash
   git clone https://github.com/yourusername/ClipSyncWindows.git
   cd ClipSyncWindows
   dotnet restore
   dotnet build --configuration Release
   ```

3. **Run**:

   ```bash
   dotnet run --project ClipSyncWindows.csproj
   ```

## 🔧 Setup & Usage

### Initial Setup

1. **Enable Bluetooth**: Ensure Bluetooth is enabled on your Windows computer
2. **Pair Devices**: Pair your Android device with your Windows computer through Windows Settings
3. **Install Android App**: Install the ClipSync Android companion app
4. **Launch ClipSync**: Start the ClipSync Windows application

### Using ClipSync

#### Starting the Service

1. Click **"Start Service"** to begin listening for connections
2. The status will show "Service: Listening for bluetooth devices..."
3. Your Android device can now connect and share clipboard content

#### Sharing from Windows to Android

1. Copy any text to your Windows clipboard
2. Select target device(s) from the device list
3. Click **"Share Clipboard"** to send content to selected devices

#### Receiving from Android

- When your Android device sends clipboard content, it will automatically appear in your Windows clipboard
- A notification will confirm successful receipt

#### Theme Switching

- Click the theme toggle button (🌙/☀️) to switch between dark and light modes
- Theme preference is automatically saved

## 🔒 Security & Privacy

- **Local Network Only**: All communication happens over Bluetooth (local range)
- **No Internet Required**: No data is sent to external servers
- **Encrypted Connection**: Uses Bluetooth's built-in encryption
- **Paired Devices Only**: Only works with previously paired devices
- **No Data Storage**: Clipboard content is not permanently stored

## 🛠️ Configuration

ClipSync stores its settings in:

```
%APPDATA%\ClipSync\
├── theme_settings.json    # Theme preferences
└── app_config.json       # Application settings (future)
```

## 🐛 Troubleshooting

### Common Issues

**"No devices found"**

- Ensure Bluetooth is enabled on both devices
- Verify devices are paired in Windows Settings
- Try refreshing the device list

**"Connection failed"**

- Check if Android app is running and listening
- Restart Bluetooth on both devices
- Re-pair devices if necessary

**"Service won't start"**

- Run ClipSync as Administrator
- Check Windows Firewall settings
- Ensure no other Bluetooth apps are conflicting

### Getting Help

- Check the [Issues](https://github.com/yourusername/ClipSyncWindows/issues) page
- Create a new issue with detailed error information
- Include Windows version and Bluetooth adapter details

## 🔄 Compatibility

### Tested Bluetooth Adapters

- Intel Wireless Bluetooth
- Realtek Bluetooth adapters
- Qualcomm Bluetooth adapters
- Most USB Bluetooth dongles (4.0+)

### Known Limitations

- Text-only clipboard sharing (images/files not supported yet)
- Maximum text size: 1MB per transfer
- Requires devices to be within Bluetooth range (~10 meters)

## 🚧 Roadmap

- [ ] File and image sharing support
- [ ] WiFi Direct connectivity option
- [ ] Clipboard history
- [ ] Auto-start with Windows
- [ ] Notification system improvements
- [ ] Multi-language support

## 🤝 Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

### Development Setup

1. Fork the repository
2. Create a feature branch: `git checkout -b feature-name`
3. Make your changes and test thoroughly
4. Submit a pull request with a clear description

### Code Style

- Follow C# coding conventions
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Include unit tests for new features

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- [InTheHand.Net.Bluetooth](https://github.com/inthehand/32feet) for Bluetooth connectivity
- [Newtonsoft.Json](https://www.newtonsoft.com/json) for JSON serialization
- Icons from [Lucide](https://lucide.dev/)

## 📞 Support

- **Email**: <support@clipsync.app>
- **GitHub Issues**: [Report a bug](https://github.com/yourusername/ClipSyncWindows/issues/new)
- **Documentation**: [Wiki](https://github.com/yourusername/ClipSyncWindows/wiki)

---

**Made with ❤️ for seamless cross-platform productivity**
