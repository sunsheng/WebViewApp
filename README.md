# WebViewApp

A minimal Win32 + WebView2 desktop client that hosts a web URL in a native window — no address bar, no status bar, no DevTools.

## Project Structure

```
WebViewApp/
├── .github/workflows/build.yml
├── src/
│   ├── main.cpp
│   ├── app.rc
│   └── resource.h
├── installer/
│   ├── Product.wxs
│   └── License.rtf
├── WebViewApp.vcxproj
├── WebViewApp.sln
└── packages.config
```

## Building Locally

```powershell
nuget restore WebViewApp.sln -PackagesDirectory packages
msbuild WebViewApp.sln /p:Configuration=Release /p:Platform=x64 /p:WebView2LoaderPreference=Static /m
```

## Release

```bash
git tag v1.0.0 && git push origin v1.0.0
```
