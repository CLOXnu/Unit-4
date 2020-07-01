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
    public GameObject bossPrefab;
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
        if (FindObjectsOfType<Enemy>().Length + FindObjectsOfType<Boss>().Length == 0)
        {
            level += 1;
            waveNumber = level;
            UpdateLevelText();

            if (level == 20)
            {
                Boss boss = SpawnBoss();
                boss.mass = 30.0f;
                boss.speed = 60.0f;
            }
            else if (level == 30)
            {
                Boss boss = SpawnBoss();
                boss.mass = 120.0f;
                boss.speed = 500.0f;
            }
            else if (level == 40)
            {
                Boss boss = SpawnBoss();
                boss.mass = 300.0f;
                boss.speed = 2400.0f;
            }
            else
            {
                RemoveProps();
                SpawnRandomProps(2);
                SpawnEnemyWave(waveNumber);
                if (level >= 8)
                {
                    SpawnMates(1);
                }
            }
        }
    }
    
    public void SpawnEnemyWave(int enemiesToSpawn)
    {
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Instantiate(enemyPrefab, GenerateSpawnPosition(), enemyPrefab.transform.rotation);
        }
    }
    
    public void SpawnEnemies(int enemiesToSpawn, bool isOnTheGround=true,
        bool hasBigger=false, bool hasSmaller=false, bool hasPowerup=false)
    {
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Enemy enemy = Instantiate(enemyPrefab, GenerateSpawnPosition(isOnTheGround), enemyPrefab.transform.rotation).GetComponent<Enemy>();
            enemy.biggerTrig = hasBigger;
            enemy.smallerTrig = hasSmaller;
            enemy.powerupTrig = hasPowerup;
        }
    }

    public void SpawnMates(int number)
    {
        for (int i = 0; i < number; i++)
        {
            Instantiate(matePrefab, GenerateSpawnPosition(), matePrefab.transform.rotation);
        }
    }

    public void SpawnRandomProps(int number)
    {
        for (int i = 0; i < number; i++)
        {
            SpawnProp(GetRandomProp());
        }
    }

    public Boss SpawnBoss()
    {
        return Instantiate(bossPrefab, GenerateSpawnPosition(false), bossPrefab.transform.rotation).GetComponent<Boss>();
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

    private Vector3 GenerateSpawnPosition(bool isOnTheGround=true)
    {
        float spawnPosX = Random.Range(-spawnRange, spawnRange);
        float spawnPosZ = Random.Range(-spawnRange, spawnRange);
        float spawnPosY = isOnTheGround ? 0 : 10;
        return new Vector3(spawnPosX, spawnPosY, spawnPosZ);
    }
}
