using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Tesseract;
using Point = System.Drawing.Point;

namespace HeatBot
{
    public class TreasureHunt
    {
        private static readonly Rectangle StartMapRegion = new Rectangle(30, 231, 95, 21);
        private static readonly Rectangle DirectionRegion = new Rectangle(0, 283, 30, 231);
        private static readonly Rectangle ClueStepsRegion = new Rectangle(31, 282, 119, 245);
        private static readonly string JsonFilePath = "dofuspourlesnoobs_clues.json";
        private static readonly TreasureDPB TreasureDataProvider = new TreasureDPB(JsonFilePath);

        private static Bitmap CaptureRegion(Rectangle region)
        {
            try
            {
                Bitmap screenshot = new Bitmap(region.Width, region.Height);
                using (Graphics g = Graphics.FromImage(screenshot))
                {
                    g.CopyFromScreen(region.Location, Point.Empty, region.Size);
                }
                return screenshot;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to capture region: {ex.Message}");
                return null;
            }
        }

        private static string ExtractTextFromBitmap(Bitmap bitmap)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                    using (var pix = Pix.LoadFromMemory(memoryStream.ToArray()))
                    using (var ocrEngine = new TesseractEngine("./tessdata", "eng", EngineMode.Default))
                    using (var page = ocrEngine.Process(pix))
                    {
                        return page.GetText().Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] OCR failed: {ex.Message}");
                return string.Empty;
            }
        }

        private static string ExtractMapCoordinates(string text)
        {
            var match = Regex.Match(text, @"\[-?\d+,\s*-?\d+\]");
            return match.Success ? match.Value : null;
        }

        private static List<string> ExtractClues(string text)
        {
            var clues = new List<string>();
            foreach (var line in text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!string.IsNullOrWhiteSpace(line))
                    clues.Add(line.Trim());
            }
            return clues;
        }

        private static async Task<bool> TeleportToNearestZaap(string mapCoordinates)
        {
            var playerCoordinateManager = new PlayerCoordinateManager(0, 0);
            var zaapMenu = new ZaapMenuEN(playerCoordinateManager);
            var treasureNearestZaap = new TreasureNearestZaap(zaapMenu);

            try
            {
                treasureNearestZaap.TeleportToNearestZaap(mapCoordinates);
                await Task.Delay(2000);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Teleportation failed: {ex.Message}");
                return false;
            }
        }
        private static void CaptureClueStepsRegion()
        {
            try
            {

                Bitmap clueStepsImage = CaptureRegion(ClueStepsRegion);
                if (clueStepsImage != null)
                {

                    string filePath = "ClueStepsRegion_Screenshot.png";
                    clueStepsImage.Save(filePath);
                    clueStepsImage.Dispose();
                    Console.WriteLine($"[INFO] ClueStepsRegion screenshot saved to {filePath}");
                }
                else
                {
                    Console.WriteLine("[ERROR] Failed to capture ClueStepsRegion.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] An error occurred while capturing ClueStepsRegion: {ex.Message}");
            }
        }
        private static async Task ProcessClueSteps(PlayerCoordinateManager playerCoordinateManager, JsonElement cluesArray)
        {
            HashSet<int> multiLineClueIds = new HashSet<int>
    {
        155, 70, 102, 117, 120, 121, 122
    };

            Rectangle[] ClueStepRegions =
            {
        new Rectangle(32, 278, 129, 16),
        new Rectangle(31, 304, 131, 15),
        new Rectangle(32, 329, 134, 14),
        new Rectangle(32, 353, 134, 16),
        new Rectangle(33, 378, 132, 17),
        new Rectangle(32, 403, 133, 16)
    };

            Rectangle[] DirectionRegions =
            {
        new Rectangle(11, 275, 19, 23),
        new Rectangle(11, 300, 20, 22),
        new Rectangle(11, 325, 20, 23),
        new Rectangle(10, 350, 20, 22),
        new Rectangle(12, 375, 21, 21),
        new Rectangle(11, 399, 21, 22)
    };

            int clueStepCount = Math.Min(ClueStepRegions.Length, DirectionRegions.Length);
            int[] fallbackWidths = { 129, 100, 50 };

            for (int clueStepIndex = 0; clueStepIndex < clueStepCount; clueStepIndex++)
            {
                Console.WriteLine($"[INFO] Processing clue step {clueStepIndex + 1}...");

                Rectangle clueRegion = ClueStepRegions[clueStepIndex];
                string clueStepsText = null;
                int clueId = -1;

                foreach (int fallbackWidth in fallbackWidths)
                {
                    clueRegion.Width = fallbackWidth;
                    Console.WriteLine($"[DEBUG] Trying clue region with width {fallbackWidth} for clue step {clueStepIndex + 1}...");

                    Bitmap clueStepImage = CaptureRegion(clueRegion);
                    if (clueStepImage == null)
                    {
                        Console.WriteLine("[ERROR] Failed to capture clue step region.");
                        continue;
                    }

                    string screenshotPath = $"ClueStep_{clueStepIndex + 1}_Width_{fallbackWidth}.png";
                    clueStepImage.Save(screenshotPath);
                    Console.WriteLine($"[INFO] Saved clue step screenshot: {screenshotPath}");

                    clueStepsText = ExtractTextFromBitmap(clueStepImage);
                    clueStepImage.Dispose();

                    if (!string.IsNullOrEmpty(clueStepsText))
                    {
                        clueStepsText = PreprocessClueText(clueStepsText);
                        Console.WriteLine($"[DEBUG] Preprocessed clue text: {clueStepsText}");

                        clueId = TreasureDataProvider.GetClueId(clueStepsText, cluesArray);

                        if (clueId != 0)
                        {
                            Console.WriteLine($"[INFO] Clue ID '{clueId}' found.");
                            break;
                        }
                        Console.WriteLine($"[WARNING] Clue ID is 0 for clue text '{clueStepsText}'. Trying next fallback width.");
                    }
                    else
                    {
                        Console.WriteLine($"[WARNING] No clue detected with region width {fallbackWidth}.");
                    }
                }

                if (clueId == 0 || string.IsNullOrEmpty(clueStepsText))
                {
                    Console.WriteLine($"[ERROR] Failed to extract valid clue for step {clueStepIndex + 1} after all fallbacks.");
                    continue;
                }

                Console.WriteLine($"[DEBUG] Detected clue text: {clueStepsText}");
                Console.WriteLine($"[INFO] Clue '{clueStepsText}' has clue ID: {clueId}");

                bool isMultiLineClue = multiLineClueIds.Contains(clueId);
                if (isMultiLineClue && clueStepIndex + 1 < clueStepCount)
                {
                    Console.WriteLine($"[INFO] Multi-line clue detected (clue ID: {clueId}). Adjusting next clue region...");
                    Rectangle nextClueRegion = ClueStepRegions[clueStepIndex + 1];
                    ClueStepRegions[clueStepIndex + 1] = new Rectangle(
                        nextClueRegion.X,
                        clueRegion.Y + clueRegion.Height,
                        nextClueRegion.Width,
                        nextClueRegion.Height + clueRegion.Height
                    );
                }

                Bitmap directionImage = CaptureRegion(DirectionRegions[clueStepIndex]);
                if (directionImage == null)
                {
                    Console.WriteLine("[ERROR] Failed to capture direction region.");
                    continue;
                }

                string direction = DetectDirection(DirectionRegions[clueStepIndex]);
                directionImage.Dispose();

                if (string.IsNullOrEmpty(direction))
                {
                    Console.WriteLine($"[ERROR] Unable to detect direction for clue step {clueStepIndex + 1}. Skipping step...");
                    continue;
                }

                Console.WriteLine($"[INFO] Detected direction: {direction}");

                var clueLocation = TreasureDataProvider.FindClosestMap(clueStepsText, playerCoordinateManager.CurrentX, playerCoordinateManager.CurrentY, direction);
                if (clueLocation != null)
                {
                    Console.WriteLine($"[INFO] Clue '{clueStepsText}' found at coordinates: ({clueLocation.Value.x}, {clueLocation.Value.y})");

                    playerCoordinateManager.SetCoordinates(clueLocation.Value.x, clueLocation.Value.y);
                    TreasureTravel.TypeTravelCommand(clueLocation.Value.x, clueLocation.Value.y);
                    StartLungsOfTheCode(playerCoordinateManager);
                }
                else
                {
                    Console.WriteLine($"[WARNING] Clue '{clueStepsText}' not found in the local data for step {clueStepIndex + 1}. Skipping step...");
                    continue;
                }

                await Task.Delay(500);
            }

            Console.WriteLine("[INFO] Finished processing all clue steps.");
            ClickConfirm(playerCoordinateManager, cluesArray);
        }

        private static string PreprocessClueText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            string[] invalidPatterns = { "ttempts remain", "attempts remain", "time left", "time remaining" };
            foreach (var pattern in invalidPatterns)
            {
                text = Regex.Replace(text, Regex.Escape(pattern), "", RegexOptions.IgnoreCase);
            }

            text = Regex.Replace(text, @"^[^\w]+|[^\w]+$", "");

            text = text.Replace("‘", "'").Replace("’", "'").Replace("“", "\"").Replace("”", "\"");

            text = Regex.Replace(text, @"[^\w\s'-]", "");

            text = Regex.Replace(text, @"\s+", " ").Trim();

            return text;
        }

        private static void ClickConfirm(PlayerCoordinateManager playerCoordinateManager, JsonElement cluesArray)
        {
            try
            {
                Rectangle confirmRegion = new Rectangle(0, 159, 296, 572);

                Bitmap confirmImage = CaptureRegion(confirmRegion);
                if (confirmImage == null)
                {
                    Console.WriteLine("[ERROR] Failed to capture confirm region.");
                    return;
                }

                string screenshotPath = "ConfirmRegionScreenshot.png";
                confirmImage.Save(screenshotPath, System.Drawing.Imaging.ImageFormat.Png);
                Console.WriteLine($"[INFO] Confirm region screenshot saved to {screenshotPath}");

                Mat regionMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(confirmImage);
                Cv2.CvtColor(regionMat, regionMat, ColorConversionCodes.BGR2GRAY);

                Mat confirmTemplate = PrepareTemplate((Bitmap)huntimage.confirm);

                regionMat.ConvertTo(regionMat, MatType.CV_8U);
                confirmTemplate.ConvertTo(confirmTemplate, MatType.CV_8U);

                Mat result = new Mat();
                Cv2.MatchTemplate(regionMat, confirmTemplate, result, TemplateMatchModes.CCoeffNormed);

                double minVal, maxVal;
                OpenCvSharp.Point minLoc, maxLoc;
                Cv2.MinMaxLoc(result, out minVal, out maxVal, out minLoc, out maxLoc);

                if (maxVal > 0.8)
                {

                    int centerX = confirmRegion.Left + maxLoc.X + confirmTemplate.Width / 2;
                    int centerY = confirmRegion.Top + maxLoc.Y + confirmTemplate.Height / 2;

                    Console.WriteLine($"[INFO] Confirm button detected at ({centerX}, {centerY}) with confidence {maxVal}");
                    ClickSimulate.SimulateClick(centerX, centerY);
                    Thread.Sleep(5000);

                    _ = ProcessClueSteps(new PlayerCoordinateManager(playerCoordinateManager.CurrentX, playerCoordinateManager.CurrentY), cluesArray);
                }
                else
                {
                    Console.WriteLine("[WARNING] Confirm button not detected with sufficient confidence.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to click confirm: {ex.Message}");
            }
        }

        public static async Task StartTreasureHunt()
        {
            Console.WriteLine("[INFO] Starting Treasure Hunt...");

            string jsonContent = File.ReadAllText(JsonFilePath);
            using JsonDocument document = JsonDocument.Parse(jsonContent);
            JsonElement cluesArray = document.RootElement.GetProperty("clues");

            int x = 0, y = 0;

            Bitmap startMapImage = CaptureRegion(StartMapRegion);
            string startMapText = startMapImage != null ? ExtractTextFromBitmap(startMapImage) : null;
            startMapImage?.Dispose();
            Console.WriteLine($"[INFO] Extracted Start Map: {startMapText}");

            string mapCoordinates = startMapText != null ? ExtractMapCoordinates(startMapText) : null;

            if (mapCoordinates != null)
            {
                Console.WriteLine($"[INFO] Extracted Start Map: {mapCoordinates}");

                var match = Regex.Match(mapCoordinates, @"\[(?<x>-?\d+),\s*(?<y>-?\d+)\]");
                if (match.Success)
                {

                    x = int.Parse(match.Groups["x"].Value);
                    y = int.Parse(match.Groups["y"].Value);

                    Console.WriteLine($"[INFO] Parsed Coordinates: x={x}, y={y}");

                    var playerCoordinateManager = new PlayerCoordinateManager(0, 0);
                    var zaapMenu = new ZaapMenuEN(playerCoordinateManager);
                    var treasureNearestZaap = new TreasureNearestZaap(zaapMenu);

                    try
                    {
                        treasureNearestZaap.TeleportToNearestZaap(mapCoordinates);
                        await Task.Delay(2000);
                        Console.WriteLine("[INFO] Teleported to the nearest Zaap.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] Teleportation to nearest Zaap failed: {ex.Message}");
                        return;
                    }

                    playerCoordinateManager.SetCoordinates(x, y);

                    await ProcessClueSteps(playerCoordinateManager, cluesArray);
                }
                else
                {
                    Console.WriteLine("[ERROR] Unable to parse map coordinates.");
                }
            }
            else
            {
                Console.WriteLine("[WARNING] Could not extract Start Map coordinates.");
            }
        }
        private static Rectangle ExtendDirectionRegion(Rectangle currentRegion)
        {

            const int yIncrement = 23;
            const int heightIncrement = 25;

            return new Rectangle(
                currentRegion.X,
                currentRegion.Y,
                currentRegion.Width,
                currentRegion.Height
            );
        }

        private static string DetectDirection(Rectangle directionRegion)
        {

            Bitmap directionImage = CaptureRegion(directionRegion);
            if (directionImage == null)
            {
                Console.WriteLine("[WARNING] Failed to capture direction image.");
                return null;
            }

            string screenshotPath = $"DirectionRegion_{directionRegion.X}_{directionRegion.Y}.png";
            directionImage.Save(screenshotPath);
            Console.WriteLine($"[DEBUG] Saved direction region screenshot: {screenshotPath}");

            try
            {

                Mat matImage = OpenCvSharp.Extensions.BitmapConverter.ToMat(directionImage);

                Mat grayImage = new Mat();
                Cv2.CvtColor(matImage, grayImage, ColorConversionCodes.BGR2GRAY);

                Mat thresholdImage = new Mat();
                Cv2.Threshold(grayImage, thresholdImage, 100, 255, ThresholdTypes.Binary);

                var templates = new Dictionary<string, Mat>
        {
            { "up", PrepareTemplate((Bitmap)huntimage.up) },
            { "down", PrepareTemplate((Bitmap)huntimage.down) },
            { "left", PrepareTemplate((Bitmap)huntimage.left) },
            { "right", PrepareTemplate((Bitmap)huntimage.right) }
        };

                var detectedDirections = new List<(string direction, double confidence)>();
                foreach (var template in templates)
                {
                    Mat result = new Mat();
                    Cv2.MatchTemplate(thresholdImage, template.Value, result, TemplateMatchModes.CCoeffNormed);

                    double minVal, maxVal;
                    OpenCvSharp.Point minLoc, maxLoc;
                    Cv2.MinMaxLoc(result, out minVal, out maxVal, out minLoc, out maxLoc);

                    if (maxVal > 0.7)
                    {
                        detectedDirections.Add((template.Key, maxVal));
                    }
                }

                if (detectedDirections.Count > 0)
                {
                    var bestDirection = detectedDirections.OrderByDescending(d => d.confidence).First();
                    Console.WriteLine($"[INFO] Detected direction: {bestDirection.direction} with confidence {bestDirection.confidence}");
                    return bestDirection.direction;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to process direction region: {ex.Message}");
            }
            finally
            {
                directionImage.Dispose();
            }

            Console.WriteLine("[WARNING] No direction detected.");
            return null;
        }

        private static Mat PrepareTemplate(Bitmap templateBitmap)
        {
            try
            {

                Mat templateMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(templateBitmap);

                Mat grayTemplate = new Mat();
                Cv2.CvtColor(templateMat, grayTemplate, ColorConversionCodes.BGR2GRAY);

                return grayTemplate;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to prepare template: {ex.Message}");
                throw;
            }
        }

        private static void StartLungsOfTheCode(PlayerCoordinateManager playerCoordinateManager)
        {
            try
            {
                Console.WriteLine("[INFO] Starting LungsOfTheCode.exe...");
                var process = System.Diagnostics.Process.Start("LungsOfTheCode.exe");

                Thread.Sleep(3000);

                int mapsToCross = GetMapsToCross();
                if (mapsToCross == 0)
                {
                    Console.WriteLine("[ERROR] Failed to determine the number of maps to cross.");
                    return;
                }

                Console.WriteLine("[INFO] Restarting LungsOfTheCode.exe...");
                process.Kill();
                System.Diagnostics.Process.Start("LungsOfTheCode.exe");

                TraverseMaps(mapsToCross, playerCoordinateManager);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to execute LungsOfTheCode.exe: {ex.Message}");
            }
        }

        private static int GetMapsToCross()
        {
            Bitmap mapCountImage = CaptureRegion(new Rectangle(706, 480, 504, 85));
            if (mapCountImage == null)
            {
                Console.WriteLine("[ERROR] Failed to capture map count region.");
                return 0;
            }

            string extractedText = ExtractTextFromBitmap(mapCountImage);
            mapCountImage.Dispose();

            if (string.IsNullOrEmpty(extractedText))
            {
                Console.WriteLine("[WARNING] No text extracted for map count.");
                return 0;
            }

            var match = Regex.Match(extractedText, @"crossing\s+(\d+)\s+maps", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                int mapsToCross = int.Parse(match.Groups[1].Value);
                Console.WriteLine($"[INFO] Maps to cross: {mapsToCross}");
                return mapsToCross;
            }

            Console.WriteLine("[WARNING] Unable to extract number of maps to cross.");
            return 0;
        }

        private static void TraverseMaps(int mapsToCross, PlayerCoordinateManager playerCoordinateManager)
        {
            for (int mapsCrossed = 0; mapsCrossed < mapsToCross; mapsCrossed++)
            {
                Console.WriteLine($"[INFO] Crossing map {mapsCrossed + 1} of {mapsToCross}...");
                MapChange.WaitForBlackScreen();
                MapChange.WaitForBlackScreenToDisappear(playerCoordinateManager);
            }

            Console.WriteLine("[INFO] All maps crossed successfully.");
            ClickBalise(playerCoordinateManager);
        }

        private static void ClickBalise(PlayerCoordinateManager playerCoordinateManager)
        {
            Console.WriteLine("[INFO] Clicking on the balise...");
            Thread.Sleep(1500);
            TreasureBalise.ClickInsideBalise();

            Console.WriteLine($"[INFO] Current Coordinates: X = {playerCoordinateManager.CurrentX}, Y = {playerCoordinateManager.CurrentY}");
        }
    }
}