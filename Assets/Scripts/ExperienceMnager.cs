using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ExperienceMnager : MonoBehaviour
{
    public static ExperienceMnager Instance { get; private set; }
    [SerializeField] private GameObject _credits;
    [SerializeField] private float _delayToReloadScene = 5f;
    public UnityEvent OnGameFinish;
    private Coroutine _closeCreditsRoutine;

    private void Awake()
    {
        Instance = this;
        if (OnGameFinish == null)
            OnGameFinish = new UnityEvent();
        OnGameFinish.AddListener(ShowCredits);
    }

    private void OnDestroy()
    {
        OnGameFinish?.RemoveListener(ShowCredits);
        if (Instance == this)
            Instance = null;
    }

    private void ShowCredits()
    {
        if (_credits == null)
            return;

        _credits.SetActive(true);
        if (_closeCreditsRoutine != null)
            StopCoroutine(_closeCreditsRoutine);
        _closeCreditsRoutine = StartCoroutine(CloseCreditsAfterDelay());
    }
    IEnumerator CloseCreditsAfterDelay()
    {
        yield return new WaitForSeconds(_delayToReloadScene);
        _credits.SetActive(false);
        _closeCreditsRoutine = null;
    }

}
