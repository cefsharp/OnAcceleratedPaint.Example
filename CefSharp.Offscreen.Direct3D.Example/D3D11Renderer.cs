using System.Runtime.InteropServices;
using CefSharp.Structs;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.WIC;
using Adapter = SharpDX.DXGI.Adapter;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using Device1 = SharpDX.Direct3D11.Device1;
using Device2 = SharpDX.Direct3D11.Device2;
using Device3 = SharpDX.Direct3D11.Device3;
using Device4 = SharpDX.Direct3D11.Device4;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Point = SharpDX.Point;
using SampleDescription = SharpDX.DXGI.SampleDescription;
using Vector2 = SharpDX.Vector2;
using Vector4 = SharpDX.Vector4;
using Viewport = SharpDX.Viewport;

namespace CefSharp.Offscreen.Direct3D.Example;

/// <summary>
/// D3D11 renderer for CefSharp.Offscreen.Direct3D.Example
/// </summary>
public class D3D11Renderer : IDisposable
{
    private Texture2D _sharedTexture;
    private Device _device;
    private Adapter _adapter;
    private static VertexDX11[] s_vertices;
    private int _width;
    private int _height;
    private VertexBufferBinding _binding;
    private Texture2D _popupLayer;
    private Point _popupPosition;
    private const string ShaderSrc = "Texture2D tex;   \n" +
                                     "   \n" +
                                     "SamplerState texSampler   \n" +
                                     "{   \n" +
                                     "   Texture = <tex>;   \n" +
                                     "   Filter = MIN_MAG_MIP_POINT; \n" +
                                     "};   \n" +
                                     "   \n" +
                                     "struct PS_IN   \n" +
                                     "{  \n" +
                                     "   float4 pos : SV_POSITION; \n" +
                                     "   float2 Tex : TEXCOORD;  \n" +
                                     "};   \n" +
                                     "   \n" +
                                     "PS_IN vertex( PS_IN input )    \n" +
                                     "{  \n" +
                                     "   return input;   \n" +
                                     "}  \n" +
                                     "   \n" +
                                     "float4 pixel( PS_IN input ) : SV_Target  \n" +
                                     "{  \n" +
                                     "   return tex.Sample( texSampler, input.Tex );   \n" +
                                     "}  \n" +
                                     "   \n";

    [StructLayout(LayoutKind.Sequential)]
    private struct VertexDX11
    {
        public Vector4 Position;
        public Vector2 TexCoord0;

        public static InputElement[] GetInputElements()
        {
            InputElement[] inputElements = [new("SV_POSITION", 0, Format.R32G32B32A32_Float, 0, 0), new("TEXCOORD", 0, Format.R32G32_Float, 16, 0)];
            return inputElements;
        }
    }

    private void CreateDevice()
    {
        CreateAdapter();
        UpgradeAdapter();
        _device = new Device(_adapter, DeviceCreationFlags.BgraSupport, FeatureLevel.Level_11_1);
        
        UpgradeDevice();
    }

    private void CreateAdapter()
    {
        using var f = new Factory1();
        _adapter = f.GetAdapter(0);
    }

    public D3D11Renderer(int windowWidth, int windowHeight)
    {
        _width = windowWidth;
        _height = windowHeight;
        s_vertices = [new VertexDX11 {Position = new Vector4(-1, 1, 0, 1), TexCoord0 = new Vector2(0, 0)}, new VertexDX11 {Position = new Vector4(3, 1, 0, 1), TexCoord0 = new Vector2(2, 0)}, new VertexDX11 {Position = new Vector4(-1, -3, 0, 1), TexCoord0 = new Vector2(0, 2)}];
        //order is important
        CreateDevice();
        CreateSharedTexture();
        CreateRenderTarget();
        CreateShaders();

        _device.ImmediateContext.Flush();
    }

    private void CreateSharedTexture()
    {
        var sharedTextureDescription = new Texture2DDescription
        {
            ArraySize = 1,
            BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
            CpuAccessFlags = CpuAccessFlags.None,
            Format = Format.B8G8R8A8_UNorm,
            Height = _height,
            MipLevels = 1,
            OptionFlags = ResourceOptionFlags.Shared | ResourceOptionFlags.SharedNthandle,
            SampleDescription = new SampleDescription(1, 0),
            Usage = ResourceUsage.Default,
            Width = _width
        };
        _sharedTexture = new Texture2D(_device as Device1, sharedTextureDescription);
    }

    private void CreateRenderTarget()
    {
        using var renderTarget = new RenderTargetView(_device as Device1, _sharedTexture);
        _device.ImmediateContext.ClearRenderTargetView(renderTarget, Color4.Black);
        _device.ImmediateContext.OutputMerger.SetRenderTargets(renderTarget);
        var viewPort = new Viewport(0, 0, _width, _height);
        _device.ImmediateContext.Rasterizer.SetViewport(viewPort);
    }

    private void UpgradeDevice()
    {
        var device5 = _device.QueryInterfaceOrNull<Device5>();
        if (device5 != null)
        {
            _device.Dispose();
            _device = device5;
            return;
        }

        var device4 = _device.QueryInterfaceOrNull<Device4>();
        if (device4 != null)
        {
            _device.Dispose();
            _device = device4;
            return;
        }

        var device3 = _device.QueryInterfaceOrNull<Device3>();
        if (device3 != null)
        {
            _device = device3;
            return;
        }

        var device2 = _device.QueryInterfaceOrNull<Device2>();
        if (device2 != null)
        {
            _device = device2;
            return;
        }

        var device1 = _device.QueryInterfaceOrNull<Device1>();
        if (device1 != null)
        {
            _device.Dispose();
            _device = device1;
        }
    }

    private void UpgradeAdapter()
    {
        var adapter4 = _adapter.QueryInterfaceOrNull<Adapter4>();
        if (adapter4 != null)
        {
            _adapter.Dispose();
            _adapter = adapter4;
            return;
        }

        var adapter3 = _adapter.QueryInterfaceOrNull<Adapter3>();
        if (adapter3 != null)
        {
            _adapter.Dispose();
            _adapter = adapter3;
            return;
        }

        var adapter2 = _adapter.QueryInterfaceOrNull<Adapter2>();
        if (adapter2 != null)
        {
            _adapter.Dispose();
            _adapter = adapter2;
            return;
        }

        var adapter1 = _adapter.QueryInterfaceOrNull<Adapter1>();
        if (adapter1 != null)
        {
            _adapter.Dispose();
            _adapter = adapter1;
        }
    }

    private void CreateShaders()
    {
        using (ShaderBytecode byteCode = ShaderBytecode.Compile(ShaderSrc, "vertex", "vs_4_0_level_9_1"))
        {
            using (var vs = new VertexShader(_device, byteCode))
            {
                _device.ImmediateContext.VertexShader.Set(vs);
            }
            
            var signature = ShaderSignature.GetInputSignature(byteCode);
            using (var layout = new InputLayout(_device, signature, VertexDX11.GetInputElements()))
            {
                _device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
                _device.ImmediateContext.InputAssembler.InputLayout = layout;
            }
        }

        using (ShaderBytecode byteCode = ShaderBytecode.Compile(ShaderSrc, "pixel", "ps_4_0_level_9_1"))
        {
            using (var ps = new PixelShader(_device, byteCode))
            {
                _device.ImmediateContext.PixelShader.Set(ps);
            }
        }
        
        using (var data = DataStream.Create(s_vertices, false, false))
        {
            var bufferDesc = new BufferDescription {BindFlags = BindFlags.VertexBuffer, SizeInBytes = s_vertices.Length * Marshal.SizeOf<VertexDX11>(), };
            using (var vb = new Buffer(_device, data, bufferDesc))
            {
                _binding = new VertexBufferBinding {Buffer = vb, Offset = 0, Stride = Marshal.SizeOf<VertexDX11>()};
            }
        }
    }

    public void OnAcceleratedPaint(IntPtr sharedTextureHandle, bool popupShown = false)
    {
        try
        {
            if (_device == null || _sharedTexture == null || sharedTextureHandle == IntPtr.Zero) return;
            if (_device is not Device1 {ImmediateContext: not null} dev1) return;
            using var cefTex = dev1.OpenSharedResource1<Texture2D>(sharedTextureHandle);
            if (cefTex == null) return;
            dev1.ImmediateContext.CopyResource(cefTex, _sharedTexture);
            if (popupShown && _popupLayer != null)//if we have a popup widget layer being shown, we need to copy it to the shared texture
            {
                dev1.ImmediateContext.CopySubresourceRegion(_popupLayer, 0, null, _sharedTexture, 0, _popupPosition.X, _popupPosition.Y);
            }
            dev1.ImmediateContext.Flush();
#if DEBUG
            SaveTextureToFile(dev1, _sharedTexture, _width, _height);//to actually show that the browser is working, let's create a texture from the paint event and save it to a file
#endif
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error copying resources from cef texture to d3d11: {e.Message}");
        }
    }

    /// <summary>
    /// Popup widget texture creation to draw over the next OnAcceleratedPaint
    /// </summary>
    /// <param name="popupHandle"></param>
    /// <param name="popupRect"></param>
    public void CreatePopupLayer(IntPtr popupHandle, Rect popupRect)
    {
        try
        {
            var sharedTextureDescription = new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Height = popupRect.Height,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.Shared,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                Width = popupRect.Width,
            };
            _popupLayer = new Texture2D(_device as Device1, sharedTextureDescription);
            _popupPosition = new Point(popupRect.X, popupRect.Y);
            
            var dev1 = _device as Device1;
            using var popupTex = dev1.OpenSharedResource1<Texture2D>(popupHandle);
            dev1.ImmediateContext.CopyResource(popupTex, _popupLayer);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error handling popup view: {e.Message}");
        }
    }

    public void RemovePopupLayer()
    {
        try
        {
            _popupLayer?.Dispose();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error disposing popup widget layer {e.Message}");
        }
    }
    
    public void ResizeWindow(int width, int height)
    {
        try
        {
            if (_width != width || _height != height)
            {
                _sharedTexture?.Dispose();
                _device.ImmediateContext.OutputMerger.GetRenderTargets(out var depthStencilViewRef);
                var targets = _device.ImmediateContext.OutputMerger.GetRenderTargets(1);
                foreach (var target in targets)
                {
                    target.Dispose();
                }
                depthStencilViewRef?.Dispose();
                _device.ImmediateContext.OutputMerger.ResetTargets();
                _width = width;
                _height = height;
                CreateSharedTexture();
                CreateRenderTarget();
                _device.ImmediateContext.Flush();
            }
            Console.WriteLine($"Resized window {width}x{height}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to resize window: {e}");
        }
    }

#if DEBUG
    private void SaveTextureToFile(Device1 device1, Texture2D source, int width, int height)
    {
        var readableTextureDescription = new Texture2DDescription
        {
            Width = width,
            Height = height,
            MipLevels = 1,
            ArraySize = 1,
            Format = Format.B8G8R8A8_UNorm,
            SampleDescription = new SampleDescription(1, 0),
            CpuAccessFlags = CpuAccessFlags.Read,
            Usage = ResourceUsage.Staging,
            BindFlags = BindFlags.None,
            OptionFlags = ResourceOptionFlags.None
        };

        using var readableTexture = new Texture2D(device1, readableTextureDescription);
        device1.ImmediateContext.CopyResource(source, readableTexture);

        var dataBox = device1.ImmediateContext.MapSubresource(readableTexture, 0, MapMode.Read, MapFlags.None, out var dataStream);

        if (dataStream != null)
        {
            var dataRectangle = new DataRectangle {DataPointer = dataStream.DataPointer, Pitch = dataBox.RowPitch};

            using var factory = new ImagingFactory();
            using var bitmap = new Bitmap(factory, readableTexture.Description.Width, readableTexture.Description.Height, PixelFormat.Format32bppBGRA, dataRectangle);
            using var fileStream = new FileStream(@$"{Application.Url.Host}.png", FileMode.OpenOrCreate);
            fileStream.Position = 0;
            using var bitmapEncoder = new PngBitmapEncoder(factory, fileStream);
            using var bitmapFrameEncode = new BitmapFrameEncode(bitmapEncoder);
            bitmapFrameEncode.Initialize();
            bitmapFrameEncode.SetSize(bitmap.Size.Width, bitmap.Size.Height);
            var pixelFormat = PixelFormat.FormatDontCare;
            bitmapFrameEncode.SetPixelFormat(ref pixelFormat);
            bitmapFrameEncode.WriteSource(bitmap);
            bitmapFrameEncode.Commit();
            bitmapEncoder.Commit();
        }

        device1.ImmediateContext.UnmapSubresource(readableTexture, 0);
    }
#endif

    public void Dispose()
    {
        _sharedTexture?.Dispose();
        _device?.Dispose();
        _adapter?.Dispose();
        _popupLayer?.Dispose();
        GC.SuppressFinalize(this);
    }
}