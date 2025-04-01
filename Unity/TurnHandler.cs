using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using OpenCvSharp;
using Point = System.Drawing.Point;

namespace HeatBot
{
    public class TurnHandler
    {
        private static readonly Rectangle DetectionRegion = new Rectangle(361, 40, 1538 - 361, 884 - 40);
        private static readonly Rectangle ClickRegion = new Rectangle(716, 953, 749 - 716, 984 - 953);
        private static readonly Rectangle PassTurnRegion = new Rectangle(1305, 919, 60, 40);

        private readonly Mat darkImage;
        private readonly Mat lightImage;
        private readonly Mat crackdarkImage;
        private readonly Mat cracklightImage;
        private readonly List<Mat> coinImages;

        public TurnHandler()
        {
            darkImage = LoadResourceImage(fight.dark);
            lightImage = LoadResourceImage(fight.light);
            crackdarkImage = LoadResourceImage(fight.crackdark);
            cracklightImage = LoadResourceImage(fight.cracklight);
            coinImages = new List<Mat>
            {
                LoadResourceImage(fight.coin1),
                LoadResourceImage(fight.coin2),
                LoadResourceImage(fight.coin3)
            };
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
        public void MoveToNearestGreenPixel()
        {
            try
            {
                using Bitmap screenshot = CaptureRegion(DetectionRegion);
                if (screenshot == null)
                {
                    Console.WriteLine("[ERROR] Failed to capture the region.");
                    return;
                }

                using var screenshotMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(screenshot);

                var lightMatch = MatchTemplate(screenshotMat, lightImage);
                var darkMatch = MatchTemplate(screenshotMat, darkImage);
                var darkCrackMatch = MatchTemplate(screenshotMat, crackdarkImage);
                var lightCrackMatch = MatchTemplate(screenshotMat, cracklightImage);

                List<Point> greenAreas = new List<Point>();

                if (lightMatch.HasValue) greenAreas.Add(lightMatch.Value);
                if (darkMatch.HasValue) greenAreas.Add(darkMatch.Value);
                if (darkCrackMatch.HasValue) greenAreas.Add(darkCrackMatch.Value);
                if (lightCrackMatch.HasValue) greenAreas.Add(lightCrackMatch.Value);

                if (greenAreas.Count == 0)
                {
                    Console.WriteLine("[INFO] No green areas detected.");
                    ProceedToAttackSequence();
                    return;
                }

                List<Mat> monsterCoins = new List<Mat>
        {
            OpenCvSharp.Extensions.BitmapConverter.ToMat(monster.coin1),
            OpenCvSharp.Extensions.BitmapConverter.ToMat(monster.coin2),
            OpenCvSharp.Extensions.BitmapConverter.ToMat(monster.coin3),
            OpenCvSharp.Extensions.BitmapConverter.ToMat(monster.coin4),
            OpenCvSharp.Extensions.BitmapConverter.ToMat(monster.coin5),
            OpenCvSharp.Extensions.BitmapConverter.ToMat(monster.coin6),
            OpenCvSharp.Extensions.BitmapConverter.ToMat(monster.coin7),
            OpenCvSharp.Extensions.BitmapConverter.ToMat(monster.coin8),
            OpenCvSharp.Extensions.BitmapConverter.ToMat(monster.coin9),
            OpenCvSharp.Extensions.BitmapConverter.ToMat(monster.coin10),
            OpenCvSharp.Extensions.BitmapConverter.ToMat(monster.coin11),
            OpenCvSharp.Extensions.BitmapConverter.ToMat(monster.coin12)
        };

                List<Point> monsterCoinLocations = new List<Point>();
                foreach (var monsterCoin in monsterCoins)
                {
                    var coinMatches = MatchTemplate(screenshotMat, monsterCoin);
                    if (coinMatches.HasValue)
                        monsterCoinLocations.Add(coinMatches.Value);
                }

                if (monsterCoinLocations.Count == 0)
                {
                    Console.WriteLine("[INFO] No monster coins detected.");
                    ProceedToAttackSequence();
                    return;
                }

                Point? bestGreenArea = null;
                double closestDistance = double.MaxValue;

                foreach (var greenArea in greenAreas)
                {
                    foreach (var coinLocation in monsterCoinLocations)
                    {
                        double distance = Math.Sqrt(
                            Math.Pow(greenArea.X - coinLocation.X, 2) +
                            Math.Pow(greenArea.Y - coinLocation.Y, 2)
                        );

                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            bestGreenArea = greenArea;
                        }
                    }
                }

                if (bestGreenArea == null)
                {
                    Console.WriteLine("[INFO] No suitable green area found.");
                    ProceedToAttackSequence();
                    return;
                }

                SimulateClick(bestGreenArea.Value.X + DetectionRegion.X, bestGreenArea.Value.Y + DetectionRegion.Y);
                Thread.Sleep(500);

                ProceedToAttackSequence();
            }
            catch (AccessViolationException ex)
            {
                Console.WriteLine("[FATAL ERROR] Access violation in OpenCvSharp. Ensure Mat objects are valid: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR] Exception in MoveToNearestGreenPixel: " + ex.Message);
            }
        }

        private void ProceedToAttackSequence()
        {
            for (int i = 0; i < 2; i++)
            {
                Console.WriteLine($"[INFO] Starting attack sequence {i + 1}");
                Logger.LogMessage($"Attack Casted: {i + 1}", Color.Green);
                ClickOnAttackRegion();
                Thread.Sleep(500);
                ClickOnCoins();
                Thread.Sleep(500);
            }

            PassTurn();
        }
        private void ClickOnAttackRegion()
        {
            try
            {

                string brainOfTheCodePath = "BrainOfTheCode.exe";

                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = brainOfTheCodePath,
                        UseShellExecute = true,
                        CreateNoWindow = false
                    }
                };

                process.Start();

                process.WaitForExit();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to execute {ex.Message}");
            }
        }

        private void ClickOnCoins()
        {
            try
            {
                using Bitmap fullScreenshot = CaptureFullScreen();
                using var fullScreenMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(fullScreenshot);

                foreach (var coinImage in coinImages)
                {
                    var match = MatchTemplate(fullScreenMat, coinImage);
                    if (match.HasValue)
                    {
                        Console.WriteLine($"[INFO] Clicking on coin at absolute position {match.Value}.");
                        SimulateClick(match.Value.X, match.Value.Y);
                        Thread.Sleep(500);

                        MoveMouseToRandomPosition();
                        return;
                    }
                }

                Console.WriteLine("[INFO] No coins detected on the screen. Clicking randomly.");
                MoveMouseToRandomPosition2();
                Thread.Sleep(250);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to click on coins or randomly: {ex.Message}");
            }
        }

        private void MoveMouseToRandomPosition()
        {
            try
            {

                int topLeftX = 447;
                int topLeftY = 766;
                int bottomRightX = 596;
                int bottomRightY = 877;

                Random random = new Random();
                int randomX = random.Next(topLeftX, bottomRightX + 1);
                int randomY = random.Next(topLeftY, bottomRightY + 1);

                Console.WriteLine($"[INFO] Moving mouse to random position ({randomX}, {randomY}) within the region.");

                ClickSimulate.SetCursorPos(randomX, randomY);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to move mouse to a random position: {ex.Message}");
            }
        }
        private void MoveMouseToRandomPosition2()
        {
            try
            {

                int topLeftX = 447;
                int topLeftY = 766;
                int bottomRightX = 596;
                int bottomRightY = 877;

                Random random = new Random();
                int randomX = random.Next(topLeftX, bottomRightX + 1);
                int randomY = random.Next(topLeftY, bottomRightY + 1);

                Console.WriteLine($"[INFO] Moving mouse to random position ({randomX}, {randomY}) within the region.");

                ClickSimulate.SimulateClick(randomX, randomY);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to move mouse to a random position: {ex.Message}");
            }
        }

        private void PassTurn()
        {
            try
            {

                string muscleOfTheCode = "MuscleOfTheCode.exe";

                Console.WriteLine($"[INFO] Attempting to open {muscleOfTheCode}.");

                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = muscleOfTheCode,
                        UseShellExecute = true,
                        CreateNoWindow = false
                    }
                };

                process.Start();

                process.WaitForExit();

                Console.WriteLine($"[INFO] {muscleOfTheCode} executed successfully.");
            }
            catch (Exception ex)
            {

            }
        }

        private Bitmap CaptureFullScreen()
        {
            Bitmap fullScreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            using (Graphics g = Graphics.FromImage(fullScreen))
            {
                g.CopyFromScreen(Point.Empty, Point.Empty, fullScreen.Size);
            }
            return fullScreen;
        }

        private Mat LoadResourceImage(Bitmap resourceImage)
        {
            if (resourceImage == null)
                throw new InvalidOperationException("[ERROR] Resource image is null.");

            try
            {
                return OpenCvSharp.Extensions.BitmapConverter.ToMat(resourceImage);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"[ERROR] Failed to convert resource image: {ex.Message}");
            }
        }

        private Bitmap CaptureRegion(Rectangle region)
        {
            Bitmap screenshot = new Bitmap(region.Width, region.Height);
            using (Graphics g = Graphics.FromImage(screenshot))
            {
                g.CopyFromScreen(region.Location, Point.Empty, region.Size);
            }
            return screenshot;
        }

        private Point? MatchTemplate(Mat source, Mat template, double threshold = 0.9)
        {
            if (source == null || source.IsDisposed || source.Empty())
            {
                Console.WriteLine("[ERROR] Source image is invalid or disposed.");
                return null;
            }

            if (template == null || template.IsDisposed || template.Empty())
            {
                Console.WriteLine("[ERROR] Template image is invalid or disposed.");
                return null;
            }

            if (template.Width > source.Width || template.Height > source.Height)
            {
                Console.WriteLine("[ERROR] Template size exceeds source image dimensions.");
                return null;
            }

            using var result = new Mat();
            try
            {
                Cv2.MatchTemplate(source, template, result, TemplateMatchModes.CCoeffNormed);
                Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out OpenCvSharp.Point maxLoc, null);

                if (maxVal >= threshold)
                {
                    return new Point(maxLoc.X + template.Width / 2, maxLoc.Y + template.Height / 2);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] MatchTemplate failed: {ex.Message}");
            }

            return null;
        }

        private void SimulateClick(int x, int y)
        {
            ClickSimulate.SetCursorPos(x, y);
            ClickSimulate.TriggerMouseClick(x, y);
        }
    }
}