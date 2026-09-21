using System.IO;
using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 存档管理器：把 Progress 持久化到磁盘 JSON。
    /// 路径：Application.persistentDataPath/save.json
    /// </summary>
    public static class ProgressStore
    {
        private static string SavePath =>
            Path.Combine(Application.persistentDataPath, "catcafe_save.json");

        public static void Save(Progress progress)
        {
            try
            {
                File.WriteAllText(SavePath, progress.ToJson());
            }
            catch (System.Exception e)
            {
                Debug.LogError($"存档失败: {e.Message}");
            }
        }

        public static Progress Load()
        {
            try
            {
                if (File.Exists(SavePath))
                    return Progress.FromJson(File.ReadAllText(SavePath));
            }
            catch (System.Exception e)
            {
                Debug.LogError($"读档失败: {e.Message}");
            }
            return new Progress(); // 新档
        }

        public static void Clear()
        {
            try { if (File.Exists(SavePath)) File.Delete(SavePath); }
            catch { }
        }
    }
}
