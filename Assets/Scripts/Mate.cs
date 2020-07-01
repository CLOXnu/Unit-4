using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mate : MonoBehaviour
{
    private GameManager _gameManager;
    public float speed = 5.0f;
    public float powerupForce = 15.0f;

    private float _speed;
    private float _powerupForce;
    public GameObject powerupIndicatorPrefab;
    private GameObject powerupIndicator;
    private Rigidbody mateRb;
    private GameObject player;
    
    private bool hasPowerup;
    private IEnumerator powerupCountDown = null;
    
    private bool hasBigger;
    private bool hasSmaller;
    private IEnumerator biggerCountDown = null;

    private GameObject focusOnEnemy = null;
    private IEnumerator siegeCountDown = null;
    
    // Start is called before the first frame update
    void Start()
    {
        _gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        powerupIndicator = Instantiate(powerupIndicatorPrefab, transform.position + new Vector3(0, -0.5f, 0),
            powerupIndicatorPrefab.transform.rotation);
        _speed = speed;
        _powerupForce = powerupForce;
        mateRb = GetComponent<Rigidbody>();
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
        
        powerupIndicator.transform.position = transform.position + new Vector3(0, (hasBigger ? -1.5f : -0.5f), 0);
        powerupIndicator.transform.Rotate(Vector3.up, 2.0f);
        
        if (!_gameManager.IsGameActive)
        {
            return;
        }

        if (focusOnEnemy != null)
        {
            Ray ray = new Ray(focusOnEnemy.transform.position, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (!hit.collider.gameObject.CompareTag("Ground"))
                {
                    focusOnEnemy = null;
                }
            }
            else
            {
                focusOnEnemy = null;
            }
        }
        if (focusOnEnemy == null)
        {
            focusOnEnemy = FindCloesetEnemy();
        }

        if (focusOnEnemy != null)
        {
            Vector3 lookDirection = (focusOnEnemy.transform.position - transform.position).normalized;
            mateRb.AddForce(lookDirection * _speed);
        }
        else
        {
            Vector3 lookDirection = (new Vector3(0, 0, 0) - transform.position).normalized;
            mateRb.AddForce(lookDirection * _speed);
        }
        
        // 围剿判断
        if (siegeCountDown != null && mateRb.velocity.magnitude > 0.1f)
        {
            StopCoroutine(siegeCountDown);
            siegeCountDown = null;
        }
        if (siegeCountDown == null && mateRb.velocity.magnitude < 0.1f)
        {
            siegeCountDown = SiegeCountDownRoutine();
            StartCoroutine(siegeCountDown);
        }
        
    }


    GameObject FindCloesetEnemy()
    {
        Enemy[] enemies = GameObject.FindObjectsOfType<Enemy>();
            
        GameObject closestEnemy = null;
        float closestDistance = 0;
        foreach (var enemy in enemies)
        {
            float distance = Vector3.Distance(enemy.gameObject.transform.position, transform.position);
            if (closestDistance == 0 || distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy.gameObject;
            }
        }

        return closestEnemy;
    }

    private void OnDestroy()
    {
        Destroy(powerupIndicator);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Powerup"))
        {
            hasPowerup = true;
            powerupIndicator.gameObject.SetActive(true);
            Destroy(other.gameObject);

            if (powerupCountDown != null)
            {
                StopCoroutine(powerupCountDown);
            }
            powerupCountDown = PowerupCountdownRoutine();
            StartCoroutine(powerupCountDown);
        }
        else if (other.CompareTag("Bigger"))
        {
            if (!hasBigger)
            {
                transform.localScale = new Vector3(4.5f, 4.5f, 4.5f);
                transform.position = new Vector3(transform.position.x, 0, transform.position.z);
                powerupIndicator.transform.localScale = new Vector3(9, 3, 9);
                mateRb.mass = 5;
                _speed = speed * 5;
            }

            hasSmaller = false;
            hasBigger = true;
            
            Destroy(other.gameObject);
            
            if (biggerCountDown != null)
            {
                StopCoroutine(biggerCountDown);
            }
            biggerCountDown = BiggerCountdownRoutine();
            StartCoroutine(biggerCountDown);
        }
        else if (other.CompareTag("Smaller"))
        {
            if (!hasSmaller)
            {
                transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                powerupIndicator.transform.localScale = new Vector3(1, 1/3.0f, 1);
                mateRb.mass = 1;
                _speed = speed * 2;
            }

            hasSmaller = true;
            hasBigger = false;
            
            Destroy(other.gameObject);
            
            if (biggerCountDown != null)
            {
                StopCoroutine(biggerCountDown);
            }
            biggerCountDown = BiggerCountdownRoutine();
            StartCoroutine(biggerCountDown);
        }
    }
    
    private void OnCollisionEnter(Collision other)
    {
        if ((other.gameObject.CompareTag("Enemy") || 
             other.gameObject.CompareTag("Player") ||
             other.gameObject.CompareTag("Mate")) && hasPowerup)
        {
            Rigidbody enemyRigidbody = other.gameObject.GetComponent<Rigidbody>();
            Vector3 awayFromPlayer = other.gameObject.transform.position - transform.position;
            
            enemyRigidbody.AddForce(awayFromPlayer * _powerupForce, ForceMode.Impulse);
        }
    }
    
    IEnumerator PowerupCountdownRoutine()
    {
        yield return new WaitForSeconds(5);
        hasPowerup = false;
        powerupIndicator.gameObject.SetActive(false);
    }
    
    bool IsManyEnemyAround()
    {
        Enemy[] enemys = GameObject.FindObjectsOfType<Enemy>();
        int aroundEnemyCount = 0;
        foreach (var enemy in enemys)
        {
            if (Vector3.Distance(transform.position, enemy.transform.position) < 2)
            {
                aroundEnemyCount += 1;
            }
        }
        
        Boss[] bosses = GameObject.FindObjectsOfType<Boss>();
        int aroundBossesCount = 0;
        foreach (var boss in bosses)
        {
            if (Vector3.Distance(transform.position, boss.transform.position) < 2)
            {
                aroundBossesCount += 1;
            }
        }

        if (Vector3.Distance(transform.position, player.transform.position) < 2)
        {
            aroundEnemyCount += 1;
        }

        if (aroundEnemyCount + aroundBossesCount >= 5)
        {
            return true;
        }

        return false;
    }
    
    IEnumerator SiegeCountDownRoutine()
    {
        yield return new WaitForSeconds(3);
        if (IsManyEnemyAround())
        {
            Destroy(gameObject);
        }
        else
        {
            siegeCountDown = null;
        }
    }
    
    IEnumerator BiggerCountdownRoutine()
    {
        yield return new WaitForSeconds(7);
        hasBigger = false;
        transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        powerupIndicator.transform.localScale = new Vector3(3, 1, 3);
        mateRb.mass = 1;
        _speed = speed;
    }
}
