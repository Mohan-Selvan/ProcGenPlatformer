using UnityEngine;
using System;
using Newtonsoft.Json;

[Serializable]
public class LevelData
{
    [JsonProperty("path_id")]
    public int PathId { get; set; }

    [JsonProperty("path_data")]
    public PathData PathData { get; set; }

    [JsonProperty("env_data")]
    public EnvData EnvData { get; set; } 
}
