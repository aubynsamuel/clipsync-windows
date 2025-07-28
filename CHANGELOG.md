# Changelog

All notable changes to ClipSync Windows will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Initial release preparation
- Production-ready configuration
- Comprehensive documentation
- Security improvements
- Error handling enhancements

## [1.0.0] - 2024-07-28

### Added

- Cross-platform clipboard sharing between Windows and Android devices
- Bluetooth connectivity for secure local sharing
- Modern WPF UI with dark/light theme support
- Real-time clipboard synchronization
- Multi-device support for simultaneous connections
- Automatic Bluetooth device discovery
- Theme persistence across application restarts
- JSON-based communication protocol
- Service-based listening for incoming connections

### Features

- **Bluetooth Integration**: Uses InTheHand.Net.Bluetooth for reliable connectivity
- **Theme System**: Complete dark/light theme implementation with ThemeManager
- **Device Management**: Automatic paired device detection and listing
- **Clipboard Sharing**: Bidirectional text sharing with Android companion app
- **Modern UI**: Clean, responsive interface following Windows design guidelines
- **Error Handling**: Basic error reporting and user feedback

### Technical Details

- Built with .NET 9.0 and WPF
- Uses Newtonsoft.Json for data serialization
- Implements MVVM patterns for UI binding
- Bluetooth service UUID: 8ce255c0-200a-11e0-ac64-0800200c9a66
- Supports Windows 10 version 1903+ and Windows 11

### Known Limitations

- Text-only clipboard sharing (no files or images)
- Requires pre-paired Bluetooth devices
- Limited to Bluetooth range (~10 meters)
- No encryption beyond Bluetooth's built-in security

---

## Release Notes Template for Future Versions

### [X.Y.Z] - YYYY-MM-DD

#### Added

- New features

#### Changed

- Changes in existing functionality

#### Deprecated

- Soon-to-be removed features

#### Removed

- Now removed features

#### Fixed

- Bug fixes

#### Security

- Security improvements
