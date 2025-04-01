using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HeatBot
{
    public static class ScriptingManager
    {
        public class Script
        {
            [JsonPropertyName("clicks")]
            public List<ActionAtCoordinate> Clicks { get; set; } = new List<ActionAtCoordinate>();

            [JsonPropertyName("bank")]
            public List<ActionAtCoordinate> Bank { get; set; } = new List<ActionAtCoordinate>();

            [JsonPropertyName("autoZaapOnFullPods")]
            public AutoZaapConfig AutoZaap { get; set; }
        }

        public class AutoZaapConfig
        {
            [JsonPropertyName("enabled")]
            public bool Enabled { get; set; }

            [JsonPropertyName("location")]
            public string Location { get; set; }
        }
        public class ActionAtCoordinate
        {
            [JsonPropertyName("x")]
            public int X { get; set; }

            [JsonPropertyName("y")]
            public int Y { get; set; }

            [JsonPropertyName("action")]
            public string Action { get; set; }

            [JsonPropertyName("bankStyle")]
            public string BankStyle { get; set; }

            [JsonPropertyName("location")]
            public string Location { get; set; }
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

        public static Script LoadScript(string scriptPath)
        {
            try
            {
                Console.WriteLine($"Reading script file: {scriptPath}");
                Logger.LogMessage($"Script: {scriptPath} Loaded Correctly", Color.Cyan);
                string scriptContent = File.ReadAllText(scriptPath);
                Console.WriteLine($"Script content: {scriptContent}");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<Script>(scriptContent, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading script: {ex.Message}");
                Logger.LogMessage($"Error loading script: {ex.Message}", Color.Red);
                throw new InvalidOperationException("Failed to load script. Ensure the file is correctly formatted.", ex);
            }
        }
        public static void ExecuteScript(string scriptPath, PlayerCoordinateManager playerCoordinateManager)
        {
            try
            {
                Console.WriteLine($"Reading script file: {scriptPath}");
                string scriptContent = File.ReadAllText(scriptPath);
                Console.WriteLine($"Script content: {scriptContent}");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                Script script = JsonSerializer.Deserialize<Script>(scriptContent, options);

                if (script?.Clicks != null && script.Clicks.Count > 0)
                {
                    Console.WriteLine($"Executing {script.Clicks.Count} actions.");
                    foreach (var action in script.Clicks)
                    {

                        if (playerCoordinateManager.CurrentX == action.X && playerCoordinateManager.CurrentY == action.Y)
                        {
                            Console.WriteLine($"Executing action '{action.Action}' at coordinates ({action.X}, {action.Y}).");
                            Logger.LogMessage($"Going '{action.Action}' at ({action.X}, {action.Y}).", Color.Green);
                            if (action.Action.Equals("up", StringComparison.OrdinalIgnoreCase))
                            {
                                MapChange.ChangeMap("up");
                                playerCoordinateManager.MoveUp();
                            }
                            else if (action.Action.Equals("down", StringComparison.OrdinalIgnoreCase))
                            {
                                MapChange.ChangeMap("down");
                                playerCoordinateManager.MoveDown();
                            }
                            else if (action.Action.Equals("left", StringComparison.OrdinalIgnoreCase))
                            {
                                MapChange.ChangeMap("left");
                                playerCoordinateManager.MoveLeft();
                            }
                            else if (action.Action.Equals("right", StringComparison.OrdinalIgnoreCase))
                            {
                                MapChange.ChangeMap("right");
                                playerCoordinateManager.MoveRight();
                            }
                            else
                            {
                                Console.WriteLine($"Invalid action '{action.Action}' specified.");
                            }

                            Thread.Sleep(1000);
                        }
                        else
                        {
                            Console.WriteLine($"No action for coordinates ({playerCoordinateManager.CurrentX}, {playerCoordinateManager.CurrentY}).");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No valid actions found in the script.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during script execution: {ex.Message}");
            }
        }
    }
}