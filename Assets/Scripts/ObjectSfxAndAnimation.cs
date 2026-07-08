using UnityEngine;
using DG.Tweening;
using System.Collections;

public class ObjectSfxAndAnimation : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip activationSfx;
    [SerializeField] private AudioClip activationFollowupSfx;
    [SerializeField, Range(0f, 1f)] private float activationVolume = 1f;
    [SerializeField, Range(-3f, 3f)] private float activationPitch = 1f;

    [Header("Scale Animation")]
    [SerializeField] private Vector3 targetScale = Vector3.one;
    [SerializeField] private float scaleDuration = 0.45f;
    [SerializeField] private Ease scaleEase = Ease.OutBack;

    [Header("Rotation Animation")]
    [SerializeField] private float secondsPerTurn = 2f;
    [SerializeField] private Ease rotationEase = Ease.Linear;

    private Tween growTween;
    private Tween rotateTween;
    private Coroutine audioSequenceRoutine;

    private void OnEnable()
    {
        PlayActivationSfx();
        PlayActivationAnimation();
    }

    private void OnDisable()
    {
        StopAllTweens();
        StopAudioSequence();
    }
    private void Update()
    {
        if (audioSource != null && audioSource.isPlaying)
            return;

        if (ExperienceMnager.Instance.AllTargetsFound())
        {
            //ExperienceMnager.Instance.OnGameFinish?.Invoke();
        }
        ExperienceMnager.Instance.OnGameFinish?.Invoke();
        gameObject.SetActive(false);
    }
    private void PlayActivationAnimation()
    {
        StopAllTweens();
        EnsureUpright();

        transform.localScale = Vector3.zero;

        growTween = transform
            .DOScale(targetScale, scaleDuration)
            .SetEase(scaleEase)
            .OnComplete(StartInfiniteRotation);
    }

    private void EnsureUpright()
    {
        Vector3 localEuler = transform.localEulerAngles;
        transform.localEulerAngles = new Vector3(0f, localEuler.y, 0f);
    }

    private void PlayActivationSfx()
    {
        if (activationSfx == null)
        {
            return;
        }

        AudioSource source = audioSource != null ? audioSource : GetComponent<AudioSource>();
        if (source == null)
        {
            return;
        }

        StopAudioSequence();
        audioSequenceRoutine = StartCoroutine(PlayAudioSequence(source));
    }

    private IEnumerator PlayAudioSequence(AudioSource source)
    {
        source.pitch = activationPitch;
        source.PlayOneShot(activationSfx, activationVolume);

        float safePitch = Mathf.Max(0.01f, Mathf.Abs(activationPitch));
        float waitTime = activationSfx.length / safePitch;
        yield return new WaitForSeconds(waitTime);

        if (activationFollowupSfx != null)
        {
            source.PlayOneShot(activationFollowupSfx, activationVolume);
        }

        audioSequenceRoutine = null;
    }

    private void StartInfiniteRotation()
    {
        rotateTween = transform
            .DORotate(new Vector3(0f, 360f, 0f), secondsPerTurn, RotateMode.LocalAxisAdd)
            .SetEase(rotationEase)
            .SetLoops(-1, LoopType.Restart);
    }

    private void StopAllTweens()
    {
        if (growTween != null && growTween.IsActive())
        {
            growTween.Kill();
        }

        if (rotateTween != null && rotateTween.IsActive())
        {
            rotateTween.Kill();
        }
    }

    private void StopAudioSequence()
    {
        if (audioSequenceRoutine != null)
        {
            StopCoroutine(audioSequenceRoutine);
            audioSequenceRoutine = null;
        }
    }
}
