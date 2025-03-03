using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Managers
{
    public class PickupSpawner : MonoBehaviour
    {
        [SerializeField] private Terrain terrain; // Assign your terrain in the Inspector
        [SerializeField] private GameObject[] pickupPrefabs; // Array of different pickup prefabs
        [SerializeField] private int pickupCount = 20; // Number of pickups to spawn
        [SerializeField] private float minSpawnDistance = 2f; // Minimum distance between pickups
        [SerializeField] private float pickupHeightOffset = 0.5f; // Offset to ensure pickups do not spawn under terrain

        private readonly List<Vector3> _spawnedPositions = new();

        private void Start()
        {
            SpawnPickups();
        }

        private void SpawnPickups()
        {
            int attempts = 0;
            int spawned = 0;

            Vector3 terrainPosition = terrain.transform.position;
            float terrainWidth = terrain.terrainData.size.x;
            float terrainLength = terrain.terrainData.size.z;
            float terrainHeight = terrain.terrainData.size.y;

            while (spawned < pickupCount && attempts < pickupCount * 5)
            {
                Vector3 randomPosition =
                    GetRandomPositionOnTerrain(terrainPosition, terrainWidth, terrainLength, terrainHeight);
                if (IsValidSpawnPosition(randomPosition))
                {
                    GameObject randomPickup = pickupPrefabs[Random.Range(0, pickupPrefabs.Length)];
                    Instantiate(randomPickup, randomPosition + Vector3.up * pickupHeightOffset, Quaternion.identity);
                    _spawnedPositions.Add(randomPosition);
                    spawned++;
                }

                attempts++;
            }
        }

        private Vector3 GetRandomPositionOnTerrain(Vector3 terrainPosition, float terrainWidth, float terrainLength,
            float terrainHeight)
        {
            float randomX = Random.Range(terrainPosition.x, terrainPosition.x + terrainWidth);
            float randomZ = Random.Range(terrainPosition.z, terrainPosition.z + terrainLength);

            Vector3 rayOrigin = new Vector3(randomX, terrainPosition.y + terrainHeight + 10f, randomZ);
            Ray ray = new Ray(rayOrigin, Vector3.down);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, terrainHeight + 20f))
            {
                return hit.point + Vector3.up * pickupHeightOffset; // Ensure pickup is slightly above terrain
            }

            float terrainY = terrain.SampleHeight(new Vector3(randomX, 0, randomZ)) + terrainPosition.y;
            return new Vector3(randomX, terrainY + pickupHeightOffset, randomZ);
        }

        private bool IsValidSpawnPosition(Vector3 position)
        {
            foreach (var spawnPos in _spawnedPositions)
            {
                if (Vector3.Distance(spawnPos, position) < minSpawnDistance)
                {
                    return false; // Too close to an existing pickup
                }
            }

            return true;
        }
    }
}