using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] ColliderNotifier _end;
    [SerializeField] TMP_Text _resultText;
    [SerializeField] TMP_Text _counter;
    [SerializeField] float _threeStarTreshold;
    [SerializeField] float _twoStarTreshold;
    [SerializeField] float _oneStarTreshold;

    float _timeFromStart = 0;
    bool _isRunning = true;
    private void Start()
    {
        _end.ColliderHit += async () =>
        {
            _resultText.enabled=true;
            await NextLevel();
        };
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

    public async Awaitable NextLevel()
    {
        _isRunning = false;

        if (_timeFromStart < _twoStarTreshold)
            _resultText.text = "You have done it again!";
        else if (_timeFromStart < _threeStarTreshold)
            _resultText.text = "Not great not terrible!";
        else if (_timeFromStart > _threeStarTreshold)
        {
            _resultText.text = "Honestly, this is attrocious time, try again!!!";
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        try
        {
            await Awaitable.WaitForSecondsAsync(3);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        catch
        {
            //TODO game finished
        }
    }
}
