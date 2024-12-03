using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;


[Serializable]
public class LevelData
{
    public List<List<int>> grid { get; set; }

    internal void PrintData()
    {
        Debug.Log($"{this.grid.ToString()}");
    }
}


public class LevelLoader : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] string levelFilePath = string.Empty;


    internal LevelData LoadLevelFromJson()
    {
        Debug.Log($"Loading from file : {levelFilePath}");

        LevelData levelData = null;

        try
        {
            string content = ReadstringFromFile(levelFilePath);
            print(content);
            levelData = JsonConvert.DeserializeObject<LevelData>(content);
        }
        catch(Exception ex)
        {
            Debug.LogError($"Exception occured while decoding : {ex }");
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
