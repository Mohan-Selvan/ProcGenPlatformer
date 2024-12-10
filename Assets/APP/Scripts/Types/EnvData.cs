using System.Collections.Generic;
using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Linq;

[Serializable]
public class EnvData
{
    [JsonProperty("grid_size")]
    public List<int> _gridSizeRaw { get; set; }

    [JsonProperty("grid")]
    public List<List<int>> _gridRaw { get; set; }

    [JsonProperty("player_path")]
    public List<List<int>> _playerPathRaw { get; set; }

    [JsonProperty("reachable_cells")]
    public List<List<int>> _reachableCellsRaw { get; set; }

    [JsonProperty("hanging_cells")]
    public List<List<int>> _hangingCellsRaw { get; set; }

    [JsonProperty("reachability")]
    public float Reachability { get; set; }

    [JsonProperty("is_solvable")]
    public bool IsSolvable { get; set; }


    [JsonIgnore]
    public Vector2Int GridSize = default;

    [JsonIgnore]
    public List<List<int>> Grid = null;

    [JsonIgnore]
    public List<Vector2Int> PlayerPath = null;

    [JsonIgnore]
    public List<Vector2Int> ReachableCells = null;

    [JsonIgnore]
    public List<Vector2Int> HangingCells = null;



    //Serialization callbacks
    [OnDeserialized]
    private void OnDeserializedCallback(StreamingContext context)
    {
        GridSize = new Vector2Int((int)this._gridSizeRaw[0], (int)this._gridSizeRaw[1]);

        PlayerPath = new List<Vector2Int>();
        foreach (var cell in this._playerPathRaw)
        {
            PlayerPath.Add(new Vector2Int(cell[0], (GridSize.y - 1) - cell[1]));
        }

        ReachableCells = new List<Vector2Int>();
        foreach (var cell in this._reachableCellsRaw)
        {
            ReachableCells.Add(new Vector2Int(cell[0], (GridSize.y - 1) - cell[1]));
        }

        HangingCells = new List<Vector2Int>();
        foreach (var cell in this._hangingCellsRaw)
        {
            HangingCells.Add(new Vector2Int(cell[0], (GridSize.y - 1) - cell[1]));
        }

        int width = GridSize.x;
        int height = GridSize.y;

        this.Grid = new List<List<int>>();

        for(int i = 0; i < width; i++)
        {
            List<int> list = new List<int>();
            for(int j = 0; j < height; j++)
            {
                list.Add(-1);
            }

            this.Grid.Add(list);
        }


        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                this.Grid[x][y] = this._gridRaw[x][(height - 1) - y]; 
            }
        }
    }

    internal void PrintData()
    {
        Debug.Log($"{this.Grid.ToString()}");

        Debug.Log($"Print start : player path");

        foreach(var cell in this.PlayerPath)
        {
            Debug.Log(cell);
        }

        Debug.Log($"Print complete : player path");
    }
}

