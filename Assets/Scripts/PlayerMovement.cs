using Fusion;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkObject))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 10f;

    private Rigidbody rb;
    private Vector3 inputVector;

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.None; // Fusion handles interpolation
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        // --- Input ---
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        inputVector = new Vector3(h, 0f, v).normalized;

        // --- Movement ---
        Vector3 move = inputVector * moveSpeed * Runner.DeltaTime;
        rb.position += move;

        // --- Rotation ---
        if (inputVector != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(inputVector, Vector3.up);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Runner.DeltaTime);
        }

        if (!HasStateAuthority) return;
    }
}
