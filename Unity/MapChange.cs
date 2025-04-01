using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using OpenCvSharp;
using Point = System.Drawing.Point;

namespace HeatBot
{
    public static class MapChange
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;

        private static readonly Rectangle mapChangeRegion = new Rectangle(733, 277, 1276 - 733, 682 - 277);
        private static ImageDetector imageDetector;

        public static void Initialize(ImageDetector detector)
        {
            imageDetector = detector;
        }
        public static class Logger
        {
            public static void LogMessage(string message, Color color)
            {
                LogConsoleForm.Instance.AppendLog(message, color);
            }

            public static void LogMessage(string message)
            {
                LogMessage(message, Color.White);
            }
        }
        public static void ChangeMap(string direction)
        {
            Random random = new Random();
            Rectangle region = direction.ToLower() switch
            {
                "up" => new Rectangle(741, 26, 346, 8),
                "left" => new Rectangle(157, 336, 195, 211),
                "right" => new Rectangle(1565, 392, 222, 185),
                "down" => new Rectangle(728, 895, 474, 14),
                _ => throw new ArgumentException("Invalid direction. Use 'up', 'down', 'left', or 'right'.")
            };

            int randomX = region.X + random.Next(0, region.Width);
            int randomY = region.Y + random.Next(0, region.Height);

            SimulateClick(randomX, randomY);

            Console.WriteLine("Waiting for black screen to appear...");
            Logger.LogMessage("Changing Map...", Color.Cyan);
            WaitForBlackScreen();
        }

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

        public static void WaitForBlackScreen()
        {
            while (!IsBlackScreenDetected())
            {
                Thread.Sleep(50);
            }
            Console.WriteLine("Black screen detected.");
        }
        public static async void WaitForBlackScreenToDisappear(PlayerCoordinateManager playerCoordinateManager)
        {
            while (IsBlackScreenDetected())
            {
                Thread.Sleep(50);
            }

            Console.WriteLine("Black screen has disappeared.");

            if (imageDetector != null)
            {
                imageDetector.NotifyMapChange();
            }

            var discordWebhookUrl = "https://discord.com/api/webhooks/1324017312814202963/jwSUb_dFuPuUOxa7ZmbrzrphzAe3a6r5-Exp0gJELZCIBBPA-mEbfLwo8sUL_weEmy35";
            var archiDetector = new ArchiDetector(discordWebhookUrl);

            var (isDetected, playerX, playerY) = await archiDetector.ScanForArchisAsync(playerCoordinateManager);

            if (isDetected)
            {
                Console.WriteLine($"Archis detected at Player Coordinates: X = {playerX}, Y = {playerY}");
                Logger.LogMessage($"Archimonstre Detected at X: {playerX}, Y: {playerY}", Color.Yellow);
            }
            else
            {
                Console.WriteLine("No Archis found in the region.");
            }
        }

        private static bool IsBlackScreenDetected()
        {
            try
            {

                using Bitmap screenshot = CaptureMapChangeRegion();
                if (screenshot == null)
                {
                    Console.WriteLine("Failed to capture the map change region.");
                    return false;
                }

                using var mat = OpenCvSharp.Extensions.BitmapConverter.ToMat(screenshot);
                using var grayMat = new Mat();
                Cv2.CvtColor(mat, grayMat, ColorConversionCodes.BGR2GRAY);

                double minVal, maxVal;
                Cv2.MinMaxLoc(grayMat, out minVal, out maxVal);

                return maxVal < 10;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during black screen detection: {ex.Message}");
                return false;
            }
        }

        private static Bitmap CaptureMapChangeRegion()
        {
            try
            {
                Bitmap screenshot = new Bitmap(mapChangeRegion.Width, mapChangeRegion.Height);
                using (Graphics g = Graphics.FromImage(screenshot))
                {
                    g.CopyFromScreen(mapChangeRegion.Location, Point.Empty, mapChangeRegion.Size);
                }
                return screenshot;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error capturing map change region: {ex.Message}");
                return null;
            }
        }
    }
}