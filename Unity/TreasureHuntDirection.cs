using System;
using System.Drawing;
using OpenCvSharp;

namespace HeatBot
{
    public static class TreasureHuntDirection
    {

        public static string GetDetectedDirection(Bitmap regionImage)
        {
            try
            {
                using var screenMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(regionImage);

                var directionTemplates = new[]
                {
                    ("up", OpenCvSharp.Extensions.BitmapConverter.ToMat(huntimage.up)),
                    ("down", OpenCvSharp.Extensions.BitmapConverter.ToMat(huntimage.down)),
                    ("left", OpenCvSharp.Extensions.BitmapConverter.ToMat(huntimage.left)),
                    ("right", OpenCvSharp.Extensions.BitmapConverter.ToMat(huntimage.right))
                };

                foreach (var (direction, templateMat) in directionTemplates)
                {
                    using var result = new Mat();
                    Cv2.MatchTemplate(screenMat, templateMat, result, TemplateMatchModes.CCoeffNormed);

                    Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out OpenCvSharp.Point maxLoc);
                    const double threshold = 0.8;

                    if (maxVal >= threshold)
                    {
                        Console.WriteLine($"[INFO] Detected Direction: {direction} at ({maxLoc.X}, {maxLoc.Y})");
                        return direction;
                    }

                    templateMat.Dispose();
                }

                Console.WriteLine("[WARNING] No matching direction detected.");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to detect directions: {ex.Message}");
                return null;
            }
        }
    }
}