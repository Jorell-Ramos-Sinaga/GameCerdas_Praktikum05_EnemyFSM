using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimplePlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;

    private CharacterController controller;
    private Animator animator;
    private float verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 movement = Vector3.zero;

        if (Camera.main != null)
        {
            Vector3 camForward = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;

            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            movement = (camRight * horizontal + camForward * vertical).normalized;
        }
        else
        {
            movement = new Vector3(horizontal, 0f, vertical).normalized;
        }

        controller.Move(
            movement * moveSpeed * Time.deltaTime
        );

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        controller.Move(
            Vector3.up *
            verticalVelocity *
            Time.deltaTime
        );

        if (movement.sqrMagnitude > 0.01f)
        {
            transform.forward = movement;
        }

        if (animator != null)
        {
            animator.SetBool("isWalking", movement.sqrMagnitude > 0.01f);
        }
    }
}