# AI Teacher Assistant

A Windows desktop application that provides an interactive, transparent overlay for teaching and presentation purposes. Built with C# and WPF on .NET 9.0.

## 🎯 Features

### ✅ Implemented

- **Transparent System-Wide Overlay**: Always-on-top transparent window that overlays all applications
- **Click-Through Functionality**: Mouse clicks pass through transparent areas to underlying applications
- **Toggle Overlay**: Use `Ctrl+Shift+Q` to show/hide the overlay
- **Control Panel**: Floating UI in top-right corner with application controls
- **Status Indicator**: Visual feedback in bottom-left corner showing overlay state

### 🚧 Planned Features

- Freehand drawing with InkCanvas
- Text annotations with movable text boxes
- Screen capture (full screen or selected area)
- QR code generation for session sharing
- API integration with backend server
- AI-powered teaching assistance

## 🚀 Getting Started

### Prerequisites

- Windows 10 or Windows 11
- .NET 9.0 SDK or later

### Building the Project

1. Clone the repository

```bash
git clone <repository-url>
cd AITeacherAssistant
```

2. Build the project

```bash
dotnet build
```

3. Run the application

```bash
dotnet run
```

Or open `AITeacherAssistant.csproj` in Visual Studio 2022 and press F5.

## 🎮 Usage

### Keyboard Shortcuts

- **Ctrl+Shift+Q**: Toggle overlay visibility on/off

### Controls

- **Close Button**: Click the "Close" button in the control panel to exit the application
- **Control Panel**: Located in the top-right corner, provides application information and controls
- **Status Indicator**: Located in the bottom-left corner, shows current overlay state (green = active, red = inactive)

## 🏗️ Architecture

The application uses WPF with a transparent, borderless window that covers the entire screen. Key technical features:

- **Win32 API Integration**: Uses `GetWindowLong` and `SetWindowLong` for click-through functionality
- **WPF Transparency**: `AllowsTransparency="True"` with transparent background
- **Always On Top**: `Topmost="True"` property ensures overlay stays above all windows
- **Extended Window Styles**: `WS_EX_TRANSPARENT` flag enables mouse click pass-through

For detailed architecture information, see [ARCHITECTURE.md](ARCHITECTURE.md).

## 📝 Documentation

- **[CHANGELOG.md](CHANGELOG.md)**: Detailed change history and version information
- **[PROGRESS.md](PROGRESS.md)**: Feature development status and roadmap
- **[ARCHITECTURE.md](ARCHITECTURE.md)**: Technical architecture and design decisions

## 🔧 Development

### Project Structure

```
AITeacherAssistant/
├── MainWindow.xaml          # Main overlay window UI
├── MainWindow.xaml.cs       # Main window code-behind
├── App.xaml                 # Application definition
├── App.xaml.cs              # Application startup
├── AITeacherAssistant.csproj # Project file
└── docs/                    # Documentation (CHANGELOG, PROGRESS, ARCHITECTURE)
```

### Technologies

- **.NET 9.0**: Core framework
- **WPF**: UI framework
- **C# 12.0**: Programming language
- **Win32 API**: Native Windows functionality via P/Invoke

## 🐛 Known Issues

- **Keyboard Focus**: The `Ctrl+Shift+Q` shortcut only works when the overlay window has focus. Future versions will implement global hotkey registration.
- **Multi-Monitor Support**: Not yet fully tested on multi-monitor setups.

## 🤝 Contributing

Contributions are welcome! Please ensure you:

1. Update CHANGELOG.md with your changes
2. Update PROGRESS.md if working on new features
3. Follow the existing code style and patterns
4. Add appropriate comments and documentation

## 📄 License

[Add your license information here]

## 👥 Authors

[Add author information here]

---

**Status**: In Development  
**Version**: 0.1.0-alpha  
**Last Updated**: October 1, 2025
