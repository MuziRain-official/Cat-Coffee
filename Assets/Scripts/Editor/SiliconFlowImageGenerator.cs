using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CatCafe.Editor
{
    /// <summary>
    /// 硅基流动 SiliconFlow 图生成器（Editor 工具，同步请求）。
    /// 菜单：Window → Cat Cafe → 生成像素资产。
    /// 需要环境变量 SILICONFLOW_API_KEY。
    /// </summary>
    public static class SiliconFlowImageGenerator
    {
        private const string ApiUrl = "https://api.siliconflow.cn/v1/images/generations";
        private const string EnvVarName = "SILICONFLOW_API_KEY";

        private const string StylePrefix =
            "anime pixel art, top-down view, cozy cat cafe, cream and warm brown retro palette, " +
            "soft low-saturation colors, no outline, clean shapes, warm cozy atmosphere, " +
            "cute anime style, game asset sprite";

        private static readonly (string name, string desc)[] Assets =
        {
            ("coffee_machine", "an espresso coffee machine on a wooden counter, single centered object"),
            ("counter", "a wooden bar counter for assembling drinks, top-down, single centered object"),
            ("warmer", "a food warmer counter with a glass shelf, top-down, single centered object"),
            ("coffee_cup", "a cup of latte coffee with milk foam on top, top-down, single centered object"),
            ("cat", "an orange tabby cat sitting, top-down view, single centered character"),
            ("player", "a cute barista girl with apron, top-down view, single centered character"),
            ("table", "a round wooden cafe table, top-down, single centered object"),
            ("chair", "a wooden chair, top-down view, single centered object"),
            ("customer", "a cafe customer character, top-down view, single centered character"),
            ("cat_nest", "a round cat bed cushion, top-down, single centered object"),
            ("cat_pad", "a small cat mat, top-down, single centered object"),
            ("floor", "wooden floor tile texture, top-down, seamless tileable"),
            ("wall", "cream wall texture, top-down, seamless tileable"),
        };

        [MenuItem("Window/Cat Cafe/生成像素资产")]
        public static void GenerateAllAssets()
        {
            string key = GetApiKey();
            if (string.IsNullOrEmpty(key))
            {
                EditorUtility.DisplayDialog("缺少 API Key",
                    $"请设置环境变量 {EnvVarName}（硅基流动 API Key），然后重启 Unity。", "知道了");
                return;
            }

            if (!EditorUtility.DisplayDialog("确认批量生成",
                $"将生成 {Assets.Length} 张像素资产到 Assets/Art，使用免费模型 Kolors。继续？", "生成", "取消"))
                return;

            try
            {
                using var client = new HttpClient();
                int done = 0;
                foreach (var (name, desc) in Assets)
                {
                    EditorUtility.DisplayProgressBar("生成像素资产", $"{name} ({done + 1}/{Assets.Length})", (float)done / Assets.Length);
                    try
                    {
                        string prompt = $"{StylePrefix}, {desc}";
                        byte[] png = RequestImage(client, key, prompt);
                        if (png != null)
                        {
                            SavePng(name, png);
                            Debug.Log($"✓ {name}");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"✗ {name} 失败: {e.Message}");
                    }
                    done++;
                }
                AssetDatabase.Refresh();
                Debug.Log("全部像素资产生成完成，见 Assets/Art/。");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static byte[] RequestImage(HttpClient client, string key, string prompt)
        {
            var payload = new Dictionary<string, object>
            {
                { "model", "Kwai-Kolors/Kolors" },
                { "prompt", prompt },
                { "image_size", "1024x1024" },
                { "batch_size", 1 },
                { "num_inference_steps", 20 },
            };
            string json = MiniJson.Serialize(payload);

            using var req = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
            req.Headers.TryAddWithoutValidation("Authorization", "Bearer " + key);
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var resp = client.SendAsync(req).GetAwaiter().GetResult();
            string body = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            if (!resp.IsSuccessStatusCode)
            {
                throw new Exception($"HTTP {(int)resp.StatusCode}: {body.Substring(0, Math.Min(300, body.Length))}");
            }

            string url = ExtractUrl(body);
            if (string.IsNullOrEmpty(url)) throw new Exception("响应里没有图片 URL");

            using var imgResp = client.GetAsync(url).GetAwaiter().GetResult();
            imgResp.EnsureSuccessStatusCode();
            return imgResp.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
        }

        private static string ExtractUrl(string jsonText)
        {
            int start = jsonText.IndexOf("\"url\":\"", StringComparison.Ordinal);
            if (start < 0) start = jsonText.IndexOf("\"url\": \"", StringComparison.Ordinal);
            if (start < 0) return null;
            start = jsonText.IndexOf('"', start + 6) + 1;
            int end = jsonText.IndexOf('"', start);
            if (end < 0) return null;
            return jsonText.Substring(start, end - start);
        }

        private static void SavePng(string name, byte[] pngBytes)
        {
            string folder = "Assets/Art";
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, name + ".png");
            File.WriteAllBytes(path, pngBytes);
        }

        private static string GetApiKey()
        {
            return Environment.GetEnvironmentVariable(EnvVarName);
        }
    }

    /// <summary>极简 JSON 序列化。</summary>
    internal static class MiniJson
    {
        public static string Serialize(object obj)
        {
            if (obj == null) return "null";
            if (obj is string s) return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
            if (obj is bool b) return b ? "true" : "false";
            if (obj is int or long or float or double)
                return Convert.ToString(obj, System.Globalization.CultureInfo.InvariantCulture);
            if (obj is IDictionary<string, object> dict)
            {
                var sb = new StringBuilder("{");
                bool first = true;
                foreach (var kv in dict)
                {
                    if (!first) sb.Append(',');
                    sb.Append("\"").Append(kv.Key).Append("\":").Append(Serialize(kv.Value));
                    first = false;
                }
                return sb.Append('}').ToString();
            }
            if (obj is System.Collections.IEnumerable list)
            {
                var sb = new StringBuilder("[");
                bool first = true;
                foreach (var item in list)
                {
                    if (!first) sb.Append(',');
                    sb.Append(Serialize(item));
                    first = false;
                }
                return sb.Append(']').ToString();
            }
            return "null";
        }
    }
}
