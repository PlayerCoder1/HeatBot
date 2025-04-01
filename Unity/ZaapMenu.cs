using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace HeatBot
{
    public class ZaapMenu
    {
        private readonly PlayerCoordinateManager _playerCoordinateManager;
        public List<ZaapLocation> ZaapLocations { get; }

        public ZaapMenu(PlayerCoordinateManager playerCoordinateManager)
        {
            _playerCoordinateManager = playerCoordinateManager;
            ZaapLocations = new List<ZaapLocation>
            {
                new ZaapLocation("Bord de la forêt maléfique", -1, 13),
                new ZaapLocation("Village d'Amakna", -2, 0),
                new ZaapLocation("Port de Madrestam", 7, -4),
                new ZaapLocation("Montagne des Craqueleurs", -5, -8),
                new ZaapLocation("Plaine des Scarafeuilles", -1, 24),
                new ZaapLocation("Château d'Amakna", 3, -5),
                new ZaapLocation("Coin des Bouftous", 5, 7),
                new ZaapLocation("Tainéla", 1, -32),
                new ZaapLocation("Cité d'Astrub", 5, -18),
                new ZaapLocation("Sufokia", 13, 26),
                new ZaapLocation("Rivage sufokien", 10, 22),
                new ZaapLocation("Immaculé", -31, -56),
                new ZaapLocation("La Cuirasse", -26, 37),
                new ZaapLocation("Foire du Trool", -11, -36),
                new ZaapLocation("Route des Roulottes", -25, 12),
                new ZaapLocation("Terres Désacrées", -15, 25),
                new ZaapLocation("Village des Eleveurs", -16, 1),
                new ZaapLocation("Plaines Rocheuses", -17, -47),
                new ZaapLocation("Massif de Cania", -13, -28),
                new ZaapLocation("Lac de Cania", -3, -42),
                new ZaapLocation("Routes Rocailleuses", -20, -20),
                new ZaapLocation("Champs de Cania", -27, -36),
                new ZaapLocation("Villages des Dopeuls", -34, -8),
                new ZaapLocation("Plaines des Porkass", -5, -23),
                new ZaapLocation("Village des Kanigs", 0, -56),
                new ZaapLocation("La Bourgade", -78, -41),
                new ZaapLocation ("Village cotier", -46, 18),
                new ZaapLocation ("Villade de Pandala", 20, -29)
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
            int x = random.Next(1236, 1370);
            int y = random.Next(791, 814);
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