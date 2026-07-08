using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ExperienceMnager : MonoBehaviour
{
    public static ExperienceMnager Instance { get; private set; }
    private bool[] _targetsFound = new bool[6];
    [SerializeField] private GameObject _credits;
    [SerializeField] private float _delayToReloadScene = 5f;
    public UnityEvent OnGameFinish;
    private void Start()
    {
        Instance = this;
        OnGameFinish.AddListener(ShowCredits);
    }

    private void ShowCredits()
    {
        _credits.SetActive(true);
        StartCoroutine(LoadSceneAfterDelay());
    }
    IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(_delayToReloadScene);
        _credits.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void FoundTarget(int targetIndex)
    {
        _targetsFound[targetIndex] = true;
    }

    public bool AllTargetsFound()
    {
        foreach (bool target in _targetsFound)
        {
            if (!target) return false;
        }
        return true;
    }
}
