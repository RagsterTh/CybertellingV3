using UnityEngine;
using DG.Tweening;

public class Billboard : MonoBehaviour
{
    [SerializeField] Transform target;

    [Header("Config")]
    [SerializeField] float minimumDistanceToActivate;
    [SerializeField] float timeToUpdateDist = 0.5f;

    [Header("Debug")]
    Vector3 playerPosLastFrame;
    float timer;
    void Start()
    {

    }
    void FixedUpdate()
    {
        timer += Time.deltaTime;
        if(timer >= timeToUpdateDist)
        {
            float distanceSquared = minimumDistanceToActivate * minimumDistanceToActivate;
            Vector3 offset = transform.position - target.position;
            if (offset.sqrMagnitude < distanceSquared)
                OnLook();

            timer = 0;
        }
        
    }
    void OnLook()
    {
        if ((target.position - playerPosLastFrame).sqrMagnitude > 0.01f)
        {
            Vector3 direction = transform.position - target.position;
            direction.y = 0;

            Quaternion rotationToLook = Quaternion.LookRotation(direction, transform.up);

            transform.DORotate(rotationToLook.eulerAngles, 0.1f);

            playerPosLastFrame = target.position;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, minimumDistanceToActivate);
    }
}
