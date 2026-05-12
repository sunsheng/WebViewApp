#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <wrl.h>
#include <wil/com.h>
#include <WebView2.h>
#include <string>

using namespace Microsoft::WRL;

static wil::com_ptr<ICoreWebView2Controller> g_controller;
static wil::com_ptr<ICoreWebView2>           g_webview;
static HWND g_hwnd = nullptr;

static const wchar_t* TARGET_URL = L"http://172.16.5.114:20000";
static const wchar_t* APP_NAME   = L"xstack";

static void ResizeWebView()
{
    if (!g_controller) return;
    RECT rc;
    GetClientRect(g_hwnd, &rc);
    g_controller->put_Bounds(rc);
}

static void InitWebView2(HWND hwnd)
{
    CreateCoreWebView2EnvironmentWithOptions(
        nullptr, nullptr, nullptr,
        Callback<ICoreWebView2CreateCoreWebView2EnvironmentCompletedHandler>(
            [hwnd](HRESULT hr, ICoreWebView2Environment* env) -> HRESULT {
                if (FAILED(hr)) return hr;
                env->CreateCoreWebView2Controller(
                    hwnd,
                    Callback<ICoreWebView2CreateCoreWebView2ControllerCompletedHandler>(
                        [](HRESULT hr, ICoreWebView2Controller* ctrl) -> HRESULT {
                            if (FAILED(hr)) return hr;

                            g_controller = ctrl;
                            g_controller->get_CoreWebView2(&g_webview);

                            // ---- settings: strip browser chrome ----
                            wil::com_ptr<ICoreWebView2Settings> settings;
                            g_webview->get_Settings(&settings);
                            settings->put_AreDefaultContextMenusEnabled(FALSE);
                            settings->put_AreDevToolsEnabled(FALSE);
                            settings->put_IsStatusBarEnabled(FALSE);
                            settings->put_IsZoomControlEnabled(FALSE);
                            settings->put_AreDefaultScriptDialogsEnabled(TRUE);
                            settings->put_IsScriptEnabled(TRUE);
                            settings->put_IsBuiltInErrorPageEnabled(FALSE);

                            // ---- redirect target=_blank links into current view ----
                            EventRegistrationToken tok{};
                            g_webview->add_NewWindowRequested(
                                Callback<ICoreWebView2NewWindowRequestedEventHandler>(
                                    [](ICoreWebView2* sender,
                                       ICoreWebView2NewWindowRequestedEventArgs* args) -> HRESULT {
                                        wil::unique_cotaskmem_string uri;
                                        PCWSTR p = nullptr;
                                        if (SUCCEEDED(args->get_Uri(&uri)))
                                            p = uri.get();
                                        if (p && *p)
                                            g_webview->Navigate(p);
                                        args->put_Handled(TRUE);
                                        return S_OK;
                                    })
                                    .Get(),
                                &tok);

                            // ---- block drag-drop onto webview ----
                            g_webview->add_WebMessageReceived(nullptr, &tok);

                            // ---- inject fingerprint noise + disable drag ----
                            g_webview->add_NavigationStarting(
                                Callback<ICoreWebView2NavigationStartingEventHandler>(
                                    [](ICoreWebView2* sender,
                                       ICoreWebView2NavigationStartingEventArgs* args) -> HRESULT {
                                        const wchar_t* script =
                                            L"(function(){"
                                            L"document.addEventListener('contextmenu',function(e){e.preventDefault();},true);"
                                            L"document.addEventListener('keydown',function(e){"
                                            L"  if(e.key==='F12'||(e.ctrlKey&&e.shiftKey&&(e.key==='I'||e.key==='J'||e.key==='C'))||"
                                            L"     (e.ctrlKey&&e.key==='U')){e.preventDefault();e.stopPropagation();}"
                                            L"},true);"
                                            L"document.addEventListener('dragstart',function(e){e.preventDefault();},true);"
                                            L"document.addEventListener('drop',function(e){e.preventDefault();},true);"
                                            L"(function(){"
                                            L"  const orig=HTMLCanvasElement.prototype.toDataURL;"
                                            L"  HTMLCanvasElement.prototype.toDataURL=function(type){"
                                            L"    const ctx=this.getContext('2d');"
                                            L"    if(ctx){ctx.fillStyle='rgba(0,0,0,0.01)';ctx.fillRect(0,0,1,1);}"
                                            L"    return orig.apply(this,arguments);"
                                            L"  };"
                                            L"})();"
                                            L"Object.defineProperty(navigator,'hardwareConcurrency',{get:function(){return 4;}});"
                                            L"try{Object.defineProperty(navigator,'deviceMemory',{get:function(){return 8;}});}catch(e){}"
                                            L"})();";
                                        sender->AddScriptToExecuteOnDocumentCreated(script, nullptr);
                                        return S_OK;
                                    })
                                    .Get(),
                                &tok);

                            ResizeWebView();
                            g_webview->Navigate(TARGET_URL);
                            return S_OK;
                        })
                        .Get());
                return S_OK;
            })
            .Get());
}

static LRESULT CALLBACK WndProc(HWND hwnd, UINT msg, WPARAM wp, LPARAM lp)
{
    switch (msg) {
    case WM_SIZE:
        ResizeWebView();
        return 0;
    case WM_DESTROY:
        PostQuitMessage(0);
        return 0;
    }
    return DefWindowProcW(hwnd, msg, wp, lp);
}

int WINAPI wWinMain(HINSTANCE hInst, HINSTANCE, PWSTR, int nCmdShow)
{
    SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);

    WNDCLASSEXW wc{};
    wc.cbSize        = sizeof(wc);
    wc.lpfnWndProc   = WndProc;
    wc.hInstance     = hInst;
    wc.hCursor       = LoadCursorW(nullptr, IDC_ARROW);
    wc.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1);
    wc.lpszClassName = APP_NAME;
    wc.hIcon         = LoadIconW(hInst, MAKEINTRESOURCEW(101));
    RegisterClassExW(&wc);

    HWND hwnd = CreateWindowExW(
        0, APP_NAME, APP_NAME,
        WS_OVERLAPPEDWINDOW,
        CW_USEDEFAULT, CW_USEDEFAULT, CW_USEDEFAULT, CW_USEDEFAULT,
        nullptr, nullptr, hInst, nullptr);

    g_hwnd = hwnd;
    ShowWindow(hwnd, SW_SHOWMAXIMIZED);
    UpdateWindow(hwnd);

    InitWebView2(hwnd);

    MSG m;
    while (GetMessageW(&m, nullptr, 0, 0)) {
        TranslateMessage(&m);
        DispatchMessageW(&m);
    }
    return (int)m.wParam;
}
