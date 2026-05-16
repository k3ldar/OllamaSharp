using Microsoft.Maui.Controls;

namespace OllamaSharp;

public partial class MainWindow : Window
{
    private const string PrefKeyX = "WindowX";
    private const string PrefKeyY = "WindowY";
    private const string PrefKeyWidth = "WindowWidth";
    private const string PrefKeyHeight = "WindowHeight";

    private const double DefaultMinimumWidth = 400;
    private const double DefaultMinimumHeight = 500;
    private const double InvalidPosition = -1.0;
    private const double DefaultWindowWidth = 800.0;
    private const double DefaultWindowHeight = 600.0;
    private const double MinVisiblePixelsWidth = 100;
    private const double MinVisiblePixelsHeight = 50;
    private const double VisibleAreaPercentage = 0.3;

    public MainWindow(Page page) : base(page)
    {
        // Set minimum window size
        MinimumWidth = DefaultMinimumWidth;
        MinimumHeight = DefaultMinimumHeight;

        // Restore window size and position
        Created += OnWindowCreated;
        Destroying += OnWindowDestroying;
    }

    private void OnWindowCreated(object sender, EventArgs e)
    {
        // Load saved preferences
        var savedX = Preferences.Get(PrefKeyX, InvalidPosition);
        var savedY = Preferences.Get(PrefKeyY, InvalidPosition);
        var savedWidth = Preferences.Get(PrefKeyWidth, DefaultWindowWidth);
        var savedHeight = Preferences.Get(PrefKeyHeight, DefaultWindowHeight);

        // Apply saved size (always safe)
        Width = savedWidth;
        Height = savedHeight;

        // Check if saved position is valid (within screen bounds)
        if (savedX >= 0 && savedY >= 0)
        {
            var isPositionValid = IsPositionWithinScreenBounds(savedX, savedY, savedWidth, savedHeight);

            if (isPositionValid)
            {
                X = savedX;
                Y = savedY;
            }
            else
            {
                // Position is outside screen bounds, let the OS decide
                System.Diagnostics.Debug.WriteLine(Constants.DebugMsgWindowPosOutsideOfBounds);
            }
        }
    }

    private void OnWindowDestroying(object sender, EventArgs e)
    {
        // Save current window position and size
        Preferences.Set(PrefKeyX, X);
        Preferences.Set(PrefKeyY, Y);
        Preferences.Set(PrefKeyWidth, Width);
        Preferences.Set(PrefKeyHeight, Height);

        System.Diagnostics.Debug.WriteLine($"Saved window: X={X}, Y={Y}, Width={Width}, Height={Height}");
    }

    private static bool IsPositionWithinScreenBounds(double x, double y, double width, double height)
    {
        try
        {
            // Get the display information
            var mainDisplay = DeviceDisplay.Current.MainDisplayInfo;

            // Convert display density to actual pixels
            var screenWidth = mainDisplay.Width / mainDisplay.Density;
            var screenHeight = mainDisplay.Height / mainDisplay.Density;

            // Check if at least 100px of the window is visible on screen
            // This ensures the window title bar is accessible
            var minVisibleWidth = Math.Min(MinVisiblePixelsWidth, width * VisibleAreaPercentage);
            var minVisibleHeight = Math.Min(MinVisiblePixelsHeight, height * VisibleAreaPercentage);

            var rightEdge = x + width;
            var bottomEdge = y + height;

            // Window must have some visible area on screen
            var isVisibleHorizontally = (x + minVisibleWidth) >= 0 && x < screenWidth;
            var isVisibleVertically = (y + minVisibleHeight) >= 0 && y < screenHeight;

            return isVisibleHorizontally && isVisibleVertically;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error checking screen bounds: {ex.Message}");
            // If we can't determine, assume it's safe to use the saved position
            return true;
        }
    }
}
