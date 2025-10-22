using Fusion;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb;   // Rigidbody for physics-based movement
    public float playerSpeed = 5f;           // Speed of the player

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;  // Ensure only the local player controls their own movement

        // Get player input
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Calculate movement direction
        Vector3 move = new Vector3(h, 0, v) * playerSpeed * Runner.DeltaTime;

        // Apply movement to Rigidbody
        rb.MovePosition(rb.position + move);

        // Update player rotation to face the direction of movement
        if (move != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.deltaTime * 10f));  // Smooth rotation
        }
    }

    private void Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();  // Get Rigidbody component if not set in the inspector
        }

        // Disable gravity if you're manually handling gravity (optional)
        rb.useGravity = true;
    }
}
