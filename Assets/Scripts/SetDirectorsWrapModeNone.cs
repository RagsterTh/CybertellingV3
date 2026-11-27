using UnityEngine;
using UnityEngine.Playables;

public class SetDirectorsWrapModeNone : MonoBehaviour
{
    private void Start()
    {
        PlayableDirector[] directors = FindObjectsByType<PlayableDirector>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (PlayableDirector director in directors)
        {
            if (director.gameObject.scene != gameObject.scene) continue;
            director.extrapolationMode = DirectorWrapMode.None;
        }
    }
}
