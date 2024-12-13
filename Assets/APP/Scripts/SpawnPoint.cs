using System.Collections;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Collider2D spawnPointCollider = null;

    [Header("References - Visuals")]
    [SerializeField] ParticleSystem chargeParticles = null;
    [SerializeField] ParticleSystem burstParticles = null;
    [SerializeField] SpriteRenderer spriteRenderer = null;

    [Header("Settings- Visuals")]
    [SerializeField] Vector2 flashRange = Vector2.up;
    [SerializeField] float flashDuration = 1.0f;

    internal void Initialize()
    {
        spawnPointCollider.enabled = true;

        Debug.Log($"Initialized : {nameof(SpawnPoint)}");
    }

    internal void Deinitialize()
    {
        spawnPointCollider.enabled = false;

        Debug.Log($"Deinitialized : {nameof(SpawnPoint)}");
    }

    internal IEnumerator SpawnPlayer_Routine(System.Action spawnPlayerAction)
    {
        chargeParticles.Play();
        yield return new WaitForSeconds(chargeParticles.main.duration * 1.2f);

        burstParticles.Play();

        spriteRenderer.color = Helper.GetColorWithAlpha(spriteRenderer.color, 1.0f);
        spawnPlayerAction?.Invoke();

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
}
