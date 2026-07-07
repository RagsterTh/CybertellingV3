using UnityEngine;

public class ExperienceMnager : MonoBehaviour
{
    private bool[] _targetsFound = new bool[2];
    [SerializeField] private GameObject _credits;
    public void FoundTarget(int targetIndex)
    {
        _targetsFound[targetIndex] = true;
        if (AllTargetsFound())
        {
            _credits.SetActive(true);
        }
    }

    private bool AllTargetsFound()
    {
        foreach (bool target in _targetsFound)
        {
            if (!target) return false;
        }
        return true;
    }
}
