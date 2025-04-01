using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace HeatBot
{
    public class ZaapMenuEN
    {
        private readonly PlayerCoordinateManager _playerCoordinateManager;
        public List<ZaapLocation> ZaapLocations { get; }

        public ZaapMenuEN(PlayerCoordinateManager playerCoordinateManager)
        {
            _playerCoordinateManager = playerCoordinateManager;
            ZaapLocations = new List<ZaapLocation>
            {
                new ZaapLocation("Edge of the evil forest", -1, 13),
                new ZaapLocation("amakna village", -2, 0),
                new ZaapLocation("Madrestam harbour", 7, -4),
                new ZaapLocation("Crackler mountain", -5, -8),
                new ZaapLocation("scaraleaf plain", -1, 24),
                new ZaapLocation("amakna castle", 3, -5),
                new ZaapLocation("gobball corner", 5, 7),
                new ZaapLocation("Tainela", 1, -32),
                new ZaapLocation("astrub city", 5, -18),
                new ZaapLocation("Sufokia", 13, 26),
                new ZaapLocation("sufokian shoreline", 10, 22),
                new ZaapLocation("immaculate heart", -31, -56),
                new ZaapLocation("the breastplate", -26, 37),
                new ZaapLocation("trool fair", -11, -36),
                new ZaapLocation("caravan alley", -25, 12),
                new ZaapLocation("breeder village", -16, 1),
                new ZaapLocation("rocky plains", -17, -47),
                new ZaapLocation("cania massif", -13, -28),
                new ZaapLocation("cania lake", -3, -42),
                new ZaapLocation("rocky roads", -20, -20),
                new ZaapLocation("cania fields", -27, -36),
                new ZaapLocation("dopple village", -34, -8),
                new ZaapLocation("lousy pig plain", -5, -23),
                new ZaapLocation("kanig village", 0, -56),
                new ZaapLocation("frigost village", -78, -41),
                new ZaapLocation ("coastal village", -46, 18),
                new ZaapLocation ("pandala village", 20, -29)
            };
        }

        public bool TeleportToLocation(string locationName)
        {
            var zaap = ZaapLocations.Find(z => z.Name.Equals(locationName, StringComparison.OrdinalIgnoreCase));
            if (zaap == null)
            {
                Console.WriteLine($"[ERROR] Zaap location '{locationName}' not found.");
                return false;
            }

            Console.WriteLine($"[INFO] Teleporting to {zaap.Name} at coordinates ({zaap.X}, {zaap.Y}).");
            Thread.Sleep(2000);
            ExecuteBonesOfTheCode();
            Thread.Sleep(2000);
            OpenZaapMenu();
            SelectZaap(zaap.Name);
            ConfirmTeleport(zaap);
            return true;
        }
        private void ExecuteBonesOfTheCode()
        {
            string executablePath = "BonesOfTheCode.exe";

            Console.WriteLine($"[INFO] Executing {executablePath} before opening the zaap menu.");
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = executablePath,
                        UseShellExecute = true,
                        CreateNoWindow = false
                    }
                };

                process.Start();
                process.WaitForExit();
                Console.WriteLine($"[INFO] {executablePath} executed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to execute {executablePath}: {ex.Message}");
            }
        }
        private void OpenZaapMenu()
        {
            Console.WriteLine("[INFO] Opening zaap menu...");
            Random random = new Random();
            int x = random.Next(514, 604);
            int y = random.Next(381, 395);
            ClickSimulate.SimulateClick(x, y);
            Thread.Sleep(2000);
        }

        private void SelectZaap(string zaapName)
        {
            Console.WriteLine($"[INFO] Selecting zaap location: {zaapName}.");
            SimulateTyping(zaapName);
            Thread.Sleep(1000);
        }

        private void ConfirmTeleport(ZaapLocation zaap)
        {
            Console.WriteLine("[INFO] Confirming teleport.");
            Random random = new Random();
            int x = random.Next(1276, 1367);
            int y = random.Next(791, 812);
            ClickSimulate.SimulateClick(x, y);
            Thread.Sleep(2000);

            _playerCoordinateManager.SetCoordinates(zaap.X, zaap.Y);

            Console.WriteLine($"[INFO] Teleport confirmed. Player coordinates updated to ({zaap.X}, {zaap.Y}).");
        }

        private void SimulateTyping(string text)
        {
            Console.WriteLine($"[INFO] Typing: {text}");
            foreach (char c in text)
            {
                Console.Write(c);
                SendKeys.SendWait(c.ToString());
                Thread.Sleep(100);
            }
            Console.WriteLine();
        }

        public class ZaapLocation
        {
            public string Name { get; set; }
            public int X { get; set; }
            public int Y { get; set; }

            public ZaapLocation(string name, int x, int y)
            {
                Name = name;
                X = x;
                Y = y;
            }
        }
    }
}