using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HeatBot
{
    public class TreasureNearestZaap
    {
        private readonly ZaapMenuEN _zaapMenu;

        public TreasureNearestZaap(ZaapMenuEN zaapMenu)
        {
            _zaapMenu = zaapMenu;
        }

        private static (int x, int y) ParseCoordinates(string mapCoordinates)
        {
            var match = Regex.Match(mapCoordinates, @"\[(?<x>-?\d+),\s*(?<y>-?\d+)\]");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int x) && int.TryParse(match.Groups[2].Value, out int y))
            {
                return (x, y);
            }
            throw new FormatException($"Invalid map coordinates format: {mapCoordinates}");
        }

        private ZaapMenuEN.ZaapLocation FindNearestZaap(int x, int y)
        {
            ZaapMenuEN.ZaapLocation nearestZaap = null;
            double minDistance = double.MaxValue;

            foreach (var zaap in _zaapMenu.ZaapLocations)
            {
                double distance = Math.Sqrt(Math.Pow(zaap.X - x, 2) + Math.Pow(zaap.Y - y, 2));
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestZaap = zaap;
                }
            }

            if (nearestZaap == null)
            {
                throw new InvalidOperationException("No Zaap locations available.");
            }

            return nearestZaap;
        }

        public void TeleportToNearestZaap(string mapCoordinates)
        {
            try
            {
                Console.WriteLine("[INFO] Parsing map coordinates...");
                var (x, y) = ParseCoordinates(mapCoordinates);

                Console.WriteLine($"[INFO] Parsed coordinates: ({x}, {y})");

                Console.WriteLine("[INFO] Finding nearest Zaap location...");
                var nearestZaap = FindNearestZaap(x, y);

                Console.WriteLine($"[INFO] Nearest Zaap: {nearestZaap.Name} at ({nearestZaap.X}, {nearestZaap.Y})");

                Console.WriteLine("[INFO] Teleporting to nearest Zaap...");
                _zaapMenu.TeleportToLocation(nearestZaap.Name);

                Console.WriteLine("[INFO] Teleportation successful.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
            }
        }
    }
}