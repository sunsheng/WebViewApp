# WebViewApp

A minimal Win32 + WebView2 desktop client that hosts a web URL in a native window — no address bar, no status bar, no DevTools.

## Project Structure

```
WebViewApp/
├── .github/
│   └── workflows/
│       └── build.yml          # CI/CD: build → MSI → release
├── src/
│   ├── main.cpp               # Win32 WinMain + WebView2 init
│   ├── app.rc                 # Version info + icon resource
│   └── resource.h             # Resource IDs
├── installer/
│   ├── Product.wxs            # WiX MSI definition
│   └── License.rtf            # EULA shown during install
├── WebViewApp.vcxproj         # MSBuild project (x86 + x64, static CRT)
├── WebViewApp.sln             # Visual Studio solution
└── packages.config            # NuGet: WebView2 + WIL
```

## Features

| Feature | Implementation |
|---------|---------------|
| Single-page kiosk window | Win32 `WS_OVERLAPPEDWINDOW`, WebView2 fills client area |
| No browser chrome | `AreDefaultContextMenusEnabled=FALSE`, `AreDevToolsEnabled=FALSE`, `IsStatusBarEnabled=FALSE` |
| Resize support | `WM_SIZE` → `controller->put_Bounds(rc)` |
| Block right-click | JS `contextmenu` event suppressed via `AddScriptToExecuteOnDocumentCreated` |
| Block F12 / DevTools | JS `keydown` filter injected on every navigation |
| Block drag & drop | JS `dragstart`/`drop` suppressed |
| Block new windows | `ICoreWebView2::add_NewWindowRequested` → `put_Handled(TRUE)` |
| Canvas fingerprint noise | Overrides `HTMLCanvasElement.prototype.toDataURL` |
| Static WebView2Loader | `WebView2LoaderPreference=Static` – no `WebView2Loader.dll` |
| Static CRT | `MultiThreaded` runtime – no MSVCRT DLL dependency |
| DPI awareness | `DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2` |

## Building Locally

### Prerequisites
- Visual Studio 2022 with "Desktop development with C++" workload
- NuGet CLI

```powershell
# Restore NuGet packages
nuget restore WebViewApp.sln -PackagesDirectory packages

# Build x64 Release
msbuild WebViewApp.sln /p:Configuration=Release /p:Platform=x64 /p:WebView2LoaderPreference=Static /m

# Build x86 Release
msbuild WebViewApp.sln /p:Configuration=Release /p:Platform=Win32 /p:WebView2LoaderPreference=Static /m
```

Output: `bin\x64\Release\WebViewApp.exe` and `bin\Win32\Release\WebViewApp.exe`

### Building the MSI

```powershell
# Download WebView2 bootstrapper
Invoke-WebRequest "https://go.microsoft.com/fwlink/p/?LinkId=2124703" -OutFile redist\MicrosoftEdgeWebview2Setup.exe

# Compile + link with WiX 3.14
cd installer
candle.exe -arch x64 -ext WixUIExtension -ext WixUtilExtension Product.wxs
light.exe  -ext WixUIExtension -ext WixUtilExtension Product.wixobj -out WebViewApp-x64.msi
```

## CI/CD

Push a tag to trigger a full build + release:

```bash
git tag v1.0.0
git push origin v1.0.0
```

GitHub Actions will:
1. Restore NuGet packages (cached)
2. Build x86 + x64 EXEs in parallel
3. Download the WebView2 Evergreen bootstrapper
4. Package everything into `WebViewApp-x64.msi` with WiX
5. Publish all three artifacts to GitHub Releases

## Customization

To change the target URL, edit `src/main.cpp`:

```cpp
static const wchar_t* TARGET_URL = L"https://www.baidu.com";
```

To change the app name / manufacturer, update:
- `src/app.rc` — `ProductName`, `CompanyName`
- `installer/Product.wxs` — `Name`, `Manufacturer`
- `WebViewApp.vcxproj` — `RootNamespace`
