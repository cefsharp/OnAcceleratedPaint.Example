using CefSharp;
using CefSharp.OffScreen;
using CefSharp.Offscreen.Direct3D.Example;

Console.WriteLine("Hello, fellow dev!");

var settings = new CefSettings
{
    BackgroundColor = Cef.ColorSetARGB(255, 255, 255, 255),//let's set a white background
    WindowlessRenderingEnabled = true
};

var success = Cef.Initialize(settings, true);
if (!success)
{
    var exitCode = Cef.GetExitCode();

    throw new Exception($"Cef.Initialize failed with {exitCode}, check the log file for more details.");
}

var app = new Application();
Console.ReadKey();
app.Close();
Console.WriteLine("Bye!");