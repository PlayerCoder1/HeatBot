using System;
using System.Threading;
using System.Drawing;

namespace HeatBot
{
    public class TreasureTravel
    {

        private static readonly Rectangle TypingRegion = new Rectangle(34, 1020, 232, 13);

        public static void TypeTravelCommand(int x, int y)
        {
            try
            {

                FocusTypingRegion();

                string travelCommand = $"/travel {x} {y}";

                Console.WriteLine($"[INFO] Typing command: {travelCommand}");

                SimulateTyping(travelCommand);

                Console.WriteLine("[INFO] Travel command successfully typed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to type travel command: {ex.Message}");
            }
        }

        private static void FocusTypingRegion()
        {
            try
            {

                int x = TypingRegion.Left + (TypingRegion.Width / 2);
                int y = TypingRegion.Top + (TypingRegion.Height / 2);

                Console.WriteLine("[INFO] Focusing on the typing region...");

                ClickSimulate.SimulateClick(x, y);

                Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to focus on typing region: {ex.Message}");
            }
        }

        private static void SimulateTyping(string text)
        {
            foreach (char c in text)
            {
                Console.Write(c);
                SendKeys.SendWait(c.ToString());
                Thread.Sleep(100);
            }
            Console.WriteLine();
        }
    }

    public static class ClickSimulateHelper
    {
        public static void SimulateClick(int x, int y)
        {

            Console.WriteLine($"[INFO] Simulating mouse click at ({x}, {y})");
        }
    }
}