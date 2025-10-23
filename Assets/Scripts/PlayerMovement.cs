using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]  // kinematic
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 10f;

    [Header("Collision")]
    public LayerMask collisionLayers = ~0;    // collide with everything by default
    public float skin = 0.02f;                // small offset so we don’t touch exactly

    private Rigidbody rb;
    private CapsuleCollider cap;
    private float halfHeight;
    private float radius;

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody>();
        cap = GetComponent<CapsuleCollider>();

        // Rigidbody: transform-driven & stable for Fusion
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        CacheCapsule();

        // Attach camera only for local player
        if (HasStateAuthority)
        {
            FollowCamera cam = Camera.main.GetComponent<FollowCamera>();
            if (cam != null)
                cam.SetTarget(transform);
        }
    }

    void OnValidate()
    {
        if (cap == null) cap = GetComponent<CapsuleCollider>();
        if (cap != null) CacheCapsule();
    }

    void CacheCapsule()
    {
        if (cap == null) return;
        radius = Mathf.Max(0.01f, cap.radius - skin);
        halfHeight = Mathf.Max(radius, cap.height * 0.5f - skin);
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 dir = new Vector3(h, 0f, v).normalized;

        if (dir.sqrMagnitude < 0.0001f) return;

        float step = moveSpeed * Runner.DeltaTime;

        // Capsule endpoints in world space (current position)
        GetCapsuleWorldEnds(transform.position, out Vector3 p0, out Vector3 p1);

        // Sweep: if blocked, slide along the surface
        if (Physics.CapsuleCast(p0, p1, radius, dir, out RaycastHit hit, step, collisionLayers, QueryTriggerInteraction.Ignore))
        {
            // Slide: remove the component of movement into the hit normal
            Vector3 along = Vector3.ProjectOnPlane(dir, hit.normal).normalized;
            if (along.sqrMagnitude > 0.0001f)
            {
                float remain = step * (1f - hit.distance / step);
                // try slide move
                GetCapsuleWorldEnds(transform.position, out p0, out p1);
                if (!Physics.CapsuleCast(p0, p1, radius, along, out RaycastHit slideHit, remain, collisionLayers, QueryTriggerInteraction.Ignore))
                {
                    transform.position += along * remain;
                }
            }
        }
        else
        {
            // Free move
            transform.position += dir * step;
        }

        // Face movement direction (visual)
        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Runner.DeltaTime);
    }

    private void GetCapsuleWorldEnds(Vector3 worldCenter, out Vector3 p0, out Vector3 p1)
    {
        // CapsuleCollider is aligned with Y axis
        Vector3 center = worldCenter + transform.rotation * cap.center;
        p0 = center + Vector3.up * (halfHeight - radius);
        p1 = center - Vector3.up * (halfHeight - radius);
    }

}
