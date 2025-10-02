# Changelog

All notable changes to the AI Teacher Assistant project will be documented in this file.

---

## [Date: 2025-10-02 - Supabase Realtime Integration]

### Added

- **Supabase Realtime Subscriptions**: Real-time AI message delivery system

  - WebSocket-based realtime connection to Supabase
  - Automatic listening for new messages in the `messages` table
  - Client-side filtering by session_id and role (assistant only)
  - Event-driven architecture for annotation and text message handling

- **SupabaseService**: Complete realtime subscription management

  - Initialize Supabase client with project URL and anon key
  - Subscribe to session-specific message channels
  - Handle INSERT events on messages table
  - Parse message metadata and route to appropriate handlers
  - Proper cleanup and unsubscription on window close

- **Message Models**: Structured data models for Supabase messages

  - `MessageMetadata` for message type and annotation data
  - `SupabaseMessage` for complete message structure
  - Support for annotation, text_response, and mixed message types
  - JSON serialization attributes for proper deserialization

- **Configuration Service**: Centralized application configuration

  - Supabase URL and anon key configuration
  - n8n API base URL and polling intervals
  - Application name and version constants
  - Easy configuration management for different environments

- **Enhanced MainWindow**: Real-time annotation rendering

  - Constructor now accepts sessionCode and sessionId parameters
  - Automatic Supabase realtime subscription on window load
  - Event handlers for annotation and text message reception
  - UI thread dispatching for safe annotation rendering
  - Proper cleanup of subscriptions on window close

- **Session ID Management**: Complete session tracking
  - StartupWindow now captures and stores sessionId from API responses
  - SessionId passed to MainWindow for realtime subscription
  - Enhanced polling to capture session metadata

### Changed

- **MainWindow Constructor**: Updated to accept session parameters

  - Now requires sessionCode and sessionId for realtime functionality
  - Maintains backward compatibility with existing functionality

- **StartupWindow Flow**: Enhanced to capture and pass session metadata

  - Polling now captures sessionId from API responses
  - SessionId passed to MainWindow for realtime subscription
  - Better integration between startup and overlay phases

- **API Integration**: Enhanced session status checking
  - SessionId now captured and stored during polling
  - Better session metadata management

### Files Modified

- `src/Services/SupabaseService.cs` (created - realtime subscription management)
- `src/Services/Configuration.cs` (created - centralized configuration)
- `src/Models/MessageModels.cs` (created - Supabase message models)
- `src/Views/MainWindow.xaml.cs` (updated - realtime integration)
- `src/Views/StartupWindow.xaml.cs` (updated - sessionId handling)
- `AITeacherAssistant.csproj` (added Supabase packages)

### Dependencies Added

- `supabase-csharp` (0.16.2) - Main Supabase client library
- `realtime-csharp` (6.0.4) - Realtime WebSocket functionality
- `postgrest-csharp` (3.5.1) - PostgreSQL REST API client
- `gotrue-csharp` (4.2.7) - Supabase authentication
- `supabase-storage-csharp` (1.4.0) - Supabase storage client
- `functions-csharp` (1.3.2) - Supabase Edge Functions client

---

## [Date: 2025-01-27 - Session Status Polling Implementation]

### Added

- **Session Status Polling**: Real-time connection monitoring in StartupWindow

  - Automatic polling every 2.5 seconds to check webapp connection status
  - 5-minute timeout with user-friendly error messages
  - Retry functionality for failed connection attempts
  - Visual feedback with status indicators and button states

- **Enhanced API Service**: Extended API communication capabilities

  - New `GetSessionStatus()` method for checking connection status
  - `SessionStatusResponse` and `WebappInfo` models for API responses
  - Proper JSON deserialization with case-insensitive property mapping
  - Enhanced error handling for network issues

- **Improved User Experience**: Better connection flow management
  - Test Connection button for development and testing
  - Retry Polling button after timeout
  - Automatic UI updates when connection status changes
  - Proper cleanup of polling timers on window close

### Changed

- **StartupWindow Behavior**: Enhanced connection detection

  - Polling starts automatically when window loads
  - Real-time status updates via API calls
  - Better integration with SessionService events
  - Improved error handling and user feedback

- **API Integration**: Enhanced session status checking
  - Updated endpoint format: `GET /sessions/status?code={CODE}`
  - Better response parsing with proper error handling
  - Case-insensitive JSON property mapping

### Files Modified

- `src/Views/StartupWindow.xaml.cs` - Added polling logic and timer management
- `src/Views/StartupWindow.xaml` - Added Test Connection button
- `src/Services/ApiService.cs` - Added session status API method and models
- `docs/CHANGELOG.md` - Updated with polling feature documentation

---

## [Date: 2025-10-01 18:00 - Home/Welcome Page Implementation]

### Added

- **Home/Welcome Page**: Complete welcome screen with modern UI design

  - Professional card-based layout with gradient backgrounds
  - Three main sections: Quick Start, Documentation, Support & Community
  - Modern button styles with hover effects and shadows
  - Responsive design with proper spacing and typography
  - External link integration for documentation and support

- **API Service Integration**: Complete n8n backend communication

  - HttpClient-based API service with proper error handling
  - Session creation endpoint integration
  - Session status checking functionality
  - Async/await pattern with timeout handling
  - JSON serialization for API payloads

- **Enhanced Application Flow**: Improved startup experience
  - HomePage as new application entry point
  - Seamless transition from HomePage to StartupWindow
  - Session code generation and API integration
  - Loading states and error handling

### Changed

- **Application Startup**: Updated to start with HomePage instead of StartupWindow

  - App.xaml StartupUri changed to src/Views/HomePage.xaml
  - StartupWindow now accepts sessionCode parameter
  - Removed session generation from StartupWindow (now handled in HomePage)

- **Session Management**: Enhanced session creation flow
  - Session code generated in HomePage before API call
  - API integration for session creation
  - Better error handling and user feedback

### Files Modified

- `src/Views/HomePage.xaml` (created - modern welcome page UI)
- `src/Views/HomePage.xaml.cs` (created - page logic and API integration)
- `src/Services/ApiService.cs` (created - n8n API communication)
- `src/Views/StartupWindow.xaml.cs` (updated - accept sessionCode parameter)
- `App.xaml` (updated - new startup page)

---

## [Date: 2025-10-01 17:00 - Startup Window with QR Code Implementation]

### Added

- **Startup Window with QR Code**: Complete startup flow for user connection

  - Modern, attractive UI with gradient background and card layout
  - QR code generation using QRCoder library (300x300px)
  - 5-letter session code display with monospace formatting
  - Connection status indicator with colored dot
  - Test connection button for development
  - Begin session button (appears after connection)
  - Invisible scrolling with hidden scrollbar

- **Session Management Service**: Complete session handling system

  - Random 5-letter code generation (A-Z only)
  - Session state management (connected/disconnected)
  - Event-driven architecture for connection status
  - JSON payload generation for QR codes
  - Test simulation functionality

- **QR Code Generation Service**: Professional QR code creation

  - QRCoder NuGet package integration
  - BitmapImage conversion for WPF display
  - Configurable QR code size
  - Session-specific QR code generation
  - Error handling for QR generation

- **Application Flow**: Complete startup-to-overlay workflow
  - StartupWindow as application entry point
  - Session code generation on startup
  - QR code display with proper formatting
  - Connection simulation for testing
  - Seamless transition to MainWindow overlay

### Changed

- **Application Startup**: Changed from MainWindow to StartupWindow

  - App.xaml StartupUri updated to src/Views/StartupWindow.xaml
  - MainWindow now launched from StartupWindow after connection

- **Window Sizing**: Optimized startup window dimensions
  - Initial size: 600x700px
  - Final size: 650x900px (user adjusted)
  - Invisible scrolling for content overflow

### Fixed

- **Content Visibility**: Resolved content cutoff issues
  - Increased window height to accommodate all content
  - Added invisible scrolling as backup
  - Optimized margins and spacing

### Files Modified

- `src/Views/StartupWindow.xaml` (created - modern UI design)
- `src/Views/StartupWindow.xaml.cs` (created - logic and event handlers)
- `src/Services/SessionService.cs` (created - session management)
- `src/Services/QRCodeService.cs` (created - QR code generation)
- `App.xaml` (updated StartupUri)
- `AITeacherAssistant.csproj` (added QRCoder package reference)

---

## [Date: 2025-10-01 16:25 - Click-Through Bug Fix (Simplified)]

### Fixed

- **Control Panel Clickability Issue**: Fixed the close button and control panel not being clickable
  - Removed problematic WS_EX_TRANSPARENT implementation that was blocking all mouse/keyboard events
  - Simplified approach: window now receives all events properly, allowing UI elements to be clickable
  - Keyboard shortcuts (Ctrl+Shift+Q) now work correctly
  - All UI elements (close button, control panel, status indicator) are fully interactive

### Changed

- **Simplified Architecture**: Removed complex Win32 API click-through implementation
- **Event Handling**: Window now properly receives mouse and keyboard events
- **Focus Management**: Window can now maintain focus for keyboard shortcuts

### Files Modified

- `src/Views/MainWindow.xaml.cs` (removed problematic Win32 API calls, simplified event handling)

---

## [Date: 2025-10-01 16:15 - Click-Through Bug Fix]

### Fixed

- **Control Panel Clickability Issue**: Fixed the close button and control panel not being clickable
  - Implemented dynamic click-through behavior using mouse event handlers
  - Click-through is now disabled when mouse hovers over UI elements (ControlPanel, StatusIndicator)
  - Click-through is re-enabled when mouse leaves UI elements
  - Maintains transparent click-through for empty areas while allowing interaction with overlay controls

### Changed

- **Mouse Event Handling**: Added MouseEnter/MouseLeave event handlers for ControlPanel and StatusIndicator
- **Dynamic Click-Through**: Click-through behavior now changes based on mouse position over UI elements

### Files Modified

- `src/Views/MainWindow.xaml.cs` (added mouse event handlers and dynamic click-through logic)

---

## [Date: 2025-10-01 15:51 - Project Restructure]

### Added

- **Professional Project Structure**: Implemented organized folder structure following best practices

  - `docs/` folder for all documentation files
  - `src/` folder for all source code
  - `tests/` folder for future unit tests
  - `src/Views/` for XAML windows and user controls
  - `src/ViewModels/` for MVVM ViewModels (future)
  - `src/Models/` for data models (future)
  - `src/Services/` for business logic services (future)
  - `src/Utilities/` for helper classes (future)
  - `src/Resources/` for images, styles, and resources
  - `src/Resources/Styles/` for XAML styles
  - `src/Resources/Images/` for image assets

- **Namespace Organization**: Updated namespaces to reflect new folder structure
  - `AITeacherAssistant.Views` namespace for UI components
  - Proper separation of concerns with dedicated folders

### Changed

- **File Organization**: Moved all files to appropriate folders

  - `MainWindow.xaml` → `src/Views/MainWindow.xaml`
  - `MainWindow.xaml.cs` → `src/Views/MainWindow.xaml.cs`
  - `CHANGELOG.md` → `docs/CHANGELOG.md`
  - `PROGRESS.md` → `docs/PROGRESS.md`
  - `ARCHITECTURE.md` → `docs/ARCHITECTURE.md`

- **Project References**: Updated App.xaml to reference new file locations

  - `StartupUri` updated to `src/Views/MainWindow.xaml`

- **Namespace Updates**: Updated class namespaces to match folder structure
  - `MainWindow` class now in `AITeacherAssistant.Views` namespace

### Fixed

- N/A (Restructure only)

### Files Modified

- `src/Views/MainWindow.xaml` (moved and updated)
- `src/Views/MainWindow.xaml.cs` (moved and updated)
- `App.xaml` (updated StartupUri)
- `docs/CHANGELOG.md` (moved and updated)
- `docs/PROGRESS.md` (moved)
- `docs/ARCHITECTURE.md` (moved)

---

## [Date: 2025-10-01 (Initial Session)]

### Added

- **Transparent Overlay Window**: Implemented a system-wide transparent overlay window that stays on top of all applications
  - Window is fullscreen, borderless, and transparent
  - `Topmost` property set to `true` for always-on-top behavior
  - `AllowsTransparency` enabled with transparent background
  - `ShowInTaskbar` set to `false` to keep taskbar clean
- **Click-Through Functionality**: Implemented Win32 API integration for click-through behavior
  - Mouse clicks pass through transparent areas to underlying applications
  - Uses `WS_EX_TRANSPARENT` extended window style
  - Win32 `GetWindowLong` and `SetWindowLong` API calls for window style manipulation
- **Keyboard Shortcut Toggle**: Added `Ctrl+Shift+Q` keyboard shortcut to toggle overlay on/off
  - Overlay can be hidden/shown without closing the application
  - Keyboard event handling in `MainWindow_KeyDown` method
- **Control Panel UI**: Added floating control panel in top-right corner
  - Dark semi-transparent background (#CC1E1E1E)
  - Application title display
  - Keyboard shortcut hint text
  - Close button with confirmation dialog
- **Status Indicator**: Added status indicator in bottom-left corner
  - Green dot when overlay is active
  - Red dot when overlay is inactive
  - Status text showing current state
- **Window Management**: Proper window lifecycle management
  - `MainWindow_Loaded` event handler for initialization
  - Window handle acquisition via `WindowInteropHelper`
  - Resource cleanup in `OnClosing` override

### Changed

- Modified `MainWindow.xaml` to support transparent overlay design
- Modified `MainWindow.xaml.cs` to add click-through and keyboard handling logic

### Fixed

- N/A (Initial implementation)

### Files Modified

- `MainWindow.xaml`
- `MainWindow.xaml.cs`
- `CHANGELOG.md` (created)
- `PROGRESS.md` (created)
- `ARCHITECTURE.md` (created)
