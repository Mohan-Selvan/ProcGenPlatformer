using System.Collections.Generic;
using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Linq;

[Serializable]
public class PathData
{
    [JsonProperty("complexity")]
    public float Complexity{ get; set; }
}

