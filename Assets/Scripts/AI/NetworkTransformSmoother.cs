using Fusion;
using UnityEngine;

[DisallowMultipleComponent]
public class NetworkTransformSmoother : NetworkBehaviour
{
    [Tooltip("How quickly remote objects interpolate to their new positions")]
    public float positionLerpSpeed = 8f;
    public float rotationLerpSpeed = 12f;

    private Vector3 _lastPosition;
    private Quaternion _lastRotation;

    public override void Spawned()
    {
        _lastPosition = transform.position;
        _lastRotation = transform.rotation;
    }

    void Update()
    {
        // Only smooth remote (non-authority) objects
        if (Object == null || Object.HasStateAuthority)
            return;

        // Smoothly interpolate between last known and current position
        transform.position = Vector3.Lerp(transform.position, _lastPosition, Time.deltaTime * positionLerpSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, _lastRotation, Time.deltaTime * rotationLerpSpeed);
    }

    public override void Render()
    {
        // Called each render tick with freshest state from network
        _lastPosition = transform.position;
        _lastRotation = transform.rotation;
    }
}
