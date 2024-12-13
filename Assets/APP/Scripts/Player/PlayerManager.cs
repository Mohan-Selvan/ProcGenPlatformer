using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerController playerController = null;

    [Header("Debug only")]
    [SerializeField] int coinsCollected = 0;

    internal void Initialize()
    {
        playerController.Initialize();
    }

    internal void Deinitialize()
    {
        playerController.DeInitialize();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag(Constants.TAG_COIN))
        {
            coinsCollected += 1;
            collision.gameObject.SetActive(false);
        }
    }
}
