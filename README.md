# NovaProject

> ⚠️ **Work in Progress** — This project is in active early development. Some features are partially stubbed or not yet implemented.

A cross-platform desktop messaging client built with **Avalonia UI** and **.NET 10**. The application follows a **process-isolated architecture**, splitting the UI and backend logic into two separate executables (`Client` and `Daemon`) that communicate through a **Named Pipe IPC channel** using newline-delimited JSON messages.

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Project Breakdown](#project-breakdown)
   - [NovaProject.Client](#novaprojectclient)
   - [NovaProject.Daemon](#novaprojectdaemon)
   - [NovaProject.Core](#novaprojectcore)
3. [IPC Protocol](#ipc-protocol)
4. [Tech Stack](#tech-stack)
5. [Prerequisites](#prerequisites)
6. [Build & Run](#build--run)
7. [Project Status](#project-status)
8. [License](#license)

---

## Architecture Overview

The solution is structured around three projects with clear boundaries:

```
NovaProject/
├── NovaProject.Client/     # Avalonia UI desktop application
├── NovaProject.Core/       # Shared models, contracts, and services
└── NovaProject.Daemon/     # Headless background worker (IPC server)
```

The **Client** and **Daemon** are fully separate OS processes. When the Client starts, it automatically launches the Daemon binary and connects to it over a local Named Pipe. All heavy work (data persistence, relay communication) is delegated to the Daemon, keeping the UI process lightweight and responsive.

```
┌─────────────────────────────┐        Named Pipe (JSON)        ┌──────────────────────────────┐
│      NovaProject.Client     │ ──────────────────────────────► │     NovaProject.Daemon       │
│       (Avalonia UI)         │ ◄────────────────────────────── │   (BackgroundService / EFCore)│
└─────────────────────────────┘                                 └──────────────────────────────┘
              │                                                               │
              └───────────────────────────────────────────────────────────────┘
                                   NovaProject.Core
                          (Shared contracts, models, utilities)
```

---

## Project Breakdown

### NovaProject.Client

The user-facing desktop application built with **Avalonia 11** following the **MVVM** pattern via `CommunityToolkit.Mvvm`.

**Startup sequence:**
1. Builds the DI container (registers `DaemonBridgeService` and `ChatService` as singletons).
2. `DaemonBridgeService` kills any stale Daemon ghost processes, then launches the Daemon binary.
3. A Named Pipe connection is established; two concurrent async tasks handle reading and writing.
4. `MainWindowViewModel` subscribes to `ChatService` events and sets up the initial UI state.

---

#### Services

| Service | Description |
|---|---|
| **`DaemonBridgeService`** | The low-level IPC layer. Manages the Named Pipe lifecycle, serializes outgoing `DaemonServerRequest` objects to JSON, and deserializes incoming `DaemonServerResponse` objects. Outgoing requests are queued in a `ConcurrentQueue` and flushed by a dedicated send task. On startup it also handles finding and killing zombie Daemon processes before spawning a fresh one. |
| **`ChatService`** | The high-level messaging API consumed by the UI. Translates user actions (selecting a contact, sending a message, switching servers) into Daemon requests and exposes events (`OnMessageReceived`, `OnUserListUpdated`, `OnConversationSwitch`, `OnCallOutbound`, etc.) that the ViewModel layer subscribes to. Also maintains a cache of open `ChatBodyViewModel` instances keyed by `LocalUid`. |

---

#### ViewModels

| ViewModel | Description |
|---|---|
| **`MainWindowViewModel`** | Root ViewModel for the application window. Manages the sidebar tab state (Users vs. Servers), the active `ChatBodyViewModel`, `ChatInputViewModel`, and `ChatTitlebarViewModel`. Subscribes to `ChatService` events to react to conversation switches and incoming calls. |
| **`ViewModelBase`** | Thin base class inheriting from `ObservableObject` (CommunityToolkit). |

---

#### Custom Controls

All controls live under `CustomControls/` and are reusable Avalonia `UserControl` components backed by their own ViewModels in `CustomControls/ViewModels/`.

| Control | Description |
|---|---|
| **`ChatBody`** | Scrollable message feed. Renders the list of messages for the active conversation. |
| **`ChatInput`** | Text input bar at the bottom of the chat. Handles message composition and dispatches send commands to `ChatService`. |
| **`ChatTitlebar`** | Top bar showing the active contact's name, avatar, and action buttons (e.g. call). |
| **`UserList`** | Sidebar list displaying contacts or servers. Supports switching between a **People** tab and a **Servers** tab. Clicking an item triggers a conversation switch via `ChatService`. |
| **`UserIconImage`** | Small reusable avatar/icon widget used across the contact list and titlebar. |
| **`ChatBubble` / `ChatBubbleOther`** | Individual message bubble controls — `ChatBubble` for outgoing messages and `ChatBubbleOther` for incoming messages, with mirrored layouts. |

---

### NovaProject.Daemon

A headless **.NET Worker Service** (`Microsoft.Extensions.Hosting`) that acts as the IPC server and data backend. It runs as a completely separate OS process.

**Key behaviours:**
- A **Mutex** (`NovaProject.Daemon.Lock`) guarantees only one instance runs at a time. If the Client detects a stale process it kills it before launching a new one.
- Runs a persistent Named Pipe server loop, accepting one client connection at a time.
- Responses are queued in a `ConcurrentQueue<DaemonServerResponse>` and flushed asynchronously, decoupling request handling from pipe I/O.
- A 5-second back-off prevents rapid error loops if the pipe crashes.

---

#### Services

| Service | Description |
|---|---|
| **`DaemonWorker`** | The core `BackgroundService`. Owns the Named Pipe server (`NovaProject_Pipe`). Runs two concurrent async tasks per connection: one for reading requests and one for writing responses. Dispatches received requests to handler methods and enqueues the resulting responses. Handles `Ping`, `LoadServerList`, `LoadLocalUserList`, `MessageOutbound` request types. The outbound message flow currently rebounds the message back to the UI as a debug echo (real relay forwarding is a stub). |
| **`DaemonDataService`** | Data access layer on top of EF Core. Provides `SaveMessage`, `GetReceivedMessages`, and `GetSentMessages` operations. Messages are stored in a local SQLite database keyed by sender/recipient `LocalUid` string representations and relay address. `EditMessage` and `DeleteMessage` are declared but not yet implemented. |

---

#### Database

| File | Description |
|---|---|
| **`DaemonDbContext`** | EF Core `DbContext` exposing the `Messages` (`DbSet<StoredMessage>`) table. |
| **`StoredMessage`** | Entity class representing a persisted message, storing timestamp, content, sender/receiver name, tag, relay address, and formatted `To`/`From` strings. |
| **`Migrations/`** | Auto-generated EF Core migration history. |

---

### NovaProject.Core

A shared class library (no executable output) referenced by both the Client and Daemon. Acts as the **contract layer** for the whole solution.

---

#### Infrastructure — IPC Contracts (`Infrastructure/Daemon/`)

These types define the JSON shape of all messages exchanged over the Named Pipe.

| Type | Description |
|---|---|
| **`DaemonServerRequest`** | DTO sent from Client to Daemon. Carries a `RequestType` enum, an optional `RelayServer` address, and a `Payload` string (JSON sub-document). |
| **`DaemonServerResponse`** | DTO sent from Daemon to Client. Carries a `ResponseType` enum and a `Payload` string. |
| **`DaemonRequestType`** | Enum: `Ping`, `LoadServerList`, `LoadLocalUserList`, `LoadUserConversation`, `MessageOutbound` |
| **`DaemonResponseType`** | Enum: `PingSuccess`, `LoadUserList`, `LoadServerList`, `MessageInbound` |

---

#### Infrastructure — Domain Models (`Infrastructure/Local/`)

| Type | Description |
|---|---|
| **`LocalUid`** | Immutable `record` representing a user identity: `Name`, `UserName`, `Tag`, and `RelayAddress`. Implements `IEquatable<User>` so it can be compared directly against `User` objects. Its `ToString()` produces a canonical `[Name#Tag@Relay;]` string used as a database key. |
| **`MessageData`** | Immutable `struct` representing a single message with `Message` text, `Content` (media/rich content, currently unused), `Sender` (`LocalUid`), and `Recipient` (`LocalUid`). |

---

#### Infrastructure — UI Abstractions (`Infrastructure/UI/`)

In-memory representations of messages used inside the UI layer.

| Type | Description |
|---|---|
| **`MessageIo`** | Base message object held in the UI. Stores `Text`, `Content`, `Sender`, `Recipient`, `Time`, and an `Edited` flag. |
| **`TextMessage`** | Specialisation of `MessageIo` for plain-text messages. |
| **`ImageMessage`** | Specialisation of `MessageIo` for image messages. |
| **`CallNotify`** | Represents a call notification event in the message feed. |

---

#### Infrastructure — Shared Entities

| Type | Description |
|---|---|
| **`User`** | Full user model with `Name`, `DisplayName`, `Tag`, and `RelayAddress`. |
| **`ServerData`** | Represents a chat server entry (name, address). |
| **`UserListDisplayItem`** | Abstract base class used as a common type for items in the sidebar list, allowing `User` and `ServerData` to be displayed in the same collection. |

---

#### Services

| Service | Description |
|---|---|
| **`Logger`** | Static utility logger using `[CallerMemberName]` and `[CallerFilePath]` attributes to automatically capture call site information without any manual tagging. Writes prefixed lines to `stdout`: `[?]` for info, `[!]` for errors, `[!!!]` for critical. A `WriteLogFile` method is declared but not yet implemented. |
| **`NotificationService`** | Cross-platform desktop notification wrapper. Uses `FreeDesktopNotificationManager` on Linux and `WindowsNotificationManager` on Windows, with a configurable display duration from `AppGlobalService`. |
| **`AppGlobalService`** | Holds the currently logged-in `User` and global constants (e.g. `MessageNotificationTime`). |

---

## IPC Protocol

All messages are **newline-delimited JSON** sent over a local Named Pipe (`NovaProject_Pipe`).

**Request** (Client → Daemon):

```json
{
  "RequestType": "MessageOutbound",
  "RelayServer": "",
  "Payload": "{ \"Message\": \"Hello!\", \"Sender\": { ... }, \"Recipient\": { ... } }"
}
```

**Response** (Daemon → Client):

```json
{
  "ResponseType": "MessageInbound",
  "Payload": "{ \"Message\": \"Hello!\", \"Sender\": { ... }, \"Recipient\": { ... } }"
}
```

### Request / Response Type Reference

| Request | Daemon Handler | Response |
|---|---|---|
| `Ping` | `RespondToPingRequest` | `PingSuccess` (current time as payload) |
| `LoadServerList` | `RespondToServerListRequest` | `LoadServerList` (JSON list of `ServerData`) |
| `LoadLocalUserList` | `RespondToUserListRequest` | `LoadUserList` (JSON list of `User`) |
| `LoadUserConversation` | *(stub)* | — |
| `MessageOutbound` | `RespondToPrivateMessageOutgoing` | `MessageInbound` (debug echo — real relay is a stub) |

---

## Tech Stack

| Area | Technology | Version |
|---|---|---|
| UI Framework | [Avalonia UI](https://avaloniaui.net/) | 11.3 |
| MVVM Toolkit | CommunityToolkit.Mvvm | 8.2 |
| Target Framework | .NET | 10.0 |
| Database | SQLite via Entity Framework Core | 10.0 |
| IPC | .NET Named Pipes (`System.IO.Pipes`) | — |
| Desktop Notifications | DesktopNotifications | 1.3 |
| Real-time *(planned)* | ASP.NET Core SignalR Client | 10.0 |
| Animations | brokiem.Egolds.Avalonia.Xaml.Interactions.Animated | 11.2 |

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

---

## Build & Run

> **Important:** Build the **Daemon first**. The Client has a post-build MSBuild target that copies the Daemon binary into its output directory under `Daemon/`. If the Daemon is not built first, the Client won't be able to launch it.

```bash
# 1. Restore all dependencies
dotnet restore

# 2. Build the Daemon
dotnet build NovaProject.Daemon/NovaProject.Daemon.csproj

# 3. Build and run the Client
dotnet run --project NovaProject.Client/NovaProject.Client.csproj
```

The Client automatically spawns the Daemon process on first launch. No manual Daemon startup is required.

---

## Project Status

The project is in **active early development**. Core IPC plumbing and UI scaffolding are functional, but several features are stubs:

| Feature | Status |
|---|---|
| Named Pipe IPC (Client ↔ Daemon) | ✅ Working |
| User & server list loading | ✅ Working (placeholder data) |
| Chat UI (bubbles, input, titlebar) | ✅ Scaffolded |
| Message send / receive (local echo) | ✅ Debug echo working |
| Message persistence (SQLite) | 🚧 Schema defined, write path implemented, not yet wired up |
| Relay server forwarding | 🚧 Stub (`ForwardMessageToRelay` logs only) |
| `LoadUserConversation` request | 🚧 Not yet handled by Daemon |
| SignalR real-time relay | 🚧 Dependency added, not yet integrated |
| `Logger.WriteLogFile()` | 🚧 Not implemented |
| Call support | 🚧 Event plumbing exists, UI/logic not implemented |

---

## License

*TBA*
