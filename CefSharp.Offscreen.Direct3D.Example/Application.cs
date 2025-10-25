namespace CefSharp.Offscreen.Direct3D.Example;

public class Application
{
    public static Uri Url { get; set; } = new("https://github.com/cefsharp/CefSharp.OnAcceleratedPaint.Example");
    private readonly OffscreenBrowser _browser;
    public Application()
    {
        var cefBrowserSettings = new BrowserSettings
        {
            BackgroundColor = Cef.ColorSetARGB(255, 255, 255, 255),
            WebGl = CefState.Enabled,
            WindowlessFrameRate = 60,
            LocalStorage = CefState.Default
        };
        
        _browser = new OffscreenBrowser(1920, 1080, Url.AbsoluteUri, cefBrowserSettings);
    }
    
    public void Close()
    {
        _browser.Stop();
        _browser.Dispose();
        Cef.Shutdown();
    }
}