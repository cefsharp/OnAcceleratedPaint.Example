using CefSharp.OffScreen;
using CefSharp.Structs;

namespace CefSharp.Offscreen.Direct3D.Example;

public class AcceleratedRenderHandler(ChromiumWebBrowser browser, int windowWidth, int windowHeight) : DefaultRenderHandler(browser)
{
    private D3D11Renderer _renderer = null!;

    public override void OnAcceleratedPaint(PaintElementType type, Rect dirtyRect, AcceleratedPaintInfo acceleratedPaintInfo)
    {
        base.OnAcceleratedPaint(type, dirtyRect, acceleratedPaintInfo);
        _renderer ??= new D3D11Renderer(windowWidth, windowHeight);
        switch (type)
        {
            case PaintElementType.View:
                _renderer?.OnAcceleratedPaint(acceleratedPaintInfo.SharedTextureHandle, PopupOpen);
                break;
            case PaintElementType.Popup:
                _renderer?.CreatePopupLayer(acceleratedPaintInfo.SharedTextureHandle, new Rect(PopupPosition.X, PopupPosition.Y, PopupSize.Width, PopupSize.Height));
                break;
        }
    }

    public void BrowserWasResized(int width, int height)
    {
        _renderer?.ResizeWindow(width, height);
    }
    
    public override void OnPopupShow(bool show)
    {
        base.OnPopupShow(show);
        if(show) return;
        _renderer?.RemovePopupLayer();
    }

    public new void Dispose()
    {
        base.Dispose();
        _renderer?.Dispose();
    }
}