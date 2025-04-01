using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HeatBot
{
    public class TreasureAPI
    {
        private static readonly string ApiUrl = "https://api.dofusdb.fr/treasure-hunt";
        private static readonly string Token = "03AFcWeA6iXe2CegPvkNIcpB6O9tcA8z7k1vHj5O_QcUWFH6O0NQOX4XDvKQPo2BoS24Sj710IAsfbPdmvo8IYIny1mBG6v9AZRFzJVIpehWc099KQVRVvYQspUcHLQBFptqUFjxXgEVSz5MDPU1O1ilH2wlnHaVTipf65UfrBdOl_347xOseznI__16PoaGYSmvl_fUiOGdijPeo4zl6QImJjdU2lSyVhldf6q6ewF4CwU4_nea8AEYkhz0F8B_o2ldIyivkN6hDh401_-itEcSSijA8vA933gJQlgILEVuYRj0mo1sLcrNrbbl9_tO_l1lRnr4gcW5GXkJHQshTLq_N0xHW5xQ_mj1Nrs8A1IcwqofuQgw9N1MaHgsu3BlmpDXPCL03o4KJ4ShHcn5gXXN5t0zjZwWnowpiFnTiAjMnCvx-IqL8-AJ_TAl5hh7hsCg9e50WtLVCTGoBltJ-5INgjCQsx5_bEDvDlltXPvPsBd4ethwtq6DvSaZy4Zd5hFh-W6ULyD-UO3wmA4-FtOufL5KGMRMJRdkt7b6s5kVWWFI8jHy6Dms_v2RYMDlUwzA3EeGP57uliHWqIpGQotqTDVSSUbbMcJqU87_u08SLWQ-Zo5nW5GbBo2EzTOw617bvc72PNEIexlzyX6K7Sa5M0GN_jowReD1pef6CvmbNZiFvp05lf31SaXXEktb2vGI_8afcrWit1DbsGEnADMH6n2o8-wyOmQE1Z_jYfos-9A9jGwPAqOc6u4MIfdcdxeEcJCkLpX-lG2NZhVWp3zu5mG02QMCh2W27bPou5iPRGBff5t4hBbwRGwKqo3K65VN7SmAZsNbvEPbiEC5ZaAWaPOl8MD5tkQdmipG3OTCMabRLU4sNCM6CTnfBn3Nb4n-YDRrGyRzO2s7iQOx_BiLA57l_KKqceYg0UDhtF9RqzBTzh3OJTsouynCuLhI7sEUQy6LBze0M7qxukw-YuVdL-D5aMTP7L_VC6-K4w47vNGL9hYr5Q0R0GVUGBdOsFwg-53VNG6kBZz4magKA9RYsUUXqJsCGZO-2hjp3ZYUOSX6LwhuvqBGW0rEEYK453s50VgGHZqPxoaliMB7H1QJk0sGcz413y09_TK0Il8qdiWHR6dfz8LA7Yhf9E-tcWmcAOU5kPR7yArxkKTu_xkebu7PCT-PhZTJCuM7bJ3D5i8ZbMkVf8sUhndDWqrzNRMRIG3xCL-Zr4EE3mEJV0Xhr356RkJdY41WIRT8Yrj4wqe-X1TaAMhyiL7JMvVhzYwFocfoimfUMGoOmYfFZRCDVt0S5Y3YlEe7S_TRDB_QR4NTK8OW3YcskVZa4gXsBFX-w1iG-ttkdoXdJPkYaryDRHvWHDJVLiPbF-NVqVk7XV34kZA54tSKVVXKA4HXWhaxcBedTe0dKaZN6duI_3_INlINSHLQC7PXoR9ImtUQwr3MNIzVZzi3_omn79AmnnNa-1llzR_GaM2NiGl54lGB9c5KR8cYjeUw8uHwjVtCX418bl0M7GtBhwxVJmAPzbUXT7ueAVcnkVbn8fYqVo1bWFg4BcibT8YqpOmlKpDW02GzmZRGmCbwkcNtrFH0_9nHpkTtOyZcgrtkSj0JcSK3gtil_kTvY80RWU2I4SUUdZe3eVYVo3Ybq0VfUFQVrU1aFKyMEn9V95cTpuZxECJs2mCuq8Wp2cRDRavaHMc9DIOSV4ikexDQKPOk8NDAdcpK3muuwPCYcGqXT6GK68NM84gZC6kURWi_g6Joez10GK32M_5V5cB8K4oKjwnKZW2BQnr2one_1BCCTQGTDZrmwmj4TwIBWqMpus8LCdpxhs_oJYbUCFvZFbtuDNUYN0aX2Ax2qO9Ef7SVqwxcbkBoAeCGNMJb_xjWZdUmRPTm8ZB8Htk6hhqcnsd6CaBdc_mh1TUd9Yz2IGe2cTUa9Na4ZoG2WWMOXZkUNtCIIVINLSgVWwkEkATrUrefNdYgFpI54ld0nZWdeEJmVeLrcWHYCmi_KMeGNeMRfXoGhD-R2RN8CJlA00JOs";

        public static int LevenshteinDistance(string source, string target)
        {
            if (string.IsNullOrEmpty(source)) return target.Length;
            if (string.IsNullOrEmpty(target)) return source.Length;

            int sourceLength = source.Length;
            int targetLength = target.Length;

            var distance = new int[sourceLength + 1, targetLength + 1];

            for (int i = 0; i <= sourceLength; distance[i, 0] = i++) { }
            for (int j = 0; j <= targetLength; distance[0, j] = j++) { }

            for (int i = 1; i <= sourceLength; i++)
            {
                for (int j = 1; j <= targetLength; j++)
                {
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;
                    distance[i, j] = Math.Min(
                        Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                        distance[i - 1, j - 1] + cost);
                }
            }

            return distance[sourceLength, targetLength];
        }

        public static (int posX, int posY)? FindClueLocation(string apiResponse, string clue)
        {
            try
            {
                using JsonDocument document = JsonDocument.Parse(apiResponse);
                JsonElement root = document.RootElement;

                if (root.TryGetProperty("data", out JsonElement dataArray))
                {
                    string normalizedClue = clue.Trim().ToLowerInvariant();
                    int similarityThreshold = 3;
                    string closestClue = null;
                    int closestDistance = int.MaxValue;
                    (int posX, int posY)? closestLocation = null;

                    foreach (JsonElement location in dataArray.EnumerateArray())
                    {
                        if (location.TryGetProperty("pois", out JsonElement poisArray))
                        {
                            foreach (JsonElement poi in poisArray.EnumerateArray())
                            {
                                if (poi.TryGetProperty("name", out JsonElement nameElement) &&
                                    nameElement.TryGetProperty("en", out JsonElement nameValue))
                                {
                                    string normalizedName = nameValue.GetString()?.Trim().ToLowerInvariant();

                                    if (!string.IsNullOrEmpty(normalizedName))
                                    {
                                        int distance = LevenshteinDistance(normalizedName, normalizedClue);

                                        if (distance <= similarityThreshold)
                                        {

                                            int posX = location.GetProperty("posX").GetInt32();
                                            int posY = location.GetProperty("posY").GetInt32();
                                            return (posX, posY);
                                        }

                                        if (distance < closestDistance)
                                        {
                                            closestDistance = distance;
                                            closestClue = nameValue.GetString();
                                            closestLocation = (
                                                location.GetProperty("posX").GetInt32(),
                                                location.GetProperty("posY").GetInt32()
                                            );
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (closestClue != null)
                    {
                        Console.WriteLine($"[INFO] Exact clue '{clue}' not found. Closest clue: '{closestClue}' (distance: {closestDistance}).");
                        return closestLocation;
                    }
                }

                Console.WriteLine($"[INFO] Clue '{clue}' not found in API response and no similar clues found.");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to parse API response: {ex.Message}");
                return null;
            }
        }

        public static async Task<string> FetchTreasureHuntDataAsync(int x, int y, string direction)
        {
            using (var client = new HttpClient())
            {
                var queryParams = new StringBuilder();
                queryParams.Append($"x={x}&");
                queryParams.Append($"y={y}&");
                queryParams.Append($"direction={direction}&");
                queryParams.Append($"$limit=50&");
                queryParams.Append($"lang=fr");

                var requestUrl = $"{ApiUrl}?{queryParams}";

                client.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0.0.0 Safari/537.36 OPR/114.0.0.0");
                client.DefaultRequestHeaders.Add("Origin", "https://dofusdb.fr");
                client.DefaultRequestHeaders.Add("Referer", "https://dofusdb.fr/");
                client.DefaultRequestHeaders.Add("Token", Token);

                try
                {
                    var response = await client.GetAsync(requestUrl);
                    response.EnsureSuccessStatusCode();

                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("[INFO] Successfully fetched treasure hunt data.");
                    return responseContent;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Failed to fetch treasure hunt data: {ex.Message}");
                    return null;
                }
            }
        }
    }
}