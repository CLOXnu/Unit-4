using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;
using Random = UnityEngine.Random;

public class Boss : MonoBehaviour
{
    private GameManager _gameManager;
    private SpawnManager _spawnManager;

    public float mass = 30.0f;
    public float speed = 60.0f;
    private float _speed;
    
    private GameObject player;
    private Rigidbody bossRb;
    
    private IEnumerator skillCountDown = null;

    // Start is called before the first frame update
    void Start()
    {
        _gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        _spawnManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
        _speed = speed;
        bossRb = GetComponent<Rigidbody>();
        bossRb.mass = mass;
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < -10.0f)
        {
            Destroy(gameObject);
            return;
        }
        
        if (!_gameManager.IsGameActive)
        {
            return;
        }
        
        Vector3 lookDirection = (player.transform.position - transform.position).normalized;
        bossRb.AddForce(lookDirection * _speed);

        if (FindObjectsOfType<Enemy>().Length == 0 && skillCountDown == null)
        {
            skillCountDown = SkillCountDownRoutine();
            StartCoroutine(skillCountDown);
        }
    }

    private void OnDestroy()
    {
        if (skillCountDown != null)
        {
            StopCoroutine(skillCountDown);
        }
    }

    IEnumerator SkillCountDownRoutine()
    {
        yield return new WaitForSeconds(3);
        int skill_id = Rand.Choice<int>(new[] {1, 2, 3}, new float[] {60, 30, 10});
        PerformSkill(skill_id, Random.Range(10, 20));
        skillCountDown = null;
    }

    void PerformSkill(int skill_id, int power)
    {
        switch (skill_id)
        {
            case 1:
                _spawnManager.SpawnEnemies(power, false);
                _spawnManager.SpawnRandomProps(2);
                _spawnManager.SpawnMates(2);
                break;
            case 2:
                _spawnManager.SpawnEnemies(power, false, false, true, false);
                _spawnManager.SpawnRandomProps(2);
                _spawnManager.SpawnMates(2);
                break;
            case 3:
                _spawnManager.SpawnEnemies(power, false, false, false, true);
                _spawnManager.SpawnRandomProps(2);
                _spawnManager.SpawnMates(2);
                break;
        }

        float scale = transform.localScale.x;
        if (scale > 1.6f)
        {
            transform.localScale = new Vector3(scale - 0.4f, scale - 0.4f, scale - 0.4f);
        }
    }
}
