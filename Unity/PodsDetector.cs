using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using OpenCvSharp;
using Serilog;
using Point = System.Drawing.Point;

namespace HeatBot
{
    public class PodsDetector
    {
        private Rectangle podsRegion;

        public Rectangle PodsRegion
        {
            get => podsRegion;
            set
            {
                podsRegion = value;
                SaveConfig(podsRegion);
            }
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
        public PodsDetector()
        {
            LoadConfig();
            CapturePodsRegion();
        }

        public double DetectPodsFillPercentage()
        {
            try
            {
                using var screenshot = CapturePodsRegion();
                if (screenshot == null)
                {
                    Console.WriteLine("Failed to capture the pods region.");
                    return -1;
                }

                using var mat = OpenCvSharp.Extensions.BitmapConverter.ToMat(screenshot);
                using var grayMat = new Mat();
                Cv2.CvtColor(mat, grayMat, ColorConversionCodes.BGR2GRAY);

                using var binaryMat = new Mat();
                Cv2.Threshold(grayMat, binaryMat, 200, 255, ThresholdTypes.Binary);

                int totalPixels = binaryMat.Width;
                int filledPixels = Cv2.CountNonZero(binaryMat.Row(binaryMat.Height / 2));

                double fillPercentage = (double)filledPixels / totalPixels * 100;

                return fillPercentage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error detecting pods fill: {ex.Message}");
                return -1;
            }
        }

        private Bitmap CapturePodsRegion()
        {
            try
            {
                Bitmap screenshot = new Bitmap(podsRegion.Width, podsRegion.Height);
                using (Graphics g = Graphics.FromImage(screenshot))
                {
                    g.CopyFromScreen(podsRegion.Location, Point.Empty, podsRegion.Size);
                }

                string filePath = "pods_region_screenshot.png";
                screenshot.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                Console.WriteLine($"Screenshot saved to {filePath}");

                return screenshot;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error capturing pods region: {ex.Message}");
                return null;
            }
        }

        private void LoadConfig()
        {
            try
            {
                if (File.Exists("config.json"))
                {
                    var configJson = File.ReadAllText("config.json");
                    var config = JsonSerializer.Deserialize<Config>(configJson);

                    if (config?.PodsRegion != null)
                    {
                        podsRegion = new Rectangle(config.PodsRegion.X, config.PodsRegion.Y, config.PodsRegion.Width, config.PodsRegion.Height);
                    }
                    else
                    {
                        Console.WriteLine("Invalid or incomplete PodsRegion in config.json. Using default region.");
                        podsRegion = new Rectangle(680, 1014, 548, 12);
                        SaveConfig();
                    }
                }
                else
                {

                    podsRegion = new Rectangle(680, 1014, 548, 12);
                    SaveConfig();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading config.json: {ex.Message}");
                podsRegion = new Rectangle(680, 1014, 548, 12);
            }
        }

        public void SaveConfig(Rectangle? newRegion = null)
        {
            try
            {
                var config = new Config
                {
                    PodsRegion = new Region
                    {
                        X = newRegion?.X ?? podsRegion.X,
                        Y = newRegion?.Y ?? podsRegion.Y,
                        Width = newRegion?.Width ?? podsRegion.Width,
                        Height = newRegion?.Height ?? podsRegion.Height
                    }
                };
                var configJson = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText("config.json", configJson);
                Console.WriteLine("Configuration updated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving to config.json: {ex.Message}");
            }
        }

        private class Config
        {
            public Region PodsRegion { get; set; }
        }

        private class Region
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
        }
    }
}