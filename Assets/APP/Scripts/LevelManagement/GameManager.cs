using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Cinemachine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] LevelsDataLoader levelsDataLoader = null;
    [SerializeField] CinemachineCamera playerCamera = null;
    [SerializeField] PlayerController playerPrefab = null;
    [SerializeField] Tilemap tileMap;

    [Header("Settings - Camera")]
    [SerializeField] Vector2 cameraFovRange = Vector2.one;
    [SerializeField] float cameraFOVStep = 0.1f;
    
    [Header("Controls - Level")]
    [SerializeField] KeyCode loadLevelKey = KeyCode.L;
    [SerializeField] KeyCode startGameKey = KeyCode.P;
    [SerializeField] KeyCode previousLevelKey = KeyCode.Alpha9;
    [SerializeField] KeyCode nextLevelKey = KeyCode.Alpha0;

    [Header("Tile settings")]
    [SerializeField] Tile emptySpaceTile = null;
    [SerializeField] Tile platformTile = null;

    [Header("Settings - Debug only")]
    [SerializeField] LevelsData levelsData = null;
    [SerializeField] PlayerController playerController = null;

    [Space(10)]
    [SerializeField] int currentLevelIndex = 0;
    [SerializeField] LevelData activeLevel = null;
    
    void Update()
    {
        if (Input.GetKeyDown(loadLevelKey))
        {
            Debug.Log("Loading levels data");
            levelsData = levelsDataLoader.LoadLevelFromJson();
            currentLevelIndex = 0;
            activeLevel = levelsData.Levels[currentLevelIndex];
            Debug.Log("Loaded levels data");
        }

        if (Input.GetKeyDown(startGameKey))
        {
            RestartActiveLevel();
        }


        if (Input.GetKeyDown(previousLevelKey))
        {
            currentLevelIndex--;
            if(currentLevelIndex < 0)
            {
                currentLevelIndex = levelsData.Levels.Count - 1;
            }

            activeLevel = levelsData.Levels[currentLevelIndex];
            RestartActiveLevel();
        }

        if (Input.GetKeyDown(nextLevelKey))
        {
            currentLevelIndex++;
            if (currentLevelIndex >= levelsData.Levels.Count)
            {
                currentLevelIndex = 0;
            }

            activeLevel = levelsData.Levels[currentLevelIndex];
            RestartActiveLevel();
        }

        //Camera controls
        if (Input.mouseScrollDelta.y < 0f)
        {
            float currentFOV = this.playerCamera.Lens.OrthographicSize;
            this.playerCamera.Lens.OrthographicSize = Mathf.Clamp(currentFOV + cameraFOVStep, cameraFovRange.x, cameraFovRange.y);
        }
        else if(Input.mouseScrollDelta.y > 0f)
        {
            float currentFOV = this.playerCamera.Lens.OrthographicSize;
            this.playerCamera.Lens.OrthographicSize = Mathf.Clamp(currentFOV - cameraFOVStep, cameraFovRange.x, cameraFovRange.y);
        }
    }

    private void RestartActiveLevel()
    {
        Debug.Log($"Starting Level : {activeLevel.PathId}, " +
            $"Path complexity : {activeLevel.PathData.Complexity}, " +
            $"IsSolvable : {activeLevel.EnvData.IsSolvable}");

        DrawLevel(activeLevel.EnvData);

        if (playerController != null)
        {
            Destroy(playerController.gameObject);
            playerController = null;
        }

        List<Vector2Int> playerPath = activeLevel.EnvData.PlayerPath;
        Vector2Int spawnCell = playerPath[0];
        Vector2Int endCell = playerPath[playerPath.Count - 1];

        Vector2 spawnPosition = tileMap.CellToWorld((Vector3Int)spawnCell);

        Debug.Log("Spawning player..");
        playerController = Instantiate<PlayerController>(playerPrefab, position: spawnPosition, rotation: Quaternion.identity);
        playerCamera.Target.TrackingTarget = playerController.transform;
    }

    private void DrawLevel(EnvData data)
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

                if (tileID == Constants.TILE_ID_PLATFORM)
                {
                    tileMap.SetTile(tileGridPosition, platformTile);
                }
                else if(tileID == Constants.TILE_ID_EMPTY_SPACE)
                {
                    tileMap.SetTile(tileGridPosition, null);
                }
                else
                {
                    Debug.LogError($"Unknown tile id provided : {tileID}");
                }

            }
        }

        print("Level draw complete");
    }
}
