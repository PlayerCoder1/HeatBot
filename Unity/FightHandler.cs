using System;
using System.Drawing;
using System.Resources;
using OpenCvSharp;
using System.Threading;
using Point = System.Drawing.Point;
using Serilog;

namespace HeatBot
{
    public class FightHandler
    {
        private static readonly Rectangle FightDetectionRegion = new Rectangle(0, 112, 315, 625);
        private static readonly Rectangle ReadyClickRegion = new Rectangle(1305, 919, 60, 40);
        private static readonly Rectangle TurnDetectionRegion = new Rectangle(333, 637, 1732 - 333, 1037 - 637);
        private static readonly Rectangle EndFightRegion = new Rectangle(361, 26, 1555 - 361, 887 - 26);
        private static readonly Rectangle EndFightClickRegion = new Rectangle(1461, 176, 1485 - 1461, 199 - 176);
        private readonly Mat combatImage;
        private readonly Mat myTurnImage;
        private readonly Mat endFightImage;
        private readonly List<CheckBox> combatImageCheckboxes;
        private readonly ImageDetector imageDetector;
        private bool isFighting = false;
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

        public FightHandler(List<CheckBox> combatImageCheckboxes, ImageDetector imageDetector)
        {
            combatImage = LoadCombatImage();
            myTurnImage = LoadMyTurnImage();
            endFightImage = LoadEndFightImage();
            this.combatImageCheckboxes = combatImageCheckboxes ?? throw new ArgumentNullException(nameof(combatImageCheckboxes));
            this.imageDetector = imageDetector ?? throw new ArgumentNullException(nameof(imageDetector));
        }

        public bool HandleFight()
        {
            try
            {
                if (isFighting)
                {
                    if (IsMyTurn())
                    {
                        Log.Information("[INFO] It's my turn to play!");
                        TurnHandler turnHandler = new TurnHandler();
                        turnHandler.MoveToNearestGreenPixel();
                    }

                    if (IsFightEnded())
                    {
                        Log.Information("[INFO] Fight has ended. Clicking to exit fight screen.");
                        Thread.Sleep(500);
                        PerformEndFightClick();
                        ResetFightState();
                    }

                    return true;
                }

                if (IsInFight())
                {
                    Log.Information("[INFO] Player is in a fight!");

                    Form1 form = Application.OpenForms.OfType<Form1>().FirstOrDefault();
                    if (form != null)
                    {
                        form.isInFight = true;
                    }

                    isFighting = true;
                    Thread.Sleep(1000);
                    PerformReadyUpClicks();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[ERROR] Exception in HandleFight.");
            }

            return false;
        }
        public bool IsMyTurn()
        {
            try
            {
                using var screenshot = CaptureTurnRegion();

                if (screenshot == null)
                {
                    Console.WriteLine("Failed to capture the turn detection region.");
                    return false;
                }

                using var screenshotMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(screenshot);
                return MatchTurnImage(screenshotMat);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error detecting turn: {ex.Message}");
                return false;
            }
        }

        public bool IsFightEnded()
        {
            try
            {
                using var screenshot = CaptureEndFightRegion();

                if (screenshot == null)
                {
                    Console.WriteLine("Failed to capture the fight end detection region.");
                    return false;
                }

                using var screenshotMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(screenshot);
                return MatchEndFightImage(screenshotMat);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error detecting fight end: {ex.Message}");
                return false;
            }
        }

        public bool IsInFight()
        {
            try
            {
                using var screenshot = CaptureFightRegion();

                if (screenshot == null)
                {
                    Console.WriteLine("Failed to capture the fight region.");
                    return false;
                }

                using var screenshotMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(screenshot);
                return MatchCombatImage(screenshotMat);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error detecting fight: {ex.Message}");
                return false;
            }
        }

        private bool MatchCombatImage(Mat screenshotMat)
        {
            using var result = new Mat();
            Cv2.MatchTemplate(screenshotMat, combatImage, result, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out _);

            if (maxVal >= 0.85)
            {
                Console.WriteLine("Combat detected in the specified region.");
                Logger.LogMessage("Fight Initiate, In Combat!", Color.Cyan);
                return true;
            }

            return false;
        }

        private bool MatchTurnImage(Mat screenshotMat)
        {
            using var result = new Mat();
            Cv2.MatchTemplate(screenshotMat, myTurnImage, result, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out _);

            if (maxVal >= 0.85)
            {
                Console.WriteLine("[INFO] It's my turn!");
                Logger.LogMessage("It's my turn!", Color.Cyan);
                return true;
            }

            return false;
        }

        private bool MatchEndFightImage(Mat screenshotMat)
        {
            using var result = new Mat();
            Cv2.MatchTemplate(screenshotMat, endFightImage, result, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out _);

            if (maxVal >= 0.85)
            {
                Console.WriteLine("[INFO] Fight ended detected!");
                Logger.LogMessage("Fight Ended!", Color.Cyan);
                return true;
            }

            return false;
        }

        private void PerformReadyUpClicks()
        {
            try
            {
                string muscleOfTheCode = "MuscleOfTheCode.exe";
                Random random = new Random();

                Console.WriteLine($"[INFO] Attempting to open {muscleOfTheCode} (Instance 1).");
                var process1 = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = muscleOfTheCode,
                        UseShellExecute = true,
                        CreateNoWindow = false
                    }
                };
                Logger.LogMessage("Starting The Fight...", Color.Cyan);
                process1.Start();
                process1.WaitForExit();
                Console.WriteLine($"[INFO] {muscleOfTheCode} (Instance 1) executed successfully.");

                int delay = random.Next(1000, 2501);
                Console.WriteLine($"[INFO] Waiting for {delay} ms before starting the second instance.");
                Thread.Sleep(delay);

                Console.WriteLine($"[INFO] Attempting to open {muscleOfTheCode} (Instance 2).");
                var process2 = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = muscleOfTheCode,
                        UseShellExecute = true,
                        CreateNoWindow = false
                    }
                };
                Logger.LogMessage("Fight Has Started...", Color.Cyan);
                process2.Start();
                process2.WaitForExit();
                Console.WriteLine($"[INFO] {muscleOfTheCode} (Instance 2) executed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] An error occurred: {ex.Message}");
            }
        }

        private void PerformEndFightClick()
        {
            try
            {
                string NervesOfTheCode = "NervesOfTheCode.exe";

                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = NervesOfTheCode,
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

        private Mat LoadCombatImage()
        {
            try
            {
                Bitmap combatBitmap = fight.detect1;
                if (combatBitmap == null)
                {
                    throw new InvalidOperationException("Combat image not found in resources.");
                }

                return OpenCvSharp.Extensions.BitmapConverter.ToMat(combatBitmap);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading combat image: {ex.Message}");
                throw;
            }
        }

        private Mat LoadMyTurnImage()
        {
            try
            {
                Bitmap myTurnBitmap = fight.montour;
                if (myTurnBitmap == null)
                {
                    throw new InvalidOperationException("My turn image not found in resources.");
                }

                return OpenCvSharp.Extensions.BitmapConverter.ToMat(myTurnBitmap);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading my turn image: {ex.Message}");
                throw;
            }
        }

        private Mat LoadEndFightImage()
        {
            try
            {
                Bitmap endFightBitmap = fight.fincombat;
                if (endFightBitmap == null)
                {
                    throw new InvalidOperationException("End fight image not found in resources.");
                }

                return OpenCvSharp.Extensions.BitmapConverter.ToMat(endFightBitmap);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading end fight image: {ex.Message}");
                throw;
            }
        }

        private Bitmap CaptureFightRegion()
        {
            try
            {
                Bitmap screenshot = new Bitmap(FightDetectionRegion.Width, FightDetectionRegion.Height);
                using (Graphics g = Graphics.FromImage(screenshot))
                {
                    g.CopyFromScreen(FightDetectionRegion.Location, Point.Empty, FightDetectionRegion.Size);
                }

                return screenshot;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error capturing fight region: {ex.Message}");
                return null;
            }
        }

        private Bitmap CaptureTurnRegion()
        {
            try
            {
                Bitmap screenshot = new Bitmap(TurnDetectionRegion.Width, TurnDetectionRegion.Height);
                using (Graphics g = Graphics.FromImage(screenshot))
                {
                    g.CopyFromScreen(TurnDetectionRegion.Location, Point.Empty, TurnDetectionRegion.Size);
                }

                return screenshot;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error capturing turn detection region: {ex.Message}");
                return null;
            }
        }

        private Bitmap CaptureEndFightRegion()
        {
            try
            {
                Bitmap screenshot = new Bitmap(EndFightRegion.Width, EndFightRegion.Height);
                using (Graphics g = Graphics.FromImage(screenshot))
                {
                    g.CopyFromScreen(EndFightRegion.Location, Point.Empty, EndFightRegion.Size);
                }

                return screenshot;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error capturing end fight detection region: {ex.Message}");
                return null;
            }
        }

        public void ResetFightState()
        {
            isFighting = false;

            Form1 form = Application.OpenForms.OfType<Form1>().FirstOrDefault();
            if (form != null)
            {
                form.isInFight = false;
            }

            Log.Information("[INFO] Fighting state reset.");
            Thread.Sleep(500);
        }
    }
}