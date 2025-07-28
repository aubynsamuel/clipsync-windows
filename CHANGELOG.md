# Changelog

All notable changes to ClipSync Windows will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0]

### Added

- Cross-platform clipboard sharing between Windows and Android devices
- Bluetooth connectivity for secure local device communication
- Modern WPF interface with dark/light theme support
- Real-time clipboard synchronization
- Service-based background listening for incoming connections
- JSON protocol for reliable data exchange
- Self-contained deployment (no .NET runtime installation required)
- Professional installer with Inno Setup
- Automatic GitHub Actions CI/CD pipeline

### Technical Details

- Built with .NET 9.0 and WPF
- Uses InTheHand.Net.Bluetooth 4.2.0 for Bluetooth connectivity
- Uses Newtonsoft.Json 13.0.1 for data serialization
- Supports Windows 10 version 1903+ and Windows 11
- Self-contained win-x64 runtime included