using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Scripts.PickUps
{
    public class PickupSpawner : MonoBehaviour
    {
        [SerializeField] private Terrain terrain;
        [SerializeField] private GameObject[] pickupPrefabs;
        [SerializeField] private int[] pickupCounts;
        [SerializeField] private float minSpawnDistance = 2f;
        [SerializeField] private float heightOffset = 1f;
        [SerializeField] private float maxPickupHeight = 10f;
        [SerializeField] private Transform pickupParent;

        private readonly List<Vector3> _spawnedPositions = new();

        private void Start()
        {
            SpawnPickups();
        }

        private void SpawnPickups()
        {
            if (pickupCounts.Length != pickupPrefabs.Length)
            {
                Debug.LogError("Mismatch between pickup prefabs and pickup counts!");
                return;
            }

            var tPosition = terrain.transform.position;
            var tWidth = terrain.terrainData.size.x;
            var tLength = terrain.terrainData.size.z;
            var tHeight = terrain.terrainData.size.y;

            for (var i = 0; i < pickupPrefabs.Length; i++)
            {
                var spawned = 0;
                var attempts = 0;
                while (spawned < pickupCounts[i] && attempts < pickupCounts[i] * 5)
                {
                    var randomPosition = GetRandomPosition(tPosition, tWidth, tLength, tHeight);

                    // Ensure the pickup height does not exceed maxPickupHeight
                    if (randomPosition.y - tPosition.y > maxPickupHeight)
                    {
                        attempts++;
                        continue; // Skip this spawn and try again
                    }

                    if (IsValidSpawnPosition(randomPosition))
                    {
                        var pickup = Instantiate(pickupPrefabs[i], randomPosition + Vector3.up * heightOffset,
                            Quaternion.identity);

                        pickup.transform.SetParent(pickupParent, true);
                        _spawnedPositions.Add(randomPosition);
                        spawned++;
                    }

                    attempts++;
                }
            }
        }

        private Vector3 GetRandomPosition(Vector3 tPosition, float tWidth, float tLength, float tHeight)
        {
            var randomX = Random.Range(tPosition.x, tPosition.x + tWidth);
            var randomZ = Random.Range(tPosition.z, tPosition.z + tLength);

            var rayOrigin = new Vector3(randomX, tPosition.y + tHeight + 10f, randomZ);
            var ray = new Ray(rayOrigin, Vector3.down);

            if (Physics.Raycast(ray, out var hit, tHeight + 20f))
            {
                var clampedHeight = Mathf.Min(hit.point.y, tPosition.y + maxPickupHeight); // Cap the height
                return new Vector3(randomX, clampedHeight + heightOffset, randomZ); // Adjusted height
            }

            var terrainY = terrain.SampleHeight(new Vector3(randomX, 0, randomZ)) + tPosition.y;
            var clampedTerrainY = Mathf.Min(terrainY, tPosition.y + maxPickupHeight); // Cap the height
            return new Vector3(randomX, clampedTerrainY + heightOffset, randomZ);
        }

        private bool IsValidSpawnPosition(Vector3 position)
        {
            return _spawnedPositions.All(spawnPos => !(Vector3.Distance(spawnPos, position) < minSpawnDistance));
        }
    }
}