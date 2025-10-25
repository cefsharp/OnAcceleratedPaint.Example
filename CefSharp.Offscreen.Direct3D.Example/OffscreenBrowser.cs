using System.Drawing;
using CefSharp.OffScreen;

namespace CefSharp.Offscreen.Direct3D.Example;

public class OffscreenBrowser : ChromiumWebBrowser
{
    public OffscreenBrowser(int width, int height, string startUrl, IBrowserSettings cefBrowserSettings) : base(startUrl, cefBrowserSettings, automaticallyCreateBrowser: false, useLegacyRenderHandler: false)
    {
        Initialize(width, height, cefBrowserSettings);
    }

    private void Initialize(int width, int height, IBrowserSettings cefBrowserSettings)
    {
        try
        {
            var windowInfo = new WindowInfo {WindowlessRenderingEnabled = true, Width = width, Height = height};
            windowInfo.SharedTextureEnabled = true;
            windowInfo.SetAsWindowless(IntPtr.Zero);
            CreateBrowser(windowInfo, cefBrowserSettings);
            Size = new Size(width, height);
            RenderHandler = new AcceleratedRenderHandler(this, width, height);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error creating browser {e}");
        }
    }

    public void ResizeBrowser(int width, int height)
    {
        Size = new Size(width, height);
        if (RenderHandler is AcceleratedRenderHandler renderHandler)
            renderHandler.BrowserWasResized(width, height);
    }

    protected override void Dispose(bool disposing)
    {
        if (RenderHandler is AcceleratedRenderHandler renderHandler)
            renderHandler.Dispose();
        base.Dispose(disposing);
    }
}