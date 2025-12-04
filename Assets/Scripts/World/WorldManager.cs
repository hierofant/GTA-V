using System.Collections.Generic;
using UnityEngine;

namespace Sandbox.World
{
    /// <summary>
    /// Oversees day/night cycle, streaming chunks and pooled objects.
    /// </summary>
    public class WorldManager : MonoBehaviour
    {
        [Header("Time of Day")]
        [SerializeField] private Light directionalLight;
        [SerializeField] private float dayLengthSeconds = 600f;

        [Header("Streaming")]
        [SerializeField] private Transform player;
        [SerializeField] private float chunkSize = 100f;
        [SerializeField] private List<WorldChunk> chunks = new();

        private float timeAccumulator;

        private void Update()
        {
            UpdateLighting();
            UpdateStreaming();
        }

        private void UpdateLighting()
        {
            if (directionalLight == null)
            {
                return;
            }

            timeAccumulator += Time.deltaTime / dayLengthSeconds;
            float cycle = timeAccumulator % 1f;
            directionalLight.transform.rotation = Quaternion.Euler(new Vector3(cycle * 360f - 90f, 170f, 0f));
            directionalLight.intensity = Mathf.Clamp01(Mathf.Sin(cycle * Mathf.PI));
        }

        private void UpdateStreaming()
        {
            if (player == null)
            {
                return;
            }

            Vector2 playerChunk = new Vector2(Mathf.Floor(player.position.x / chunkSize), Mathf.Floor(player.position.z / chunkSize));
            foreach (WorldChunk chunk in chunks)
            {
                if (chunk == null)
                {
                    continue;
                }

                Vector2 chunkCoord = new Vector2(Mathf.Floor(chunk.transform.position.x / chunkSize), Mathf.Floor(chunk.transform.position.z / chunkSize));
                float distance = Vector2.Distance(playerChunk, chunkCoord);
                bool shouldBeActive = distance <= 1.5f;
                if (chunk.gameObject.activeSelf != shouldBeActive)
                {
                    chunk.gameObject.SetActive(shouldBeActive);
                }
            }
        }
    }
}
