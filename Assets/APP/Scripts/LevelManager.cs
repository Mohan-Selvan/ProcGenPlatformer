using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] LevelLoader levelLoader = null;
    [SerializeField] Tilemap tileMap;

    [SerializeField] Tile emptySpaceTile = null;
    [SerializeField] Tile platformTile = null;

    private static Dictionary<int, Tile> tileDic;

    private void Start()
    {
        tileDic = new Dictionary<int, Tile>()
        {
            { 0, emptySpaceTile },
            { 1, platformTile   }
        };
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            LevelData levelData = levelLoader.LoadLevelFromJson();
            UpdateLevel(levelData);
        }
    }

    private void UpdateLevel(LevelData levelData)
    {
        print("Updating level");

        int width = levelData.grid.Count;
        int height = levelData.grid[0].Count;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                int tileID = levelData.grid[i][(height - 1) - j];
                Vector3Int tileGridPosition = new Vector3Int(i, j, 0);

                print($"Tile : {tileGridPosition}, {tileID}");

                if (tileID == 1)
                {
                    tileMap.SetTile(tileGridPosition, platformTile);
                }
                else
                {
                    tileMap.SetTile(tileGridPosition, null);
                }

            }
        }

        print("Level updated");
    }
}
