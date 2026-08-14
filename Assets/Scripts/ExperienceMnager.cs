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
        StartCoroutine(CloseCreditsAfterDelay());
    }
    IEnumerator CloseCreditsAfterDelay()
    {
        yield return new WaitForSeconds(_delayToReloadScene);
        _credits.SetActive(false);
    }

}
