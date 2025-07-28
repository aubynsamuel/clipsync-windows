# Contributing to ClipSync Windows

Thank you for your interest in contributing to ClipSync Windows! This document provides guidelines and information for contributors.

## 🚀 Getting Started

### Prerequisites

- Visual Studio 2022 or later
- .NET 9.0 SDK
- Git
- Windows 10/11 with Bluetooth support

### Setting Up Development Environment

1. Fork the repository on GitHub
2. Clone your fork locally:

   ```bash

   git clone https://github.com/yourusername/ClipSyncWindows.git
   cd ClipSyncWindows
   ```

3. Open `ClipSyncWindows.sln` in Visual Studio
4. Restore NuGet packages: `dotnet restore`
5. Build the solution: `Ctrl+Shift+B`

## 📝 Code Style Guidelines

### C# Conventions

- Use PascalCase for public members, classes, and methods
- Use camelCase for private fields and local variables
- Use meaningful, descriptive names
- Follow Microsoft's C# coding conventions
- Use `var` when the type is obvious from the right side

### Example

```csharp
public class BluetoothService
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    
    public async Task<bool> ConnectToDeviceAsync(BluetoothDeviceInfo device)
    {
        var connectionResult = await EstablishConnectionAsync(device);
        return connectionResult.IsSuccessful;
    }
}
```

### XML Documentation

- Add XML documentation for all public APIs
- Include `<summary>`, `<param>`, and `<returns>` tags

```csharp
/// <summary>
/// Sends clipboard content to the specified Bluetooth device.
/// </summary>
/// <param name="device">The target Bluetooth device</param>
/// <param name="content">The clipboard content to send</param>
/// <returns>True if the content was sent successfully, false otherwise</returns>
public async Task<bool> SendClipboardAsync(BluetoothDeviceInfo device, string content)
{
    // Implementation
}
```

## 🔧 Development Workflow

### Branch Naming

- Feature branches: `feature/description-of-feature`
- Bug fixes: `bugfix/description-of-bug`
- Documentation: `docs/description-of-change`

### Commit Messages

Use conventional commit format:

```

type(scope): description

[optional body]

[optional footer]
```

Types:

- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

Examples:

```
feat(bluetooth): add device connection timeout handling
fix(ui): resolve theme switching memory leak
docs(readme): update installation instructions
```

### Pull Request Process

1. Create a feature branch from `main`
2. Make your changes with appropriate tests
3. Update documentation if needed
4. Ensure all tests pass
5. Create a pull request with:
   - Clear title and description
   - Reference any related issues
   - Screenshots for UI changes
   - Test instructions

## 🧪 Testing

### Running Tests

```bash
dotnet test
```

### Test Guidelines

- Write unit tests for new functionality
- Maintain or improve code coverage
- Test edge cases and error conditions
- Use descriptive test method names

### Test Structure

```csharp
[Test]
public void SendClipboard_WithValidDevice_ShouldReturnTrue()
{
    // Arrange
    var device = CreateMockDevice();
    var content = "test content";
    
    // Act
    var result = _service.SendClipboard(device, content);
    
    // Assert
    Assert.IsTrue(result);
}
```

## 🐛 Bug Reports

### Before Reporting

- Check existing issues to avoid duplicates
- Test with the latest version
- Gather system information

### Bug Report Template

```markdown
**Describe the bug**
A clear description of what the bug is.

**To Reproduce**
Steps to reproduce the behavior:
1. Go to '...'
2. Click on '....'
3. See error

**Expected behavior**
What you expected to happen.

**Screenshots**
If applicable, add screenshots.

**Environment:**
- OS: [e.g. Windows 11]
- .NET Version: [e.g. 9.0]
- Bluetooth Adapter: [e.g. Intel AX200]
- App Version: [e.g. 1.0.0]

**Additional context**
Any other context about the problem.
```

## 💡 Feature Requests

### Feature Request Template

```markdown
**Is your feature request related to a problem?**
A clear description of what the problem is.

**Describe the solution you'd like**
A clear description of what you want to happen.

**Describe alternatives you've considered**
Other solutions you've considered.

**Additional context**
Any other context or screenshots.
```

## 📚 Areas for Contribution

### High Priority

- [ ] File sharing support
- [ ] Improved error handling and logging
- [ ] Performance optimizations
- [ ] Unit test coverage
- [ ] Accessibility improvements

### Medium Priority

- [ ] WiFi Direct support
- [ ] Clipboard history
- [ ] Auto-start functionality
- [ ] Notification improvements
- [ ] Multi-language support

### Documentation

- [ ] Code documentation
- [ ] User guides
- [ ] API documentation
- [ ] Troubleshooting guides

## 🔒 Security Considerations

### Reporting Security Issues

- **DO NOT** create public issues for security vulnerabilities
- Email security issues to: <security@clipsync.app>
- Include detailed reproduction steps
- Allow time for investigation before public disclosure

### Security Guidelines

- Validate all input data
- Use secure communication protocols
- Follow principle of least privilege
- Avoid hardcoded secrets or credentials

## 📋 Code Review Guidelines

### For Authors

- Keep changes focused and atomic
- Write clear commit messages
- Add tests for new functionality
- Update documentation as needed

### For Reviewers

- Be constructive and respectful
- Focus on code quality and maintainability
- Check for security implications
- Verify tests are adequate

## 🏆 Recognition

Contributors will be recognized in:

- README.md contributors section
- Release notes for significant contributions
- GitHub contributor graphs

## 📞 Getting Help

- **Discord**: [ClipSync Community](https://discord.gg/clipsync)
- **GitHub Discussions**: [Ask questions](https://github.com/yourusername/ClipSyncWindows/discussions)
- **Email**: <dev@clipsync.app>

## 📄 License

By contributing to ClipSync Windows, you agree that your contributions will be licensed under the MIT License.

---

Thank you for contributing to ClipSync Windows! 🎉
