using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Cinemachine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] LevelsDataLoader levelsDataLoader = null;
    [SerializeField] Tilemap tileMap;
    [SerializeField] PlayerManager playerManager = null;
    [SerializeField] CinemachineCamera playerCamera = null;
    [SerializeField] SpawnPoint spawnPoint = null;
    [SerializeField] GoalPoint goalPoint = null;

    [Header("References - Prefab")]
    [SerializeField] GameObject coinPrefab = null;

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

    [Header("Collectibles settings")]
    [SerializeField] int maxCoins = 1;

    [Header("Settings - Debug only")]
    [SerializeField] LevelsData levelsData = null;


    [Space(10)]
    [SerializeField] int currentLevelIndex = 0;
    [SerializeField] LevelData activeLevel = null;

    [Space(10)]
    [SerializeField] List<GameObject> coinsList = new List<GameObject>();

    private Coroutine _activeRoutine = null;

    private void Start()
    {
        playerManager.Initialize();
        playerManager.Deinitialize();

        spawnPoint.Initialize();
        goalPoint.Initialize(
            () => {
                if (_activeRoutine != null)
                {
                    StopCoroutine(_activeRoutine);
                    _activeRoutine = null;
                }

                _activeRoutine = StartCoroutine(HandleLevelEnd_Routine());
            }    
        );

        spawnPoint.gameObject.SetActive(false);
        goalPoint.gameObject.SetActive(false);

        coinsList = new List<GameObject>();
        for(int i = 0; i < maxCoins; i++)
        {
            GameObject coin = Instantiate<GameObject>(coinPrefab, this.transform.position, Quaternion.identity);
            coinsList.Add(coin);
            coin.gameObject.SetActive(false);
        }
    }

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
        if(CanRunRoutine())
        {
            _activeRoutine = StartCoroutine(RestartActiveLevel_Routine());
        }
        else
        {
            Debug.Log($"Routine cannot run, blocked by : {_activeRoutine}");
        }
    }

    private IEnumerator RestartActiveLevel_Routine()
    {
        Debug.Log($"Start Routine : {nameof(RestartActiveLevel_Routine)}");

        Debug.Log($"Starting Level : {activeLevel.PathId}, " +
        $"Path complexity : {activeLevel.PathData.Complexity}, " +
        $"IsSolvable : {activeLevel.EnvData.IsSolvable}");

        DrawLevel(activeLevel.EnvData);

        playerManager.Deinitialize();

        Debug.Log("Spawning player..");
        playerCamera.Target.TrackingTarget = spawnPoint.transform;
        yield return spawnPoint.SpawnPlayer_Routine(() =>
        {
            playerCamera.Target.TrackingTarget = playerManager.transform;
            playerManager.transform.position = spawnPoint.transform.position;
            playerManager.Initialize();
        });

        _activeRoutine = null;
        yield return null;

        Debug.Log($"Stop Routine : {nameof(RestartActiveLevel_Routine)}");
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

        ////////////Spawn and Goal point//////////////

        Vector2Int spawnPointCoord = data.PlayerPath[0];
        Vector2 spawnPointPosition = tileMap.CellToWorld((Vector3Int)spawnPointCoord) + (tileMap.cellSize * 0.5f);
        spawnPoint.transform.position = spawnPointPosition;
        spawnPoint.gameObject.SetActive(true);

        Vector2Int goalPointCoord = data.PlayerPath[data.PlayerPath.Count - 1];
        Vector2 goalPointPosition = tileMap.CellToWorld((Vector3Int)goalPointCoord) + (tileMap.cellSize * 0.5f);
        goalPoint.transform.position = goalPointPosition;
        goalPoint.gameObject.SetActive(true);



        ///////////////Placing coins//////////////////
        
        int numberOfCoins = Mathf.Clamp(data.ReachableCells.Count, 0, maxCoins);

        System.Collections.Generic.List<int> randomIndices = new List<int>();
        while(randomIndices.Count < numberOfCoins)
        {
            int index = Random.Range(0, data.ReachableCells.Count - 1);
            if (!randomIndices.Contains(index))
            {
                randomIndices.Add(index);
            }

        }

        for (int i = 0; i < coinsList.Count; i++)
        {
            GameObject coin = coinsList[i];

            if(i < (numberOfCoins - 1))
            {
                Vector2Int tileCoord = data.ReachableCells[randomIndices[i]];
                Vector2 coinPosition = tileMap.CellToWorld((Vector3Int)tileCoord) + (tileMap.cellSize * 0.5f);
                coin.transform.position = coinPosition;
                coin.gameObject.SetActive(true);
            }
            else
            {
                coin.gameObject.SetActive(false);
            }            
        }

        print("Level draw complete");
    }

    private bool CanRunRoutine()
    {
        return _activeRoutine == null;
    }

    private IEnumerator HandleLevelEnd_Routine()
    {
        yield return goalPoint.ReceivePlayer_Routine(() =>
        {
            this.playerManager.Deinitialize();
        });

        _activeRoutine = null;
    }
}
