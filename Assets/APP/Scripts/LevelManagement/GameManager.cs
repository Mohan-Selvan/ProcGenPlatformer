using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Cinemachine;
using System.Collections;
using TMPro;

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

    //Events
    public event System.Action<LevelData> OnActiveLevelChanged = null;
    public event System.Action OnPlayerSpawned = null;
    public event System.Action OnPlayerDespawned = null;

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
            SetActiveLevel(0);
            Debug.Log("Loaded levels data");
        }

        if (Input.GetKeyDown(startGameKey))
        {
            PlayActiveLevel();
        }

        if (Input.GetKeyDown(previousLevelKey))
        {
            SwitchLevelPrevious();
        }

        if (Input.GetKeyDown(nextLevelKey))
        {
            SwitchLevelNext();
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

    public void PlayActiveLevel()
    {
        if(CanRunRoutine())
        {
            if(activeLevel.EnvData.IsSolvable)
            {
                _activeRoutine = StartCoroutine(PlayActiveLevel_Routine());
            }
            else
            {
                Debug.Log($"Level : {activeLevel.PathId} cannot be played.");
            }

        }
        else
        {
            Debug.Log($"Routine cannot run, blocked by : {_activeRoutine}");
        }
    }

    public void ExitActiveLevel()
    {
        if (CanRunRoutine())
        {
            _activeRoutine = StartCoroutine(HandleLevelEnd_Routine());
        }
        else
        {
            Debug.Log($"Routine cannot run, blocked by : {_activeRoutine}");
        }
    }

    private IEnumerator PlayActiveLevel_Routine()
    {
        Debug.Log($"Start Routine : {nameof(PlayActiveLevel_Routine)}");

        Debug.Log($"Starting Level : {activeLevel.PathId}, " +
        $"Path complexity : {activeLevel.PathData.Complexity}, " +
        $"IsSolvable : {activeLevel.EnvData.IsSolvable}");

        OnPlayerSpawned?.Invoke();

        DrawLevel(activeLevel.EnvData);

        playerManager.Deinitialize();

        Debug.Log("Spawning player..");
        playerCamera.Target.TrackingTarget = spawnPoint.transform;
        playerCamera.Prioritize();

        yield return spawnPoint.SpawnPlayer_Routine(() =>
        {
            playerCamera.Target.TrackingTarget = playerManager.transform;
            playerManager.transform.position = spawnPoint.transform.position;
            playerManager.Initialize();
        });

        _activeRoutine = null;
        yield return null;

        Debug.Log($"Stop Routine : {nameof(PlayActiveLevel_Routine)}");
    }

    public void SwitchLevelPrevious()
    {
        SetActiveLevel(currentLevelIndex - 1);
    }

    public void SwitchLevelNext()
    {
        SetActiveLevel(currentLevelIndex + 1);
    }

    private void SetActiveLevel(int index)
    {
        if(index >= (levelsData.Levels.Count))
        {
            index = 0;
        }
        else if(index < 0)
        {
            index = levelsData.Levels.Count - 1;
        }

        currentLevelIndex = index;
        activeLevel = levelsData.Levels[currentLevelIndex];

        DrawLevel(activeLevel.EnvData);
        playerCamera.Target.TrackingTarget = spawnPoint.transform;

        OnActiveLevelChanged?.Invoke(activeLevel);
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
            int index = Random.Range(0, data.ReachableCells.Count);
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

        OnPlayerDespawned?.Invoke();

        _activeRoutine = null;
    }
}
