using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    private GameManager _gameManager;
    private Rigidbody playerRb;
    public float jumpForce = 7.0f;
    public float speed = 5.0f;
    public float powerupForce = 15.0f;

    private float _jumpForce;
    private float _speed;
    private float _powerupForce;
    public GameObject powerupIndicator;
    public GameObject superstarIndicator;
    
    private GameObject focalPoint;
    
    private bool hasPowerup;
    private IEnumerator powerupCountDown = null;

    private bool hasSuperstar;
    private IEnumerator superstarCountDown = null;
    
    private bool hasBigger;
    private bool hasSmaller;
    private IEnumerator biggerCountDown = null;

    private IEnumerator siegeCountDown = null;
    
    // Start is called before the first frame update
    void Start()
    {
        _gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        _jumpForce = jumpForce;
        _speed = speed;
        _powerupForce = powerupForce;
        playerRb = GetComponent<Rigidbody>();
        focalPoint = GameObject.Find("FocalPoint");
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < -10.0f)
        {
            _gameManager.GameOver();
            Destroy(gameObject);
            Destroy(powerupIndicator.gameObject);
            Destroy(superstarIndicator.gameObject);
            return;
        }

        float yOffset = hasBigger ? -1.5f : (hasSmaller ? -0.5f / 3.0f : -0.5f);
        powerupIndicator.transform.position = transform.position + new Vector3(0, yOffset, 0);
        powerupIndicator.transform.Rotate(Vector3.up, 2.0f);
        superstarIndicator.transform.position = transform.position + new Vector3(0, 0, 0);
        superstarIndicator.transform.Rotate(Vector3.up, 2.0f);
        
        if (!_gameManager.IsGameActive)
        {
            return;
        }
        
        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");
        playerRb.AddForce(focalPoint.transform.forward * _speed * verticalInput);
        playerRb.AddForce(focalPoint.transform.right * _speed * horizontalInput);

        if (hasSuperstar && Input.GetKeyDown(KeyCode.Space))
        {
            Ray ray = new Ray(transform.position, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                GameObject hitGameObject = hit.collider.gameObject;
                if (hit.distance < 0.5f * transform.localScale.y + 0.01f &&
                    hitGameObject.CompareTag("Ground") || 
                    hitGameObject.CompareTag("Obstacle") || 
                    hitGameObject.CompareTag("Enemy") ||
                    hitGameObject.CompareTag("Mate"))
                {
                    playerRb.AddForce(Vector3.up * _jumpForce * (hasBigger ? 5 : 1), ForceMode.Impulse);
                }
            }
        }

        // 围剿判断
        if (siegeCountDown != null && playerRb.velocity.magnitude > 0.1f)
        {
            StopCoroutine(siegeCountDown);
            siegeCountDown = null;
        }
        if (siegeCountDown == null && playerRb.velocity.magnitude < 0.1f)
        {
            siegeCountDown = SiegeCountDownRoutine();
            StartCoroutine(siegeCountDown);
        }
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
                superstarIndicator.transform.localScale = new Vector3(9, 3, 9);
                playerRb.mass = 5;
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
                superstarIndicator.transform.localScale = new Vector3(1, 1/3.0f, 1);
                playerRb.mass = 1;
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
        else if (other.CompareTag("Superstar"))
        {
            hasSuperstar = true;
            superstarIndicator.gameObject.SetActive(true);
            Destroy(other.gameObject);
            
            if (superstarCountDown != null)
            {
                StopCoroutine(superstarCountDown);
            }
            superstarCountDown = SuperstarCountdownRoutine();
            StartCoroutine(superstarCountDown);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if ((other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Mate")) && hasPowerup)
        {
            Rigidbody enemyRigidbody = other.gameObject.GetComponent<Rigidbody>();
            Vector3 awayFromPlayer = other.gameObject.transform.position - transform.position;
            
            enemyRigidbody.AddForce(awayFromPlayer * _powerupForce, ForceMode.Impulse);
        }
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
        
        Mate[] mates = GameObject.FindObjectsOfType<Mate>();
        int aroundMatesCount = 0;
        foreach (var mate in mates)
        {
            if (Vector3.Distance(transform.position, mate.transform.position) < 2)
            {
                aroundMatesCount += 1;
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

        if (aroundEnemyCount + aroundMatesCount + aroundBossesCount >= 5)
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
            _gameManager.GameOver();
        }
        else
        {
            siegeCountDown = null;
        }
    }

    IEnumerator PowerupCountdownRoutine()
    {
        yield return new WaitForSeconds(7);
        hasPowerup = false;
        powerupIndicator.gameObject.SetActive(false);
    }
    
    IEnumerator SuperstarCountdownRoutine()
    {
        yield return new WaitForSeconds(7);
        hasSuperstar = false;
        superstarIndicator.gameObject.SetActive(false);
    }
    
    IEnumerator BiggerCountdownRoutine()
    {
        yield return new WaitForSeconds(7);
        hasBigger = false;
        hasSmaller = false;
        transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        powerupIndicator.transform.localScale = new Vector3(3, 1, 3);
        superstarIndicator.transform.localScale = new Vector3(3, 1, 3);
        playerRb.mass = 1;
        _speed = speed;
    }
}
