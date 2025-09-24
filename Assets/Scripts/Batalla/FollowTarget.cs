using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    private Transform target;
    private Vector3 offset;

    public void Init(Transform target, Vector3 offset)
    {
        this.target = target;
        this.offset = offset;
    }

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
}