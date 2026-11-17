using UnityEngine;

[ExecuteAlways]
public class FollowHand : MonoBehaviour
{
    [SerializeField] private Transform handTransform;
    [SerializeField] private Vector3 offset;
    [SerializeField] private Vector3 rotationOffset;
    void Update()
    {
        transform.position = handTransform.position + offset;
        transform.rotation = handTransform.rotation * Quaternion.Euler(rotationOffset);
    }
}
