using Fusion;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] CharacterController ch;
    public float playerSpeed;
   

    public override void FixedUpdateNetwork()
    {
        if(HasStateAuthority == false)
        {
            return;
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(h, 0, v) * playerSpeed * Runner.DeltaTime;

        ch.Move(move);

        if(move != Vector3.zero)
        {
            gameObject.transform.forward = move;
        }
    }
}
