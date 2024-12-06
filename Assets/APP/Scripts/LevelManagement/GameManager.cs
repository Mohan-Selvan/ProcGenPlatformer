using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Cinemachine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] LevelLoader levelLoader = null;
    [SerializeField] CinemachineCamera playerCamera = null;
    [SerializeField] PlayerController playerPrefab = null;
    [SerializeField] Tilemap tileMap;

    [Header("Settings - Camera")]
    [SerializeField] Vector2 cameraFovRange = Vector2.one;
    [SerializeField] float cameraFOVStep = 0.1f;
    
    [Header("Controls - Level")]
    [SerializeField] KeyCode loadLevelKey = KeyCode.L;
    [SerializeField] KeyCode startGameKey = KeyCode.P;

    [Header("Tile settings")]
    [SerializeField] Tile emptySpaceTile = null;
    [SerializeField] Tile platformTile = null;

    [Header("Settings - Debug only")]
    [SerializeField] LevelData levelData = null;
    [SerializeField] PlayerController playerController = null;

    void Update()
    {
        if (Input.GetKeyDown(loadLevelKey))
        {
            Debug.Log("Loading level");
            levelData = levelLoader.LoadLevelFromJson();
            DrawLevel(levelData);
        }

        if (Input.GetKeyDown(startGameKey))
        {
            if(playerController != null)
            {
                Destroy(playerController);
                playerController = null;
            }

            List<Vector2Int> playerPath = levelData.PlayerPath;
            Vector2Int spawnCell = playerPath[0];
            Vector2Int endCell = playerPath[playerPath.Count - 1];

            Vector2 spawnPosition = tileMap.CellToWorld((Vector3Int)spawnCell);

            Debug.Log("Spawning player..");
            playerController = Instantiate<PlayerController>(playerPrefab, position: spawnPosition, rotation: Quaternion.identity);
            playerCamera.Target.TrackingTarget = playerController.transform;
        }

        //Camera controls
        if(Input.mouseScrollDelta.y > 0f)
        {
            float currentFOV = this.playerCamera.Lens.OrthographicSize;
            this.playerCamera.Lens.OrthographicSize = Mathf.Clamp(currentFOV + cameraFOVStep, cameraFovRange.x, cameraFovRange.y);
        }
        else if(Input.mouseScrollDelta.y < 0f)
        {
            float currentFOV = this.playerCamera.Lens.OrthographicSize;
            this.playerCamera.Lens.OrthographicSize = Mathf.Clamp(currentFOV - cameraFOVStep, cameraFovRange.x, cameraFovRange.y);
        }
    }

    private void DrawLevel(LevelData data)
    {
        print("Drawing level");

        int width = data.GridSize.x;
        int height = data.GridSize.y;
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int tileID = data.Grid[x][y];
                Vector3Int tileGridPosition = new Vector3Int(x, y, 0);

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

        print("Level draw complete");
    }
}
