using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private ColliderNotifier _end;
    [SerializeField] TMP_Text _resultText;
    [SerializeField] TMP_Text _counter;
    [SerializeField] float _threeStarTreshold;
    [SerializeField] float _twoStarTreshold;
    [SerializeField] float _oneStarTreshold;
    
    [SerializeField] GameObject EndLevelScreen;
    
    [SerializeField] GameObject unStar1;
    [SerializeField] GameObject unStar2;
    [SerializeField] GameObject unStar3;
    
    [SerializeField] GameObject Star1;
    [SerializeField] GameObject Star2;
    [SerializeField] GameObject Star3;
    
    [SerializeField] TMP_Text timeDisplay;
    

    float _timeFromStart = 0;
    bool _isRunning = true;
    private void Start()
    {
        // just find the end automatically
        _end = FindObjectsByType <ColliderNotifier>(FindObjectsSortMode.InstanceID)[0];
        
        _end.ColliderHit += async () =>
        {
            _resultText.enabled=true;
            // await NextLevel();
            OpenEndLevelScreen();

        };
    }

    public void ResetTime(float recordedTime)
    {
        _timeFromStart = recordedTime;
    }

    public float GetTime() => _timeFromStart;
    
    private void OpenEndLevelScreen()
    {
        EndLevelScreen.SetActive(true);
        Time.timeScale = 0;
        
        int stars = 0;

        if (_timeFromStart <= _oneStarTreshold)
            stars = 1;
        if (_timeFromStart <= _twoStarTreshold)
            stars = 2;
        if (_timeFromStart <= _threeStarTreshold)
            stars = 3;

        if (stars > 2)
        {
            unStar3.SetActive(false);
            Star3.SetActive(true);
        }
        if (stars > 1)
        {
            unStar2.SetActive(false);
            Star2.SetActive(true);
        }
        if (stars > 0)
        {
            unStar1.SetActive(false);
            Star1.SetActive(true);
        }
        
        timeDisplay.text = _timeFromStart.ToString("F0") + " Seconds";
        
    }

    private void Update()
    {
        if (!_isRunning)
            return;

        _timeFromStart += Time.deltaTime;
        _counter.text = _timeFromStart.ToString("F0");

        if (_timeFromStart > _oneStarTreshold)
            _counter.color = Color.red;
        else if (_timeFromStart > _twoStarTreshold)
            _counter.color = Color.orange;
        else if (_timeFromStart > _threeStarTreshold)
            _counter.color = Color.green;
    }

    public void NextLevel()
    {
        Time.timeScale = 1;
        _isRunning = false;
        try
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        catch
        {
            //TODO game finished
        }
    }
}
