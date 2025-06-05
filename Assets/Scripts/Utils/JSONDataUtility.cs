using System.IO;
using UnityEngine;

public class DataKeyString {
    static string routes1 = "Route1Item";
    static string routes2 = "Route2Item";
    static string routes3 = "Route3Item";
    static string routes4 = "Route4Item";
}
/// <summary>
/// 一个用于在本地存储和加载JSON数据的工具类。
/// 使用 JsonUtility 对可序列化对象进行操作。
/// </summary>
public static class JSONDataUtility
{
    private static string GetFilePath(string key)
    {
        // .json 数据，让文件类型更明确
        return Path.Combine(Application.persistentDataPath, $"{key}.json");
    }

    /// <summary>
    /// 保存一个可序列化的对象到本地文件（JSON格式）。
    /// </summary>
    /// <typeparam name="T">要保存的数据类型。该类型必须带有 [System.Serializable] 属性。</typeparam>
    /// <param name="key">用于标识数据的唯一键名，也将作为文件名。</param>
    /// <param name="data">要保存的数据对象。</param>
    /// <returns>如果保存成功则返回 true，否则返回 false。</returns>
    public static bool SaveData<T>(string key, T data) where T : class
    {
        if (data == null)
        {
            Debug.LogError($"DataUtility: 尝试保存空数据到 '{key}'。");
            return false;
        }

        string json = JsonUtility.ToJson(data);
        string filePath = GetFilePath(key);

        try
        {
            // 确保目录存在
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(filePath, json);
            Debug.Log($"DataUtility: 数据 '{key}' 已成功保存到：{filePath}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"DataUtility: 保存数据 '{key}' 失败。错误：{e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 从本地文件（JSON格式）加载数据。
    /// </summary>
    /// <typeparam name="T">要加载的数据类型。该类型必须带有 [System.Serializable] 属性。</typeparam>
    /// <param name="key">用于标识数据的唯一键名，即文件名。</param>
    /// <returns>加载成功的数据对象；如果文件不存在、数据为空或反序列化失败，则返回 null。</returns>
    public static T LoadData<T>(string key) where T : class
    {
        string filePath = GetFilePath(key);

        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"DataUtility: 未找到数据文件 '{key}'，路径：{filePath}");
            return null;
        }

        try
        {
            string json = File.ReadAllText(filePath);
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning($"DataUtility: 数据文件 '{key}' 内容为空。");
                return null;
            }

            T data = JsonUtility.FromJson<T>(json);
            Debug.Log($"DataUtility: 数据 '{key}' 已成功从：{filePath} 加载。");
            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"DataUtility: 加载数据 '{key}' 失败。错误：{e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 检查特定键名的数据文件是否存在。
    /// </summary>
    /// <param name="key">要检查的键名。</param>
    /// <returns>如果文件存在则返回 true，否则返回 false。</returns>
    public static bool HasData(string key)
    {
        string filePath = GetFilePath(key);
        return File.Exists(filePath);
    }

    /// <summary>
    /// 删除特定键名的数据文件。
    /// </summary>
    /// <param name="key">要删除的键名。</param>
    /// <returns>如果删除成功则返回 true，如果文件不存在或删除失败则返回 false。</returns>
    public static bool DeleteData(string key)
    {
        string filePath = GetFilePath(key);
        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
                Debug.Log($"DataUtility: 数据文件 '{key}' 已成功删除。");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"DataUtility: 删除数据文件 '{key}' 失败。错误：{e.Message}");
                return false;
            }
        }
        else
        {
            Debug.LogWarning($"DataUtility: 尝试删除不存在的数据文件 '{key}'。");
            return false;
        }
    }
}