# Dijkstra Path Finder

A Windows Forms desktop application that lets you build a weighted graph and watch
**Dijkstra's shortest-path algorithm** run step by step, wrapped in a modern
glassmorphic dark-theme UI. It includes an animated splash screen and a
local username/password authentication layer backed by a JSON file.

---

## Table of Contents
- [Description](#description)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [How to Build and Run](#how-to-build-and-run)
- [Default Credentials](#default-credentials)
- [File Overview](#file-overview)
- [Application Flow](#application-flow)
- [Bug Fixes Applied](#bug-fixes-applied)

---

## Description

Create nodes by clicking on the canvas, connect them with weighted edges,
pick a **source** and a **target**, then run Dijkstra to visualize the search:
visited nodes, the current node, tentative edges, the final shortest path, and
any equal-cost alternative paths are all color-coded and animated in real time.

Users authenticate (or register) on a login screen before reaching the graph
editor. Credentials are stored locally in `Users.json` with **SHA-256 hashed
passwords** (no plaintext).

---

## Features

### Authentication
- Register and login from a single toggleable form.
- Username rules: 2–30 characters, letters / digits / underscores only.
- Password rules: minimum 4 characters.
- Case-insensitive username matching; passwords stored as SHA-256 hashes.
- User data persisted to `Users.json`.

### Graph editing
- Add unlimited nodes anywhere on the canvas.
- Connect nodes with weighted, directional edges (weight entered via dialog).
- Designate one source and one target node.
- Clear just the visualization, or clear the whole graph.

### Dijkstra visualization
- Step-by-step animation on a 300 ms timer.
- Color-coded node states: unvisited (purple), visited (cyan), current (gold),
  source (red, "S"), target (green, "T").
- Edge states: normal (gray), tentative (orange), shortest path (green),
  equal-cost alternative path (dashed orange).
- Result dialog reports total distance, the path, and alternative-path count.

### UI / UX
- Glassmorphic dark theme with rounded, gradient panels.
- Animated splash screen with logo scale-in and progress bar.
- Borderless, draggable windows with custom minimize/close controls.
- Button hover effects and status-bar feedback for every action.

---

## Tech Stack

| Aspect        | Detail                                             |
|---------------|----------------------------------------------------|
| Language      | C#                                                 |
| UI Framework  | Windows Forms (WinForms)                           |
| Target        | `net8.0-windows` (.NET 8, SDK-style project)       |
| Rendering     | System.Drawing / GDI+ custom painting              |
| Persistence   | `System.Text.Json` over a local `Users.json` file  |
| Hashing       | SHA-256 (`System.Security.Cryptography`)           |

> Requires the **.NET 8 SDK** to build and the .NET 8 Desktop Runtime to run.
> WinForms is Windows-only.

---

## How to Build and Run

```bash
# From the project folder
cd WinFormsApp1

# Restore + build
dotnet build

# Run the app
dotnet run
```

### Using Visual Studio
1. Open `WinFormsApp1.slnx` (or the `.csproj`) in Visual Studio.
2. Set `WinFormsApp1` as the startup project.
3. Press F5.

---

## Default Credentials

There are **no pre-seeded accounts** — `Users.json` ships as an empty array `[]`.
On first run, click **Register** to create your own account, then log in with it.
Newly registered credentials are saved (hashed) to `Users.json` in the
application's output directory.

---

## File Overview

```
WinFormsApp1/
├── Program.cs           # Entry point; sequences Splash → Login → Main
├── SplashForm.cs        # Animated splash screen
├── LoginForm.cs         # Login / Register form (mode toggle)
├── AuthHelper.cs        # User model + auth logic (load/save/hash/verify)
├── MainForm.cs          # Graph editor + Dijkstra visualization
├── Users.json           # Local user store (JSON array, SHA-256 hashes)
├── WinFormsApp1.csproj  # SDK-style project (net8.0-windows)
└── README.md
```

| File           | Responsibility                                                                 |
|----------------|--------------------------------------------------------------------------------|
| `Program.cs`   | `[STAThread]` entry point; runs splash, then login, then main form.            |
| `SplashForm.cs`| Loading animation, progress bar, fade-out, then closes.                        |
| `LoginForm.cs` | Collects credentials, validates input, calls `AuthHelper`, exposes `Username`. |
| `AuthHelper.cs`| `User` class + `Register`, `Login`, `UserExists`, hashing and JSON I/O.        |
| `MainForm.cs`  | `Node` / `Edge` / `OperationMode` types, canvas painting, Dijkstra timer loop. |

---

## Application Flow

```
Start ─▶ SplashForm (animated, auto-closes)
      ─▶ LoginForm  (register or login; sets Username on success)
      ─▶ MainForm   (only if authenticated — receives Username)
```

If the login window is closed without a successful login, the app exits instead
of opening the main form.

---

## Bug Fixes Applied

The following real defects were found and fixed:

1. **Main form launched without authentication** (`Program.cs`)
   After the login window closed, the main form was created and run
   unconditionally — even if the user closed it via the X without ever logging
   in, passing a `null` username through. Added a guard that exits when
   `login.Username` is null/empty.

2. **Null-reference risk on malformed user records** (`AuthHelper.cs`)
   `Login`, `Register`, and `UserExists` called `u.Username.Equals(...)`, which
   throws if a deserialized record has a null `Username`. Switched to
   `string.Equals(...)` (null-safe) and added a null/empty `PasswordHash` check
   in `Login`.

3. **Fragile JSON deserialization** (`AuthHelper.cs`)
   `LoadUsers` deserialized with default (case-sensitive) options, so a
   hand-edited `Users.json` using camelCase keys would silently produce empty
   records. Added `PropertyNameCaseInsensitive = true`.

4. **Null-reference risk while reconstructing the shortest path** (`MainForm.cs`)
   In `ShowResult`, `node.PreviousNodeId` was read after a `TryGetValue` that
   could fail, dereferencing a null `node`. The loop now breaks safely when a
   node id is missing.

> Note: `Users.json` is resolved via `AppDomain.CurrentDomain.BaseDirectory`,
> so it is found regardless of the current working directory — this was already
> correct and required no change.
"# Dijkstra-Visualizer-.NET-8-" 
