using System;
using UnityEngine;
using System.IO;

public class SaveSystem
{
    
    /// <summary>
    /// 保存数据到本地文件，文件位置为：Application.persistentDataPath，其为：
    /// Windows: C:\Users\-username-\AppData\LocalLow\-companyName-\-productName-
    /// Mac: /Users/-username-/Library/Application Support/-companyName-/-productName-
    /// Linux: /home/-username-/.local/share/-companyName-/-productName-
    /// </summary>
    /// <param name="saveFileName">
    /// 保存文件的文件名，例如：save.data</param>
    /// <param name="saveData">
    /// 保存的数据，需要实现ISaveData接口
    /// </param>
    public static void SaveByJson(string saveFileName, object saveData)
    {
        var json = JsonUtility.ToJson(saveData);
        
        // Combine：将多个字符串组合成一个路径
        var path = Path.Combine(Application.persistentDataPath, saveFileName);

        try
        {
            // File.WriteAllText：将指定的文本内容写入到指定的文件中，如果文件不存在则创建文件，如果文件存在则覆盖文件
            File.WriteAllText(path, json);
#if UNITY_EDITOR
            Debug.Log($"Successfully Saved data to : {path}.");
#endif
        }
        catch (Exception e)
        {
#if UNITY_EDITOR
            Debug.LogError($"Failed to save data to : {path}.\n{e}");
#endif
            throw;
        }
    }

    
    /// <summary>
    /// 从本地文件中加载数据，文件位置为：Application.persistentDataPath
    /// </summary>
    /// <param name="saveFileName"></param>
    /// <typeparam name="T">
    /// </typeparam>
    /// <returns></returns>
    public static T LoadFromJson<T>(string saveFileName)
    {
        var path = Path.Combine(Application.persistentDataPath, saveFileName);

        try
        {
            var json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<T>(json);

#if UNITY_EDITOR
            Debug.Log($"Successfully Loaded data from : {path}.");
#endif
            return data;
        }
        catch (Exception e)
        {
#if UNITY_EDITOR
            Debug.LogError($"Failed to load data from : {path}.\n{e}");
#endif
            return default;
        }
    }

    
    /// <summary>
    /// 删除本地文件
    /// </summary>
    /// <param name="saveFileName"></param>
    public static void DeleteSaveFile(string saveFileName)
    {
        var path = Path.Combine(Application.persistentDataPath, saveFileName);
        try
        {
            File.Delete(path);
        }
        catch (Exception e)
        {
#if UNITY_EDITOR
            Debug.LogError($"Failed to delete data from : {path}.\n{e}");            
#endif
            throw;
        }
    }

    
    /// <summary>
    /// 判断本地文件是否存在
    /// </summary>
    /// <param name="saveFileName"></param>
    /// <returns></returns>
    public static bool SaveFileExists(string saveFileName)
    {
        var path = Path.Combine(Application.persistentDataPath, saveFileName);
        return File.Exists(path);
    }
    
}