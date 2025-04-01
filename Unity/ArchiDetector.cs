using System;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace HeatBot
{
    public class ArchiDetector
    {
        private readonly Rectangle detectionRegion;
        private readonly string discordWebhookUrl;

        public ArchiDetector(string webhookUrl)
        {

            detectionRegion = new Rectangle(361, 26, 1555 - 361, 887 - 26);

            discordWebhookUrl = webhookUrl;
        }

        public async Task<(bool isDetected, int x, int y)> ScanForArchisAsync(PlayerCoordinateManager playerCoordinateManager)
        {
            try
            {

                using Bitmap screenshot = CaptureDetectionRegion();
                if (screenshot == null)
                {
                    Console.WriteLine("Failed to capture the detection region.");
                    return (false, playerCoordinateManager.CurrentX, playerCoordinateManager.CurrentY);
                }

                using Mat screenshotMat = BitmapConverter.ToMat(screenshot);

                using Mat archiImage = BitmapConverter.ToMat(fight.archi);
                using Mat archi2Image = BitmapConverter.ToMat(fight.archi2);

                bool foundArchi = await DetectImageAsync(screenshotMat, archiImage);
                bool foundArchi2 = await DetectImageAsync(screenshotMat, archi2Image);

                bool isDetected = foundArchi || foundArchi2;

                if (isDetected)
                {
                    string message = $"Archimonstre Detected at: [{playerCoordinateManager.CurrentX}, {playerCoordinateManager.CurrentY}]";
                    Console.WriteLine(message);

                    await SendDiscordEmbedNotification(playerCoordinateManager);
                }

                return (isDetected, playerCoordinateManager.CurrentX, playerCoordinateManager.CurrentY);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ArchiDetector: {ex.Message}");
                return (false, playerCoordinateManager.CurrentX, playerCoordinateManager.CurrentY);
            }
        }

        private Bitmap CaptureDetectionRegion()
        {
            try
            {
                Bitmap screenshot = new Bitmap(detectionRegion.Width, detectionRegion.Height);
                using (Graphics g = Graphics.FromImage(screenshot))
                {
                    g.CopyFromScreen(detectionRegion.Location, System.Drawing.Point.Empty, detectionRegion.Size);
                }
                return screenshot;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error capturing detection region: {ex.Message}");
                return null;
            }
        }

        private async Task<bool> DetectImageAsync(Mat source, Mat template)
        {
            return await Task.Run(() =>
            {
                using Mat result = new Mat();
                Cv2.MatchTemplate(source, template, result, TemplateMatchModes.CCoeffNormed);

                Cv2.MinMaxLoc(result, out double minVal, out double maxVal, out _, out _);

                double detectionThreshold = 0.85;

                Console.WriteLine($"Template match confidence: {maxVal}");
                return maxVal >= detectionThreshold;
            });
        }

        private async Task SendDiscordEmbedNotification(PlayerCoordinateManager playerCoordinateManager)
        {
            try
            {
                using HttpClient client = new HttpClient();

                string date = DateTime.Now.ToString("dd-MM-yyy");
                string time = DateTime.Now.ToString("HH:mm:ss");

                var payload = new
                {
                    embeds = new[]
                    {
                        new
                        {
                            title = "Archimonstre Detected! 🎉",
                            description = "**HeatBot Archimonstre Detector**",
                            color = 16711680,
                            fields = new[]
                            {
                                new { name = "Position", value = $"**[{playerCoordinateManager.CurrentX}, {playerCoordinateManager.CurrentY}]**", inline = true },
                                new { name = "Date", value = date, inline = true },
                                new { name = "Hour", value = time, inline = true }
                            },
                            footer = new
                            {
                                text = "HeatDetector",
                                icon_url = "https://cdn.discordapp.com/avatars/1324017312814202963/ddc93df03c2699ad0a47386050a8cb0c.webp?size=80"
                            }
                        }
                    }
                };

                string jsonPayload = JsonSerializer.Serialize(payload);

                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(discordWebhookUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Notification sent to Discord successfully.");
                }
                else
                {
                    Console.WriteLine($"Failed to send Discord notification. Status Code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending Discord notification: {ex.Message}");
            }
        }
    }
}