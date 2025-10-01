# Architecture Documentation

This document describes the architecture, design patterns, and key components of the AI Teacher Assistant application.

---

## Project Overview

**AI Teacher Assistant** is a Windows desktop application built with C# and WPF (.NET 9.0) that provides an interactive overlay for teaching and presentation purposes. The application runs as a transparent, system-wide overlay that allows teachers to draw, annotate, and capture screens while presenting.

---

## Technology Stack

- **Framework**: .NET 9.0
- **UI Framework**: WPF (Windows Presentation Foundation)
- **Language**: C# 12.0
- **Platform**: Windows 10/11 (x64)
- **Architecture Pattern**: MVVM (planned for future components)

---

## Project Structure

```
AITeacherAssistant/
├── docs/                        # Documentation
│   ├── CHANGELOG.md             # Change history
│   ├── PROGRESS.md              # Feature progress tracking
│   └── ARCHITECTURE.md          # This file
│
├── src/                         # Source code
│   ├── Views/                   # XAML Windows and User Controls
│   │   ├── MainWindow.xaml      # Main overlay window UI definition
│   │   └── MainWindow.xaml.cs   # Main overlay window code-behind
│   │
│   ├── ViewModels/              # MVVM ViewModels (future)
│   │
│   ├── Models/                  # Data models (future)
│   │
│   ├── Services/                # Business logic services (future)
│   │
│   ├── Utilities/               # Helper classes (future)
│   │
│   └── Resources/               # Images, styles, etc.
│       ├── Styles/              # XAML styles (future)
│       └── Images/              # Image assets (future)
│
├── tests/                       # Unit tests (future)
│
├── App.xaml                     # Application definition
├── App.xaml.cs                  # Application startup logic
├── AssemblyInfo.cs              # Assembly metadata
└── AITeacherAssistant.csproj    # Project file
```

---

## Key Components

### 1. MainWindow (src/Views/MainWindow.xaml / src/Views/MainWindow.xaml.cs)

**Purpose**: The main overlay window that provides transparent, always-on-top functionality.

**Namespace**: `AITeacherAssistant.Views`

**Key Responsibilities**:

- Display transparent fullscreen overlay
- Handle window styling and transparency
- Manage click-through behavior
- Process keyboard shortcuts
- Display control panel and status indicator

**Key Properties**:

- `WindowStyle="None"`: Removes title bar and borders
- `AllowsTransparency="True"`: Enables transparency
- `Background="Transparent"`: Makes window transparent
- `WindowState="Maximized"`: Fullscreen coverage
- `Topmost="True"`: Always on top of other windows
- `ShowInTaskbar="False"`: Hidden from taskbar

**Key Methods**:

- `EnableClickThrough()`: Enables mouse click pass-through using Win32 API
- `DisableClickThrough()`: Disables click-through for UI interaction
- `ToggleOverlay()`: Toggles overlay visibility on/off
- `MainWindow_KeyDown()`: Handles keyboard shortcuts
- `UpdateStatusIndicator()`: Updates UI status display
- `ControlPanel_MouseEnter()`: Disables click-through when hovering over control panel
- `ControlPanel_MouseLeave()`: Re-enables click-through when leaving control panel
- `StatusIndicator_MouseEnter()`: Disables click-through when hovering over status indicator
- `StatusIndicator_MouseLeave()`: Re-enables click-through when leaving status indicator

---

## Technical Implementation Details

### Click-Through Functionality

The application uses Win32 API to implement click-through behavior:

```csharp
// Win32 API Constants
GWL_EXSTYLE = -20              // Extended window style index
WS_EX_TRANSPARENT = 0x00000020 // Click-through flag
WS_EX_LAYERED = 0x00080000     // Layered window flag

// Win32 API Functions
GetWindowLong()   // Retrieve window style
SetWindowLong()   // Modify window style
```

**How it works**:

1. Application gets window handle via `WindowInteropHelper`
2. Retrieves current extended window styles using `GetWindowLong`
3. Adds `WS_EX_TRANSPARENT` flag to make window click-through
4. Applies new style using `SetWindowLong`

This allows mouse clicks to pass through transparent areas to underlying applications while still allowing interaction with visible UI elements (control panel, status indicator).

---

### Window Transparency

WPF transparency is achieved through:

- `AllowsTransparency="True"` on Window
- `WindowStyle="None"` to remove chrome
- `Background="Transparent"` for transparent background
- UI elements with semi-transparent backgrounds (e.g., `#CC1E1E1E`)

---

### Keyboard Shortcut System

**Current Implementation**:

- Uses WPF's `KeyDown` event on the Window
- Checks for modifier keys using `Keyboard.Modifiers`
- `Ctrl+Shift+Q`: Toggle overlay visibility

**Limitations**:

- Only works when window has focus
- Focus can be lost when clicking on other applications

**Future Enhancement**:

- Implement global hotkey registration using Win32 `RegisterHotKey` API
- This would allow shortcuts to work even when window doesn't have focus

---

## Design Patterns

### Current Patterns

1. **Code-Behind Pattern**: Currently using simple code-behind for the main window
2. **Event-Driven Architecture**: UI events trigger business logic
3. **Win32 Interop Pattern**: P/Invoke for native Windows functionality

### Planned Patterns

1. **MVVM (Model-View-ViewModel)**: Will be implemented for more complex features

   - Separation of UI and business logic
   - Data binding for reactive UI updates
   - Commands for user actions

2. **Repository Pattern**: For data persistence and API communication
3. **Service Layer**: For business logic and external API integration

---

## Component Interaction Flow

```
┌─────────────────────────────────────────────────────┐
│                   MainWindow                        │
│  ┌───────────────────────────────────────────────┐ │
│  │         Transparent Overlay Grid              │ │
│  │                                               │ │
│  │  ┌──────────────────┐                        │ │
│  │  │  Control Panel   │ (Top-Right)            │ │
│  │  │  - Title         │                        │ │
│  │  │  - Help Text     │                        │ │
│  │  │  - Close Button  │                        │ │
│  │  └──────────────────┘                        │ │
│  │                                               │ │
│  │                                               │ │
│  │                        ┌──────────────────┐  │ │
│  │                        │ Status Indicator │  │ │
│  │                        │ - Status Dot     │  │ │
│  │ (Bottom-Left)          │ - Status Text    │  │ │
│  │                        └──────────────────┘  │ │
│  └───────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────┘
          │                      │
          │                      │
          ▼                      ▼
   [Win32 API]            [WPF Event System]
   Click-through          Keyboard/Mouse Events
```

---

## Future Architecture Enhancements

### Phase 2: Drawing System

- Add `InkCanvas` control for freehand drawing
- Implement drawing toolbar with tools (pen, eraser, colors)
- Add drawing mode state management
- Disable click-through during drawing

### Phase 3: Text Annotation System

- Implement draggable text box components
- Add text editing controls
- Font/color picker UI

### Phase 4: Screen Capture System

- Implement screenshot capture service
- Add area selection UI
- File system service for local storage

### Phase 5: QR Code Generation

- QR code generation library integration
- Session management service
- QR display window component

### Phase 6: API Integration

- HTTP client service for backend communication
- Authentication service
- Real-time communication (SignalR consideration)
- DTO models for API requests/responses

---

## Dependencies

### Current Dependencies

- **.NET 9.0 SDK**: Core framework
- **WPF**: UI framework (included in .NET)
- **Win32 API**: Native Windows functionality via P/Invoke

### Planned Dependencies

- **System.Net.Http**: For HTTP API calls
- **Newtonsoft.Json** or **System.Text.Json**: JSON serialization
- **QRCoder** or similar: QR code generation
- **Microsoft.Toolkit.Wpf**: Modern WPF controls (optional)

---

## Security Considerations

1. **Win32 API Usage**: Limited to window styling and input handling
2. **API Communication**: Will require secure HTTPS endpoints
3. **Authentication**: Token-based auth for API calls (planned)
4. **Screen Capture**: User consent and data privacy considerations

---

## Performance Considerations

1. **Transparency Performance**: Hardware acceleration enabled via WPF
2. **Click-Through Performance**: Native Win32 API for optimal performance
3. **Memory Management**: Proper disposal of resources in `OnClosing`
4. **UI Responsiveness**: Async/await for IO operations (future API calls)

---

## Testing Strategy

### Manual Testing (Current)

- Overlay visibility and transparency
- Click-through functionality
- Keyboard shortcuts
- Multi-monitor support

### Automated Testing (Planned)

- Unit tests for business logic
- Integration tests for API communication
- UI automation tests for critical workflows

---

## Build and Deployment

**Build Configuration**:

- Target Framework: `net9.0-windows`
- Output Type: `WinExe` (Windows executable)
- Nullable Reference Types: Enabled
- Implicit Usings: Enabled

**Deployment Strategy**:

- Self-contained deployment with .NET runtime
- Single-file executable (optional)
- Windows Installer (MSI) for distribution

---

Last Updated: October 1, 2025
