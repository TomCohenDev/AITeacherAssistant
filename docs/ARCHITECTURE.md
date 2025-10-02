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
│   │   ├── MainWindow.xaml.cs   # Main overlay window code-behind
│   │   ├── StartupWindow.xaml   # Startup window with QR code UI
│   │   └── StartupWindow.xaml.cs # Startup window code-behind
│   │
│   ├── ViewModels/              # MVVM ViewModels (future)
│   │
│   ├── Models/                  # Data models (future)
│   │
│   ├── Services/                # Business logic services
│   │   ├── SessionService.cs    # Session management and code generation
│   │   └── QRCodeService.cs     # QR code generation service
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

### 2. StartupWindow (src/Views/StartupWindow.xaml / src/Views/StartupWindow.xaml.cs)

**Purpose**: Initial connection window for users to join sessions via QR code or session code.

**Namespace**: `AITeacherAssistant.Views`

**Key Responsibilities**:

- Display modern, attractive UI for user connection
- Generate and display QR codes for mobile scanning
- Show 5-letter session codes for manual entry
- **Poll API for connection status every 2.5 seconds**
- **Handle 5-minute timeout with retry functionality**
- Manage connection status and user feedback
- Handle transition to MainWindow after connection

**Key Properties**:

- `Height="900" Width="650"`: Optimized window size
- `ResizeMode="CanResize"`: Allow window resizing
- `WindowStartupLocation="CenterScreen"`: Centered on screen
- `Background="#F5F7FA"`: Light background color
- `_pollingTimer`: DispatcherTimer for API polling
- `_isPolling`: Boolean tracking polling state
- `POLLING_INTERVAL_MS = 2500`: 2.5-second polling interval
- `TIMEOUT_SECONDS = 300`: 5-minute timeout

**Key Methods**:

- `StartupWindow_Loaded()`: Initialize session, generate QR code, and start polling
- `StartPolling()`: Begin polling timer for connection status
- `StopPolling()`: Stop polling timer and cleanup
- `CheckConnectionStatus()`: Call API to check if user has connected
- `HandleTimeout()`: Handle 5-minute timeout with retry UI
- `OnUserConnected()`: Handle user connection event
- `OnUserDisconnected()`: Handle user disconnection event
- `TestConnectionButton_Click()`: Simulate connection for testing or retry polling
- `BeginSessionButton_Click()`: Launch MainWindow and close startup

### 3. SessionService (src/Services/SessionService.cs)

**Purpose**: Manages session codes and connection state.

**Namespace**: `AITeacherAssistant.Services`

**Key Responsibilities**:

- Generate random 5-letter session codes (A-Z)
- Track connection state (connected/disconnected)
- Provide events for connection status changes
- Generate JSON payload for QR codes

**Key Properties**:

- `CurrentSessionCode`: Current 5-letter session code
- `IsUserConnected`: Boolean connection state

**Key Methods**:

- `GenerateSessionCode()`: Create random 5-letter code
- `CreateNewSession()`: Generate new session and return code
- `MarkUserConnected()`: Set user as connected
- `MarkUserDisconnected()`: Set user as disconnected
- `SimulateUserConnection()`: Test connection simulation
- `GetQRCodePayload()`: Generate JSON for QR code

### 4. ApiService (src/Services/ApiService.cs)

**Purpose**: Handles all communication with the n8n backend API.

**Namespace**: `AITeacherAssistant.Services`

**Key Responsibilities**:

- Create sessions via POST /sessions/create endpoint
- Check session status via GET /sessions/status endpoint
- Handle HTTP requests with proper error handling
- Serialize/deserialize JSON data
- Manage HTTP client lifecycle

**Key Properties**:

- `BaseUrl`: n8n webhook API base URL
- `_httpClient`: HTTP client for API communication

**Key Methods**:

- `CreateSession(code)`: Create new session with 5-letter code
- `GetSessionStatus(code)`: Check if webapp has connected to session
- `ConnectToSession(code, userInfo)`: Connect user to existing session (future)

**Response Models**:

- `SessionStatusResponse`: API response for session status checks
- `WebappInfo`: Information about webapp connection status

### 5. QRCodeService (src/Services/QRCodeService.cs)

**Purpose**: Generates QR codes using QRCoder library.

**Namespace**: `AITeacherAssistant.Services`

**Key Responsibilities**:

- Generate QR codes from text data
- Convert System.Drawing.Bitmap to WPF BitmapImage
- Handle QR code sizing and formatting
- Generate session-specific QR codes

**Key Methods**:

- `GenerateQRCode(string data, int size)`: Generate QR code from text
- `GenerateSessionQRCode(string sessionCode, int size)`: Generate session QR code
- `ConvertToBitmapImage(Bitmap bitmap)`: Convert to WPF-compatible image

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

## Application Flow

### Startup to Overlay Flow

```
┌─────────────────────────────────────────────────────┐
│                Application Start                    │
└─────────────────────┬───────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────┐
│                StartupWindow                        │
│  ┌───────────────────────────────────────────────┐ │
│  │         Modern UI Card                        │ │
│  │  - App Logo & Title                          │ │
│  │  - QR Code (300x300px)                       │ │
│  │  - Session Code (5 letters)                  │ │
│  │  - Connection Status                         │ │
│  │  - Test/Begin Buttons                        │ │
│  └───────────────────────────────────────────────┘ │
└─────────────────────┬───────────────────────────────┘
                      │
                      │ User Connects (QR/Code)
                      ▼
┌─────────────────────────────────────────────────────┐
│                MainWindow (Overlay)                 │
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
│  │                        ┌──────────────────┐  │ │
│  │                        │ Status Indicator │  │ │
│  │                        │ - Status Dot     │  │ │
│  │ (Bottom-Left)          │ - Status Text    │  │ │
│  │                        └──────────────────┘  │ │
│  └───────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────┘
```

## Component Interaction Flow

### Service Layer Interactions

```
┌─────────────────────────────────────────────────────┐
│                StartupWindow                        │
│  ┌───────────────────────────────────────────────┐ │
│  │         UI Components                         │ │
│  │  - QR Code Image                             │ │
│  │  - Session Code Text                         │ │
│  │  - Status Indicator                          │ │
│  │  - Action Buttons                            │ │
│  └───────────────────────────────────────────────┘ │
└─────────────────────┬───────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────┐
│                Service Layer                        │
│  ┌─────────────────┐    ┌─────────────────────────┐ │
│  │ SessionService  │    │    QRCodeService        │ │
│  │ - Generate Code │    │ - Generate QR Code      │ │
│  │ - Track State   │    │ - Convert to Bitmap     │ │
│  │ - Events        │    │ - Error Handling        │ │
│  └─────────────────┘    └─────────────────────────┘ │
└─────────────────────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────┐
│                MainWindow (Overlay)                 │
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

## Session Status Polling Mechanism

The StartupWindow implements a robust polling system to monitor webapp connections in real-time.

### Polling Configuration

- **Poll Interval**: 2.5 seconds (2500ms)
- **Timeout**: 5 minutes (300 seconds)
- **Endpoint**: `GET /sessions/status?code={CODE}`
- **Timer Type**: `DispatcherTimer` (UI thread)

### Polling State Machine

```
StartupWindow Loaded
        ↓
Start Polling (every 2.5s)
        ↓
Check Status via API
        ├─ webapp.connected = false → Continue polling
        ├─ webapp.connected = true → Stop polling, show "User Connected"
        └─ 5 minutes elapsed → Stop polling, show timeout error
```

### Error Handling

- **Network Errors**: Continue polling (transient errors)
- **API Errors**: Log error, continue polling
- **Timeout**: Show retry UI with user notification
- **Window Close**: Stop polling and cleanup resources

### UI Feedback

- **Polling Active**: Orange status dot, "Waiting for connection..."
- **User Connected**: Green status dot, "✓ User Connected"
- **Timeout**: Orange status dot, "⏱️ Connection Timeout"
- **Retry Button**: "🔄 Retry Polling" after timeout

---

## Supabase Realtime Subscription System

The application implements real-time AI message delivery using Supabase's WebSocket-based realtime functionality.

### Realtime Architecture

```
┌─────────────────────────────────────────────────────┐
│                Supabase Database                    │
│  ┌───────────────────────────────────────────────┐ │
│  │              messages table                   │ │
│  │  - id (UUID)                                  │ │
│  │  - session_id (UUID)                          │ │
│  │  - role (text) - "assistant" or "user"       │ │
│  │  - content (text) - message content           │ │
│  │  - metadata (jsonb) - structured data         │ │
│  │  - created_at (timestamp)                     │ │
│  └───────────────────────────────────────────────┘ │
└─────────────────────┬───────────────────────────────┘
                      │
                      │ WebSocket Connection
                      ▼
┌─────────────────────────────────────────────────────┐
│                SupabaseService                      │
│  ┌───────────────────────────────────────────────┐ │
│  │         Realtime Subscription                 │ │
│  │  - Channel: "messages-{sessionId}"           │ │
│  │  - Event: INSERT on messages table           │ │
│  │  - Filter: session_id = current session      │ │
│  │  - Filter: role = "assistant"                │ │
│  └───────────────────────────────────────────────┘ │
└─────────────────────┬───────────────────────────────┘
                      │
                      │ Events
                      ▼
┌─────────────────────────────────────────────────────┐
│                MainWindow                           │
│  ┌───────────────────────────────────────────────┐ │
│  │         Event Handlers                        │ │
│  │  - OnAnnotationReceived()                     │ │
│  │  - OnTextMessageReceived()                    │ │
│  │  - OnSupabaseError()                          │ │
│  └───────────────────────────────────────────────┘ │
└─────────────────────┬───────────────────────────────┘
                      │
                      │ UI Updates
                      ▼
┌─────────────────────────────────────────────────────┐
│                Annotation Canvas                    │
│  ┌───────────────────────────────────────────────┐ │
│  │         Rendered Annotations                   │ │
│  │  - Text blocks                                │ │
│  │  - Arrows and shapes                          │ │
│  │  - Freehand drawings                          │ │
│  │  - Real-time updates                          │ │
│  └───────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────┘
```

### Message Flow

1. **AI Processing**: n8n processes user input and generates AI response
2. **Database Insert**: AI response inserted into Supabase `messages` table
3. **Realtime Trigger**: Supabase triggers WebSocket event for new message
4. **Client Filtering**: SupabaseService filters by session_id and role
5. **Event Dispatch**: Appropriate event handler called based on message type
6. **UI Rendering**: AnnotationRenderer renders visual elements on canvas

### Message Types

- **`annotation`**: Pure annotation data (shapes, arrows, text)
- **`text_response`**: Pure text response from AI
- **`mixed`**: Both annotation and text content
- **`text_with_image`**: Text response with uploaded image

### Configuration

- **Supabase URL**: `https://YOUR_PROJECT.supabase.co`
- **Supabase Anon Key**: Public key for client authentication
- **Auto-connect**: Enabled for automatic WebSocket connection
- **Auto-refresh**: Enabled for token refresh

### Error Handling

- **Connection Failure**: Show warning dialog, continue without realtime
- **Subscription Failure**: Log error, attempt reconnection
- **Message Parse Error**: Log error, skip malformed message
- **UI Thread Safety**: All UI updates dispatched to UI thread

### Performance Considerations

- **WebSocket Overhead**: Minimal, only active during session
- **Client-side Filtering**: Reduces unnecessary message processing
- **Event-driven**: No polling, immediate response to new messages
- **Memory Management**: Proper cleanup on window close

---

## Dependencies

### Current Dependencies

- **.NET 9.0 SDK**: Core framework
- **WPF**: UI framework (included in .NET)
- **Win32 API**: Native Windows functionality via P/Invoke
- **QRCoder 1.6.0**: QR code generation library
- **System.Drawing.Common**: Required by QRCoder for bitmap operations
- **supabase-csharp 0.16.2**: Main Supabase client library
- **realtime-csharp 6.0.4**: Supabase realtime WebSocket functionality
- **postgrest-csharp 3.5.1**: PostgreSQL REST API client
- **gotrue-csharp 4.2.7**: Supabase authentication client
- **supabase-storage-csharp 1.4.0**: Supabase storage client
- **functions-csharp 1.3.2**: Supabase Edge Functions client
- **Newtonsoft.Json 13.0.3**: JSON serialization (via Supabase dependencies)
- **System.Reactive 5.0.0**: Reactive extensions (via Supabase dependencies)
- **Websocket.Client 4.6.1**: WebSocket client (via Supabase dependencies)

### Planned Dependencies

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
