using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

namespace HeatBot
{
    public static class ClickSimulate
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        public static void SimulateClick(int x, int y)
        {
            try
            {

                if (!SetCursorPos(x, y))
                {
                    throw new InvalidOperationException("Failed to move cursor to the specified position.");
                }
                Console.WriteLine($"Cursor moved to position: ({x}, {y})");

                Thread.Sleep(100);

                TriggerMouseClick(x, y);
                Console.WriteLine("Mouse click triggered.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during click simulation: {ex.Message}");
            }
        }

        public static void TriggerMouseClick(int x, int y)
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
            Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
        }
    }
}