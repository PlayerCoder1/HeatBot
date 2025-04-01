using System;
using System.Drawing;
using System.IO;
using OpenCvSharp;
using Point = System.Drawing.Point;

namespace HeatBot
{
    public static class TreasureBalise
    {
        private static Bitmap LoadBaliseImageFromResources()
        {
            try
            {
                Console.WriteLine("[INFO] Loading balise image from resources...");

                return (Bitmap)huntimage.balise;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to load balise image: {ex.Message}");
                return null;
            }
        }

        public static void ClickInsideBalise()
        {
            try
            {
                Bitmap baliseImage = LoadBaliseImageFromResources();
                if (baliseImage == null)
                {
                    Console.WriteLine("[ERROR] Balise image is not loaded. Aborting click operation.");
                    return;
                }

                Console.WriteLine("[INFO] Capturing current screen to locate balise...");
                using Bitmap screen = CaptureFullScreen();
                if (screen == null)
                {
                    Console.WriteLine("[ERROR] Failed to capture the screen. Aborting.");
                    return;
                }

                Console.WriteLine("[INFO] Matching balise image on screen...");
                Point? matchPoint = FindImageOnScreen(screen, baliseImage);
                if (matchPoint.HasValue)
                {
                    Console.WriteLine($"[INFO] Balise found at location: ({matchPoint.Value.X}, {matchPoint.Value.Y}). Clicking...");

                    ClickSimulate.SimulateClick(matchPoint.Value.X, matchPoint.Value.Y);
                }
                else
                {
                    Console.WriteLine("[WARNING] Balise image not found on the screen.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error during balise click operation: {ex.Message}");
            }
        }

        private static Bitmap CaptureFullScreen()
        {
            try
            {
                Rectangle screenBounds = Screen.PrimaryScreen.Bounds;
                Bitmap screenshot = new Bitmap(screenBounds.Width, screenBounds.Height);
                using (Graphics g = Graphics.FromImage(screenshot))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, screenBounds.Size);
                }
                return screenshot;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to capture full screen: {ex.Message}");
                return null;
            }
        }

        private static Point? FindImageOnScreen(Bitmap screen, Bitmap template)
        {
            try
            {
                using Mat screenMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(screen);
                using Mat templateMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(template);

                using Mat result = new Mat();
                Cv2.MatchTemplate(screenMat, templateMat, result, TemplateMatchModes.CCoeffNormed);

                Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out OpenCvSharp.Point maxLoc);

                Console.WriteLine($"[INFO] Template match confidence: {maxVal}");
                if (maxVal >= 0.8)
                {
                    return new Point(maxLoc.X + template.Width / 2, maxLoc.Y + template.Height / 2);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to locate template on screen: {ex.Message}");
                return null;
            }
        }
    }
}