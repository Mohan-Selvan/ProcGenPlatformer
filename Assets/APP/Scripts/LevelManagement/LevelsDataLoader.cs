using System;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class LevelsDataLoader : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] string levelsFilePath = string.Empty;

    internal LevelsData LoadLevelFromJson()
    {
        Debug.Log($"Loading from file : {levelsFilePath}");

        LevelsData data = null;

        try
        {
            string content = ReadstringFromFile(levelsFilePath);
            print(content);
            data = JsonConvert.DeserializeObject<LevelsData>(content);
        }
        catch(Exception ex)
        {
            Debug.LogError($"Exception occured while decoding : {ex}");
        }

        Debug.Log($"Loaded from file : {levelsFilePath}");
        return data;
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
