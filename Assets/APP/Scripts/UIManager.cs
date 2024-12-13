using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Cinemachine;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameManager gameManager = null;
    [SerializeField] CinemachineCamera levelViewCamera = null;

    [Header("References - UI")]
    [SerializeField] private GameObject gameplayUIWindow = null;
    [SerializeField] private Button exitButton = null;

    [Space(5)]
    [SerializeField] private GameObject levelSelectionWindow = null;
    [SerializeField] private TMP_Text levelDetailsText = null;
    [SerializeField] private Button playButton = null;

    private void Start()
    {
        gameManager.OnActiveLevelChanged += GameManager_OnActiveLevelChanged;
        gameManager.OnPlayerSpawned += GameManager_OnPlayerSpawned;
        gameManager.OnPlayerDespawned += GameManager_OnPlayerDespawned;

        playButton.onClick.AddListener(() =>
        {
            gameManager.PlayActiveLevel();
        });

        exitButton.onClick.AddListener(() =>
        {
            gameManager.ExitActiveLevel();
        });

        gameplayUIWindow.SetActive(false);
        levelSelectionWindow.SetActive(true);

        levelViewCamera.Prioritize();
    }

    private void GameManager_OnPlayerSpawned()
    {
        levelSelectionWindow.SetActive(false);
        gameplayUIWindow.SetActive(true);
    }

    private void GameManager_OnPlayerDespawned()
    {
        levelSelectionWindow.SetActive(true);
        gameplayUIWindow.SetActive(false);
        levelViewCamera.Prioritize();
    }


    private void OnDestroy()
    {
        gameManager.OnActiveLevelChanged -= GameManager_OnActiveLevelChanged;
        gameManager.OnPlayerSpawned -= GameManager_OnPlayerSpawned;
        gameManager.OnPlayerDespawned -= GameManager_OnPlayerDespawned;
    }

    private void GameManager_OnActiveLevelChanged(LevelData data)
    {
        if(data == null)
        {
            playButton.interactable = false;
            levelDetailsText.text = $"Sorry! There was an issue, Please reach out to '{Constants.SUPPORT_MAIL_ID}'";
            return;
        }

        levelDetailsText.text = $"LEVEL DETAILS" +
            $"\nLevel ID : {data.PathId}" +
            $"\nIs Solvable : {data.EnvData.IsSolvable}";

        playButton.interactable = data.EnvData.IsSolvable;
    }
}
