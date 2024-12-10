using System;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class LevelLoader : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] string levelFilePath = string.Empty;

    internal EnvData LoadLevelFromJson()
    {
        Debug.Log($"Loading from file : {levelFilePath}");

        EnvData levelData = null;

        try
        {
            string content = ReadstringFromFile(levelFilePath);
            print(content);
            levelData = JsonConvert.DeserializeObject<EnvData>(content);
        }
        catch(Exception ex)
        {
            Debug.LogError($"Exception occured while decoding : {ex}");
        }

        Debug.Log($"Loaded from file : {levelFilePath}");
        return levelData;
    }

    private string ReadstringFromFile(string filePath)
    {
        try
        {
            string content = string.Empty;

            if(!File.Exists(filePath))
            {
                Debug.LogError($"File does not exist : {filePath}");
                return content;
            }
            
            content = File.ReadAllText(filePath);
            Debug.Log($"Loaded contents from file : {filePath}");

            return content;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error reading file : {ex.Message}");
            return string.Empty;
        }
    }
}
