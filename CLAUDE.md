# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Is

**xstack** is a minimal Win32 kiosk-style desktop app (built as `xstack.exe`) that embeds a single hardcoded URL inside a native window using Microsoft WebView2. There is no address bar, no status bar, no DevTools, and no browser chrome — it renders one URL, full-screen in the client area.

The repo is named `WebViewApp` but the product name, namespace, and binary are all `xstack`.

## Build Commands

Requires Windows with Visual Studio 2022 (MSVC v143 toolset), NuGet CLI, and MSBuild on PATH.

```powershell
# Restore NuGet packages (Microsoft.Web.WebView2, WIL)
nuget restore WebViewApp.sln -PackagesDirectory packages

# Build Release x64
msbuild WebViewApp.sln /p:Configuration=Release /p:Platform=x64 /p:WebView2LoaderPreference=Static /m

# Build Release x86 (Win32)
msbuild WebViewApp.sln /p:Configuration=Release /p:Platform=Win32 /p:WebView2LoaderPreference=Static /m

# Debug build (x64)
msbuild WebViewApp.sln /p:Configuration=Debug /p:Platform=x64 /m
```

Output lands in `bin/{Platform}/{Configuration}/xstack.exe`. There are no automated tests.

## MSI Installer

The installer is WiX v3 (`installer/Product.wxs`). It is built by CI only — it requires `bin/x64/Release/xstack.exe` and `redist/MicrosoftEdgeWebview2Setup.exe` to be present before running `candle.exe` / `light.exe`. The MSI silently installs the WebView2 runtime bootstrapper if WebView2 is not already present on the target machine.

## CI / Release

GitHub Actions (`.github/workflows/build.yml`) runs three jobs in sequence:

1. **build** — builds Win32 and x64 in parallel, uploads each `xstack.exe` as an artifact
2. **installer** — downloads the x64 artifact + WebView2 bootstrapper, builds `xstack-x64.msi` with WiX
3. **release** — triggered only on `v*.*.*` tags (or manual dispatch with `create_release=true`); bundles `xstack-x86.exe`, `xstack-x64.exe`, and `xstack-x64.msi` into `xstack.zip` and publishes a GitHub Release

To cut a release:
```bash
git tag v1.2.3 && git push origin v1.2.3
```

## Architecture: src/main.cpp

The entire application is a single translation unit. Key globals:

- `g_controller` / `g_webview` — WRL COM smart pointers (`wil::com_ptr`) for the WebView2 controller and view
- `TARGET_URL` — the hardcoded URL loaded on startup (`http://172.16.5.114:20000`)
- `APP_NAME` — window class name and title (`xstack`)

**Initialization flow** (`InitWebView2`): fully async, callback-chain style — `CreateCoreWebView2EnvironmentWithOptions` → `CreateCoreWebView2Controller` → configure settings → register event handlers → `Navigate(TARGET_URL)`.

**WebView2 settings applied** (all to remove browser chrome):
- Context menus, DevTools, status bar, and zoom control disabled
- Script and default script dialogs enabled
- Built-in error page disabled

**Event handlers registered:**
- `NewWindowRequested` — intercepts `target=_blank` navigations and redirects them into the current view instead of opening a new window
- `NavigationStarting` → `AddScriptToExecuteOnDocumentCreated` — injects a script on every navigation that: blocks right-click context menu, blocks F12/Ctrl+Shift+I/J/C/U devtools shortcuts, blocks drag-and-drop, adds minor canvas fingerprint noise, and normalizes `navigator.hardwareConcurrency` (→4) and `navigator.deviceMemory` (→8)

**Window** is created at 1280×800 with `WS_OVERLAPPEDWINDOW`. `WM_SIZE` calls `ResizeWebView()` to keep the WebView filling the entire client rect. DPI awareness is set to `DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2`.

## Changing the Target URL or App Name

Edit the constants at the top of `src/main.cpp`:
```cpp
static const wchar_t* TARGET_URL = L"http://172.16.5.114:20000";
static const wchar_t* APP_NAME   = L"xstack";
```

`APP_NAME` is used as both the Win32 window class name and the title bar text. The product name in the version resource (`src/app.rc`) and the WiX installer (`installer/Product.wxs`) must be updated separately if renaming the product.

## Dependency Versions

Pinned in `packages.config` and mirrored in `.vcxproj` property defaults and the CI workflow env vars:
- `Microsoft.Web.WebView2`: `1.0.2903.40`
- `Microsoft.Windows.ImplementationLibrary` (WIL): `1.0.240803.1`

To upgrade, update all three locations consistently.
