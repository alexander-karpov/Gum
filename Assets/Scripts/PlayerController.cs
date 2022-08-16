using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gum
{
    public class PlayerController : MonoBehaviour
    {
        CharacterController controller;

        PlayerInput playerInput;

        [Header("Movement")]
        [SerializeField]
        float speed = 5f;

        [SerializeField]
        float rotationSmoothTime = .1f;

        float currentAngle;

        float rotationVelocity;

        [Header("Jump and gravity")]
        [SerializeField]
        float gravity = 9.8f;

        [SerializeField]
        float gravityMultiplier = 2;

        [SerializeField]
        float groundedVelocity = -0.5f;

        [SerializeField]
        float jumpHeight = 3f;

        float yVelocity;

        bool jumpPrepared;

        // Start is called before the first frame update
        void Start()
        {
            controller = GetComponent<CharacterController>();
            playerInput = GetComponent<PlayerInput>();
        }

        // Update is called once per frame
        void Update()
        {
            HandleGravityAndJump();

            var move = playerInput.actions["Move"].ReadValue<Vector2>();

            if (move == Vector2.zero)
            {
                return;
            }

            Vector3 input = new Vector3(move.x, 0, move.y);

            float directionAngle =
                Mathf.Atan2(input.x, input.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;

            currentAngle =
                Mathf
                    .SmoothDampAngle(currentAngle,
                    directionAngle,
                    ref rotationVelocity,
                    rotationSmoothTime);

            Vector3 direction = Quaternion.Euler(0, directionAngle, 0) * Vector3.forward;

            transform.rotation = Quaternion.Euler(0, currentAngle, 0);
            controller.Move(speed * Time.deltaTime * direction);
        }

        void HandleGravityAndJump()
        {
            var jump = playerInput.actions["Jump"];
            var look = playerInput.actions["Look"];

            if (controller.isGrounded && yVelocity < 0f)
            {
                yVelocity = groundedVelocity;
            }

            if (!jumpPrepared && jump.WasPressedThisFrame())
            {
                jumpPrepared = true;
            }

            if (look.triggered)
            {
                jumpPrepared = false;
            }

            if (
                controller.isGrounded &&
                jumpPrepared &&
                playerInput.actions["Jump"].WasReleasedThisFrame()
            )
            {
                yVelocity = Mathf.Sqrt(jumpHeight * 2f * gravity);
                jumpPrepared = false;

                InputSystem.QueueDeltaStateEvent(Gamepad.current.rightStick, new Vector2(0.5f, 0f));
                Debug.Log("QueueDeltaStateEvent");
            }

            yVelocity -= gravity * gravityMultiplier * Time.deltaTime;
            controller.Move(yVelocity * Time.deltaTime * Vector3.up);
        }
    }
}
