using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    private GameManager _gameManager;
    public GameObject enemyPrefab;
    public GameObject matePrefab;
    public List<GameObject> propsPrefabs;
    public List<float> propsProbability;
    public TextMeshProUGUI levelText;
    private float spawnRange = 9;
    public int level = 0;
    private int waveNumber = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        _gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_gameManager.IsGameActive)
        {
            return;
        }
        if (FindObjectsOfType<Enemy>().Length == 0)
        {
            level += 1;
            waveNumber = level;
            UpdateLevelText();
            RemoveProps();
            SpawnRandomProps(2);
            SpawnEnemyWave(waveNumber);
            if (level >= 8)
            {
                SpawnMates(1);
            }
        }
    }
    
    void SpawnEnemyWave(int enemiesToSpawn)
    {
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Instantiate(enemyPrefab, GenerateSpawnPosition(), enemyPrefab.transform.rotation);
        }
    }

    void SpawnMates(int number)
    {
        for (int i = 0; i < number; i++)
        {
            Instantiate(matePrefab, GenerateSpawnPosition(), matePrefab.transform.rotation);
        }
    }

    void SpawnRandomProps(int number)
    {
        for (int i = 0; i < number; i++)
        {
            SpawnProp(GetRandomProp());
        }
    }

    GameObject GetRandomProp()
    {
        float randomValue = Random.Range(0, 100);
        int index;
        for (index = 0; index < propsProbability.Count; index++)
        {
            randomValue -= propsProbability[index];
            if (randomValue < 0)
            {
                break;
            }
        }
        return propsPrefabs[index];
    }

    void RemoveProps()
    {
        Prop[] props = FindObjectsOfType<Prop>();
        if (props.Length != 0)
        {
            foreach (var prop in props)
            {
                Destroy(prop.gameObject);
            }
        }
    }

    void SpawnProp(GameObject prop)
    {
        Instantiate(prop, GenerateSpawnPosition(), prop.transform.rotation);
    }

    void UpdateLevelText()
    {
        levelText.text = "level: " + level;
    }

    private Vector3 GenerateSpawnPosition()
    {
        float spawnPosX = Random.Range(-spawnRange, spawnRange);
        float spawnPosZ = Random.Range(-spawnRange, spawnRange);
        return new Vector3(spawnPosX, 0, spawnPosZ);
    }
}
