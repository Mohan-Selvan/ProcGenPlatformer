using System.Collections;
using UnityEngine;

public class GoalPoint : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Collider2D goalPointCollider = null;

    [Header("References - Visuals")]
    [SerializeField] ParticleSystem burstParticles = null;
    [SerializeField] SpriteRenderer spriteRenderer = null;

    [Header("Settings- Visuals")]
    [SerializeField] Vector2 flashRange = Vector2.up;
    [SerializeField] float flashDuration = 1.0f;

    private System.Action _playerReachGoalCallback = null;

    internal void Initialize(System.Action receiveAction)
    {
        this._playerReachGoalCallback = receiveAction;
        goalPointCollider.enabled = true;

        Debug.Log($"Initialized : {nameof(GoalPoint)}");
    }

    internal void Deinitialize()
    {
        goalPointCollider.enabled = false;

        Debug.Log($"Deinitialized : {nameof(GoalPoint)}");
    }

    internal IEnumerator ReceivePlayer_Routine(System.Action playerReachGoalCallback)
    {
        burstParticles.Play();

        spriteRenderer.color = Helper.GetColorWithAlpha(spriteRenderer.color, 1.0f);
        playerReachGoalCallback?.Invoke();

        float progress = 1f;
        while (progress > 0f)
        {
            progress -= (Time.deltaTime / flashDuration);
            spriteRenderer.color = Helper.GetColorWithAlpha(spriteRenderer.color, Mathf.Lerp(flashRange.x, flashRange.y, progress));

            yield return null;
        }

        spriteRenderer.color = Helper.GetColorWithAlpha(spriteRenderer.color, flashRange.x);

        yield return null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag(Constants.TAG_PLAYER))
        {
            this._playerReachGoalCallback?.Invoke();
        }
    }
}
