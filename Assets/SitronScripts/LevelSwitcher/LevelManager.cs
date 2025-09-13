using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] ColliderNotifier _start;
    [SerializeField] ColliderNotifier _end;
    [SerializeField] GameObject _canvasObj;
    [SerializeField] List<Level> _allLevels;

    Level _currentLevel;

    public static LevelManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void Start()
    {
        _start.ColliderHit += () => 
        {
            _canvasObj.SetActive(false);
        };

        _end.ColliderHit += () =>
        {
            _canvasObj.SetActive(true);
        };
        LaunchLevel(_allLevels.First());
    }
    void LaunchLevel(Level level)
    {
        _currentLevel = level;
    }
    public void NextLevel()
    {
        if(_allLevels.ElementAtOrDefault(_allLevels.IndexOf(_currentLevel)+1) is Level nextLevel)
        {
            LaunchLevel(nextLevel);
        }
        else
        {
            //TODO game finished
        }
    }
}
