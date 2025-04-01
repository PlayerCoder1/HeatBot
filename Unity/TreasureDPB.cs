using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace HeatBot
{
    public class TreasureDPB
    {
        private readonly string _jsonFilePath;

        public TreasureDPB(string jsonFilePath)
        {
            _jsonFilePath = jsonFilePath;
        }

        public (int x, int y)? FindClosestMap(string clue, int currentX, int currentY, string direction)
        {
            try
            {

                string jsonContent = File.ReadAllText(_jsonFilePath);
                using JsonDocument document = JsonDocument.Parse(jsonContent);

                JsonElement root = document.RootElement;

                JsonElement cluesArray = root.GetProperty("clues");
                JsonElement mapsArray = root.GetProperty("maps");

                int clueId = GetClueId(clue, cluesArray);
                if (clueId == -1)
                {
                    Console.WriteLine($"[ERROR] Clue '{clue}' not found in the JSON file.");
                    return null;
                }

                var directionalMap = GetDirectionalMapWithClue(clueId, mapsArray, currentX, currentY, direction);
                if (directionalMap.HasValue)
                {
                    Console.WriteLine($"[INFO] Map for clue '{clue}' found at coordinates: ({directionalMap.Value.x}, {directionalMap.Value.y}) considering direction '{direction}'.");
                }
                else
                {
                    Console.WriteLine($"[WARNING] No map found for clue '{clue}' considering direction '{direction}'.");
                }

                return directionalMap;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to process the JSON file: {ex.Message}");
                return null;
            }
        }

        private (int x, int y)? GetDirectionalMapWithClue(int clueId, JsonElement mapsArray, int currentX, int currentY, string direction)
        {
            double closestDistance = double.MaxValue;
            (int x, int y)? closestMap = null;

            foreach (JsonElement mapElement in mapsArray.EnumerateArray())
            {

                if (mapElement.TryGetProperty("clues", out JsonElement cluesArray))
                {
                    var clueIds = cluesArray.EnumerateArray().Select(c => c.GetInt32());

                    if (clueIds.Contains(clueId))
                    {

                        int mapX = mapElement.GetProperty("x").GetInt32();
                        int mapY = mapElement.GetProperty("y").GetInt32();

                        if (IsCorrectDirection(currentX, currentY, mapX, mapY, direction))
                        {

                            double distance = CalculateDistance(currentX, currentY, mapX, mapY);

                            if (distance < closestDistance)
                            {
                                closestDistance = distance;
                                closestMap = (mapX, mapY);
                            }
                        }
                    }
                }
            }

            return closestMap;
        }

        private bool IsCorrectDirection(int currentX, int currentY, int mapX, int mapY, string direction)
        {
            return direction switch
            {
                "up" => mapY < currentY && mapX == currentX,
                "down" => mapY > currentY && mapX == currentX,
                "left" => mapX < currentX && mapY == currentY,
                "right" => mapX > currentX && mapY == currentY,
                _ => false
            };
        }

        public int GetClueId(string clue, JsonElement cluesArray)
        {
            int similarityThreshold = 10;
            int closestClueId = -1;
            int closestDistance = int.MaxValue;
            string closestHint = null;

            foreach (JsonElement clueElement in cluesArray.EnumerateArray())
            {
                if (clueElement.TryGetProperty("hinten", out JsonElement hintElement))
                {
                    string hint = hintElement.GetString()?.Trim();

                    int distance = CalculateLevenshteinDistance(clue.Trim().ToLowerInvariant(), hint?.ToLowerInvariant() ?? "");
                    if (distance == 0)
                    {

                        return clueElement.GetProperty("clueid").GetInt32();
                    }
                    else if (distance < similarityThreshold && distance < closestDistance)
                    {

                        closestDistance = distance;
                        closestClueId = clueElement.GetProperty("clueid").GetInt32();
                        closestHint = hint;
                    }
                }
            }

            if (closestClueId != -1)
            {
                Console.WriteLine($"[INFO] Clue '{clue}' not found exactly, but closest match is '{closestHint}' with clue ID {closestClueId}.");
            }
            else
            {
                Console.WriteLine($"[WARNING] Clue '{clue}' not found in the JSON file.");
            }

            return closestClueId;
        }

        public static int CalculateLevenshteinDistance(string source, string target)
        {
            if (string.IsNullOrEmpty(source)) return target.Length;
            if (string.IsNullOrEmpty(target)) return source.Length;

            int sourceLength = source.Length;
            int targetLength = target.Length;
            int[,] distance = new int[sourceLength + 1, targetLength + 1];

            for (int i = 0; i <= sourceLength; i++) distance[i, 0] = i;
            for (int j = 0; j <= targetLength; j++) distance[0, j] = j;

            for (int i = 1; i <= sourceLength; i++)
            {
                for (int j = 1; j <= targetLength; j++)
                {
                    int cost = (source[i - 1] == target[j - 1]) ? 0 : 1;
                    distance[i, j] = Math.Min(
                        Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                        distance[i - 1, j - 1] + cost
                    );
                }
            }

            return distance[sourceLength, targetLength];
        }

        private double CalculateDistance(int x1, int y1, int x2, int y2)
        {
            int dx = x2 - x1;
            int dy = y2 - y1;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}