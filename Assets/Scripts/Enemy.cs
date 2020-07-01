using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class Enemy : MonoBehaviour
{
    private GameManager _gameManager;
    public float speed = 3.0f;
    public float powerupForce = 10.0f;

    private float _speed;
    private float _powerupForce;
    public GameObject powerupIndicatorPrefab;
    private GameObject powerupIndicator;
    private Rigidbody enemyRb;
    private GameObject player;
    
    private bool hasPowerup;
    private IEnumerator powerupCountDown = null;
    
    private bool hasBigger;
    private bool hasSmaller;
    private IEnumerator biggerCountDown = null;
    
    private GameObject focusOnPlayer = null;

    public bool biggerTrig;
    public bool smallerTrig;
    public bool powerupTrig;
    
    // Start is called before the first frame update
    void Start()
    {
        _gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        powerupIndicator = Instantiate(powerupIndicatorPrefab, transform.position + new Vector3(0, -0.5f, 0),
            powerupIndicatorPrefab.transform.rotation);
        _speed = speed;
        _powerupForce = powerupForce;
        enemyRb = GetComponent<Rigidbody>();
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

        if (biggerTrig) triggerBigger();
        if (smallerTrig) triggerSmaller();
        if (powerupTrig) triggerPowerup();
        biggerTrig = false;
        smallerTrig = false;
        powerupTrig = false;
        
        if (focusOnPlayer != null)
        {
            Ray ray = new Ray(focusOnPlayer.transform.position, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (!hit.collider.gameObject.CompareTag("Ground"))
                {
                    focusOnPlayer = null;
                }
            }
            else
            {
                focusOnPlayer = null;
            }
        }
        if (focusOnPlayer == null)
        {
            focusOnPlayer = FindCloesetPlayer();
        }

        if (focusOnPlayer != null)
        {
            Vector3 lookDirection = (focusOnPlayer.transform.position - transform.position).normalized;
            enemyRb.AddForce(lookDirection * _speed);
        }
        else
        {
            Vector3 lookDirection = (new Vector3(0, 0, 0) - transform.position).normalized;
            enemyRb.AddForce(lookDirection * _speed);
        }
    }
    
    
    GameObject FindCloesetPlayer()
    {
        Mate[] mates = GameObject.FindObjectsOfType<Mate>();
            
        GameObject closestPlayer = null;
        float closestDistance = 0;
        foreach (var mate in mates)
        {
            float distance = Vector3.Distance(mate.gameObject.transform.position, transform.position);
            if (closestDistance == 0 || distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = mate.gameObject;
            }
        }

        if (closestDistance == 0 || closestDistance > Vector3.Distance(player.transform.position, transform.position))
        {
            closestPlayer = player;
        }

        return closestPlayer;
    }

    private void OnDestroy()
    {
        Destroy(powerupIndicator);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Powerup"))
        {
            Destroy(other.gameObject);
            triggerPowerup();
        }
        else if (other.CompareTag("Bigger"))
        {
            Destroy(other.gameObject);
            triggerBigger();
        }
        else if (other.CompareTag("Smaller"))
        {
            Destroy(other.gameObject);
            triggerSmaller();
        }
    }

    public void triggerPowerup()
    {
        hasPowerup = true;
        powerupIndicator.gameObject.SetActive(true);

        if (powerupCountDown != null)
        {
            StopCoroutine(powerupCountDown);
        }
        powerupCountDown = PowerupCountdownRoutine();
        StartCoroutine(powerupCountDown);
    }

    public void triggerBigger()
    {
        if (!hasBigger)
        {
            transform.localScale = new Vector3(4.5f, 4.5f, 4.5f);
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            powerupIndicator.transform.localScale = new Vector3(9, 3, 9);
            enemyRb.mass = 5;
            _speed = speed * 5;
        }

        hasSmaller = false;
        hasBigger = true;
            
        if (biggerCountDown != null)
        {
            StopCoroutine(biggerCountDown);
        }
        biggerCountDown = BiggerCountdownRoutine();
        StartCoroutine(biggerCountDown);
    }

    public void triggerSmaller()
    {
        if (!hasSmaller)
        {
            transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            powerupIndicator.transform.localScale = new Vector3(1, 1/3.0f, 1);
            enemyRb.mass = 1;
            _speed = speed * 2;
        }

        hasSmaller = true;
        hasBigger = false;
            
        if (biggerCountDown != null)
        {
            StopCoroutine(biggerCountDown);
        }
        biggerCountDown = BiggerCountdownRoutine();
        StartCoroutine(biggerCountDown);
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
    
    IEnumerator BiggerCountdownRoutine()
    {
        yield return new WaitForSeconds(7);
        hasBigger = false;
        transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        powerupIndicator.transform.localScale = new Vector3(3, 1, 3);
        enemyRb.mass = 1;
        _speed = speed;
    }
}
