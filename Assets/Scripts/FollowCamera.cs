using Fusion;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0f, 15f, -10f);
    public float followSpeed = 10f;

    private Transform target;

    public void SetTarget(Transform t)
    {
        target = t;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Smoothly follow target
        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);
        transform.LookAt(target.position);
    }
}
