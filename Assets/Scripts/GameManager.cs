using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public InputField levelField;
    public Button startButton;
    
    public TextMeshProUGUI gameOverText;
    public Button restartButton;

    private SpawnManager _spawnManager;

    private bool _isGameActive = false;
    public bool IsGameActive => _isGameActive;

    // Start is called before the first frame update
    void Start()
    {
        _spawnManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame(int level)
    {
        titleText.gameObject.SetActive(false);
        levelField.gameObject.SetActive(false);
        startButton.gameObject.SetActive(false);
        
        _spawnManager.level = level - 1;
        _isGameActive = true;
    }

    public void GameOver()
    {
        gameOverText.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(true);
        _isGameActive = false;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void startButtonClicked()
    {
        int level;

        if (int.TryParse(levelField.text, out level))
        {
            if (level < 1 || level > 50)
            {
                level = 1;
            }
            StartGame(level);
            return;
        }

        StartGame(1);
    }

    public void levelFieldValueChanged()
    {
        levelField.text = Regex.Replace(levelField.text, @"[^0-9]", "");
    }
}
