using System.Runtime.InteropServices;

namespace Respite.Services;

public static class WindowActionService
{
    private const int WM_COMMAND = 0x0111;
    private const int MIN_ALL = 419;

    [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
    private static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);

    [DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

    public static void MinimizeAllWindows()
    {
        try
        {
            IntPtr lHwnd = FindWindow("Shell_TrayWnd", null);
            if (lHwnd != IntPtr.Zero)
            {
                SendMessage(lHwnd, WM_COMMAND, (IntPtr)MIN_ALL, IntPtr.Zero);
            }
        }
        catch
        {
            // TODO: logs
        }
    }
}

