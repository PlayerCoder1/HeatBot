using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using OpenCvSharp;
using CvPoint = OpenCvSharp.Point;
using Point = System.Drawing.Point;
using Serilog;

namespace HeatBot
{
    public class ImageDetector
    {
        private readonly PodsDetector podsDetector;
        private Point? lastClickPosition = null;
        private bool isAfterMapChange = false;
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
        public void CountdownLog(int delayMs, string baseMessage)
        {
            int interval = 1000;
            int remainingTime = delayMs;

            while (remainingTime > 0)
            {
                Logger.LogMessage($"{baseMessage}: {remainingTime} ms remaining.");
                Thread.Sleep(interval);
                remainingTime -= interval;

                if (remainingTime < 0)
                    remainingTime = 0;
            }

            Logger.LogMessage($"{baseMessage}: 0 ms remaining. Gathering complete.");
        }

        public ImageDetector()
        {
            podsDetector = new PodsDetector();
        }
        public void NotifyMapChange()
        {
            Console.WriteLine("[DEBUG] NotifyMapChange called. Resetting per-map click counts.");
            currentMapClickCount = new Dictionary<Point, int>();
            isAfterMapChange = true;
        }
        private string GetMapKey(PlayerCoordinateManager playerCoordinateManager)
        {
            return $"{playerCoordinateManager.CurrentX},{playerCoordinateManager.CurrentY}";
        }
        private readonly Dictionary<string, Dictionary<Point, int>> ignoreList = new();
        private Dictionary<Point, int> currentMapClickCount = new();

        public bool DetectAndClick(List<string> resourceKeys, PlayerCoordinateManager playerCoordinateManager)
        {
            bool foundImage = false;

            string currentMapKey = GetMapKey(playerCoordinateManager);
            if (!ignoreList.ContainsKey(currentMapKey))
            {
                ignoreList[currentMapKey] = new Dictionary<Point, int>();
            }

            try
            {
                Rectangle captureRegion = new Rectangle(425, 50, 1518 - 425, 829 - 50);

                foreach (var resourceKey in resourceKeys)
                {
                    double podsFillPercentage = podsDetector.DetectPodsFillPercentage();
                    if (podsFillPercentage >= 80)
                    {
                        Console.WriteLine("[INFO] Pods are full. Skipping resource detection.");
                        break;
                    }

                    Bitmap resourceImage = GetResourceImage(resourceKey);
                    if (resourceImage == null)
                    {
                        Console.WriteLine($"[WARNING] Resource image {resourceKey} not found.");
                        continue;
                    }

                    using var targetImage = OpenCvSharp.Extensions.BitmapConverter.ToMat(resourceImage);
                    using var screenshot = CaptureScreen(captureRegion);
                    var match = MatchImage(screenshot, targetImage, captureRegion);

                    if (match.HasValue)
                    {
                        Point resourcePosition = new Point(match.Value.X, match.Value.Y);

                        if (ignoreList[currentMapKey].ContainsKey(resourcePosition) &&
                            ignoreList[currentMapKey][resourcePosition] >= 3)
                        {
                            Console.WriteLine($"[INFO] Ignoring resource at {resourcePosition} on map {currentMapKey}.");
                            continue;
                        }

                        SimulateClick(resourcePosition.X, resourcePosition.Y);
                        Console.WriteLine($"[INFO] Clicked on {resourceKey} at position ({resourcePosition.X}, {resourcePosition.Y}).");
                        Logger.LogMessage($"Clicked on {resourceKey} at position ({resourcePosition.X}, {resourcePosition.Y}).", Color.Pink);

                        if (isAfterMapChange)
                        {
                            Console.WriteLine("[DEBUG] Map change detected. Applying default delay: 8000 ms.");
                            Logger.LogMessage("Gathering Resources, Waiting 8 seconds...", Color.Cyan);
                            CountdownLog(8000, "Gathering...");
                            isAfterMapChange = false;
                        }
                        else
                        {

                            if (lastClickPosition.HasValue)
                            {
                                double distance = CalculateDistance(lastClickPosition.Value, match.Value);
                                double delay = CalculateDynamicDelay(distance);
                                Console.WriteLine($"[DEBUG] Calculated delay: {delay:F2} ms based on distance: {distance:F2}.");
                                CountdownLog((int)delay, "Gathering...");
                            }
                            else
                            {
                                Console.WriteLine("[DEBUG] No previous click position found. Applying default delay: 8000 ms.");
                                Logger.LogMessage("Gathering Resources, Waiting 8 seconds...", Color.Cyan);
                                CountdownLog(8000, "Gathering...");
                            }
                        }

                        lastClickPosition = match.Value;
                        foundImage = true;

                        if (!currentMapClickCount.ContainsKey(resourcePosition))
                        {
                            currentMapClickCount[resourcePosition] = 0;
                        }

                        currentMapClickCount[resourcePosition]++;
                        if (currentMapClickCount[resourcePosition] >= 3)
                        {
                            if (!ignoreList[currentMapKey].ContainsKey(resourcePosition))
                            {
                                ignoreList[currentMapKey][resourcePosition] = 0;
                            }

                            ignoreList[currentMapKey][resourcePosition] = 3;
                            Console.WriteLine($"[INFO] Resource at {resourcePosition} added to ignore list for map {currentMapKey}.");
                            Logger.LogMessage($"Resource at {resourcePosition} added to ignore list for map {currentMapKey}.", Color.Yellow);
                        }

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[ERROR] Exception during resource detection.");
            }

            return foundImage;
        }
        private double CalculateDistance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }

        private double CalculateDynamicDelay(double distance)
        {
            const double baseDelay = 4800;
            const double distanceFactor = 0.8;

            if (distance > 400)
            {
                return 5300 + (distance * distanceFactor);
            }

            return baseDelay + (distance * distanceFactor);
        }

        private Bitmap GetResourceImage(string resourceKey)
        {
            return resourceKey switch
            {
                "frene1" => bucheron.frene1,
                "frene2" => bucheron.frene2,
                "frene3" => bucheron.frene3,
                "frene4" => bucheron.frene4,
                "frene5" => bucheron.frene5,
                "frene6" => bucheron.frene6,
                "frene7" => bucheron.frene7,
                "charme1" => bucheron.charme1,
                "charme2" => bucheron.charme2,
                "charme3" => bucheron.charme3,
                "charme4" => bucheron.charme4,
                "chataigne1" => bucheron.chataigner,
                "chataigne2" => bucheron.chataigner2,
                "chataigne3" => bucheron.chataigner3,
                "chataigne4" => bucheron.chataigne4,
                "erable1" => bucheron.erable1,
                "erable2" => bucheron.erable2,
                "erable3" => bucheron.erable3,
                "merisier1" => bucheron.merisier,
                "merisier2" => bucheron.merisier2,
                "merisier3" => bucheron.merisier3,
                "noyer1" => bucheron.noyer1,
                "noyer2" => bucheron.noyer2,
                "orme1" => bucheron.orme1,
                "orme2" => bucheron.orme2,
                "if1" => bucheron.if1,
                "if2" => bucheron.if2,
                "bambou1" => bucheron.bambou1,
                "bambou2" => bucheron.bambou2,
                "bambou3" => bucheron.bambou3,
                "bambou4" => bucheron.bambou4,
                "bambou-sacré1" => bucheron.bambou_sacré1,
                "bambou-sacré2" => bucheron.bambou_sacré2,
                "bambou-sacré3" => bucheron.bambou_sacré3,
                "bombu1" => bucheron.bombu1,
                "bombu2" => bucheron.bombu2,
                "ébène1" => bucheron.ébène1,
                "ébène2" => bucheron.ébène2,
                "kaliptus1" => bucheron.kaliptus1,
                "kaliptus2" => bucheron.kaliptus2,
                "kaliptus3" => bucheron.kaliptus3,
                "noisetier1" => bucheron.noisetier1,
                "noisetier2" => bucheron.noisetier2,
                "noisetier3" => bucheron.noisetier3,
                "oliviolet1" => bucheron.oliviolet1,
                "oliviolet2" => bucheron.oliviolet2,
                "oliviolet3" => bucheron.oliviolet3,
                "pin1" => bucheron.pin1,
                "pin2" => bucheron.pin2,
                "tremble1" => bucheron.tremble1,
                "tremble2" => bucheron.tremble2,
                "tremble3" => bucheron.tremble3,
                "tremble4" => bucheron.tremble4,
                "ortie1" => alchimiste.ortiee1,
                "ortie2" => alchimiste.ortiee2,
                "ortie3" => alchimiste.ortiee3,
                "ortie4" => alchimiste.ortiee4,
                "sauge1" => alchimiste.sauge1,
                "sauge2" => alchimiste.sauge2,
                "sauge3" => alchimiste.sauge3,
                "trefle1" => alchimiste.trefle1,
                "trefle2" => alchimiste.trefle2,
                "menthe1" => alchimiste.menthe1,
                "menthe2" => alchimiste.menthe2,
                "eldeweiss1" => alchimiste.eldeweiss1,
                "eldeweiss2" => alchimiste.eldeweiss2,
                "belladone1" => alchimiste.belladone1,
                "belladone2" => alchimiste.belladone2,
                "ginseng1" => alchimiste.ginseng1,
                "ginseng2" => alchimiste.ginseng2,
                "ginseng3" => alchimiste.ginseng3,
                "ginseng4" => alchimiste.ginseng4,
                "mandragore1" => alchimiste.mandragore1,
                "mandragore2" => alchimiste.mandragore2,
                "orchidée1" => alchimiste.orchidé1,
                "orchidée2" => alchimiste.orchidé2,
                "orchidée3" => alchimiste.orchidé3,
                "orchidée4" => alchimiste.orchidé4,
                "orchidée5" => alchimiste.orchidé5,
                "orchidée6" => alchimiste.orchidé6,
                "pandouille1" => alchimiste.pandouille1,
                "pandouille2" => alchimiste.pandouille2,
                "pandouille3" => alchimiste.pandouille3,
                "pandouille4" => alchimiste.pandouille4,
                "perce-neige1" => alchimiste.perce_neige1,
                "perce-neige2" => alchimiste.perce_neige2,
                "perce-neige3" => alchimiste.perce_neige3,
                "blé1" => paysan.blé1,
                "blé2" => paysan.blé2,
                "blé3" => paysan.blé3,
                "blé4" => paysan.blé4,
                "blé5" => paysan.blé5,
                "blé6" => paysan.blé6,
                "blé7" => paysan.blé7,
                "avoine1" => paysan.avoine1,
                "avoine2" => paysan.avoine2,
                "avoine3" => paysan.avoine3,
                "avoine4" => paysan.avoine4,
                "avoine5" => paysan.avoine5,
                "avoine6" => paysan.avoine6,
                "avoine7" => paysan.avoine7,
                "avoine8" => paysan.avoine8,
                "avoine9" => paysan.avoine9,
                "avoine10" => paysan.avoine10,
                "avoine11" => paysan.avoine11,
                "chanvre1" => paysan.chanvre,
                "chanvre2" => paysan.chanvre2,
                "houblon1" => paysan.houblon1,
                "houblon2" => paysan.houblon2,
                "lin1" => paysan.lin1,
                "lin2" => paysan.lin2,
                "orge1" => paysan.orge1,
                "orge2" => paysan.orge2,
                "orge3" => paysan.orge3,
                "orge4" => paysan.orge4,
                "orge5" => paysan.orge5,
                "orge6" => paysan.orge6,
                "orge7" => paysan.orge7,
                "orge8" => paysan.orge8,
                "orge9" => paysan.orge9,
                "orge10" => paysan.orge10,
                "seigle1" => paysan.seigle1,
                "seigle2" => paysan.seigle2,
                "seigle3" => paysan.seigle3,
                "frostiz1" => paysan.frostiz1,
                "frostiz2" => paysan.frostiz2,
                "frostiz3" => paysan.frostiz3,
                "mais1" => paysan.mais1,
                "mais2" => paysan.mais2,
                "mais3" => paysan.mais3,
                "malt1" => paysan.malt1,
                "malt2" => paysan.malt2,
                "malt3" => paysan.malt3,
                "malt4" => paysan.malt4,
                "millet1" => paysan.millet1,
                "millet2" => paysan.millet2,
                "millet3" => paysan.millet3,
                "millet4" => paysan.millet4,
                "fer1" => mineur.fer1,
                "fer2" => mineur.fer2,
                "fer3" => mineur.fer3,
                "fer4" => mineur.fer4,
                "fer5" => mineur.fer5,
                "fer6" => mineur.fer6,
                "etain1" => mineur.etain,
                "bronze1" => mineur.bronze,
                "bronze2" => mineur.bronze2,
                "kobalte" => mineur.kobalte,
                _ => null
            };
        }

        private Mat CaptureScreen(Rectangle captureRegion)
        {
            using Bitmap screenshot = new Bitmap(captureRegion.Width, captureRegion.Height);

            using (Graphics g = Graphics.FromImage(screenshot))
            {
                g.CopyFromScreen(captureRegion.Location, Point.Empty, captureRegion.Size);
            }

            return OpenCvSharp.Extensions.BitmapConverter.ToMat(screenshot);
        }

        private Point? MatchImage(Mat source, Mat template, Rectangle captureRegion, double threshold = 0.9)
        {
            using var result = new Mat();
            Cv2.MatchTemplate(source, template, result, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out OpenCvSharp.Point maxLoc);

            if (maxVal >= threshold)
            {
                return new Point(maxLoc.X + template.Width / 2 + captureRegion.X,
                                 maxLoc.Y + template.Height / 2 + captureRegion.Y);
            }

            return null;
        }

        private void SimulateClick(int x, int y)
        {
            if (!ClickSimulate.SetCursorPos(x, y))
            {
                Console.WriteLine("Failed to move the cursor.");
                return;
            }

            Thread.Sleep(100);
            ClickSimulate.TriggerMouseClick(x, y);
        }
    }
}