using UnityEngine;

public class GenerateObjectController : MonoBehaviour
{

    public void ShowObject(GameObject obj)
    {
         obj.SetActive(true);
        obj.transform.position = transform.position;
    }
}
