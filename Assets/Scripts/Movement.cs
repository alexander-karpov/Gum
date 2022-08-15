using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gum
{
    public class Movement : MonoBehaviour
    {
        CharacterController controller;

        const float speed = 5f;

        [SerializeField]
        float rotationSmoothTime = .1f;

        float currentAngle;

        float rotationVelocity = 1f;

        [Header("Gravity")]
        [SerializeField]
        float gravity = 9.8f;

        [SerializeField]
        float gravityMultiplier = 2;

        [SerializeField]
        float groundedGravity = -0.5f;

        [SerializeField]
        float jumpHeight = 3f;

        float yVelocity;

        Vector2 touchBeginPosition;

        // Start is called before the first frame update
        void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        // Update is called once per frame
        void Update()
        {
            var horizontal = Input.GetAxisRaw("Horizontal");
            var vertical = Input.GetAxisRaw("Vertical");

            HandleGravityAndJump();

            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);

                // Move the cube if the screen has the finger moving.
                if (touch.phase == TouchPhase.Began)
                {
                    touchBeginPosition = touch.position;
                }

                if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                {
                    var touchMovement = touch.position - touchBeginPosition;

                    horizontal = touchMovement.x;
                    vertical = touchMovement.y;
                }
            }

            if (horizontal == 0 && vertical == 0)
            {
                return;
            }

            Vector3 input = new Vector3(horizontal, 0, vertical).normalized;

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
            if (controller.isGrounded && yVelocity < 0f)
            {
                yVelocity = groundedGravity;
            }

            if (controller.isGrounded && Input.GetAxisRaw("Jump") > 0)
            {
                yVelocity = Mathf.Sqrt(jumpHeight * 2f * gravity);
            }

            yVelocity -= gravity * gravityMultiplier * Time.deltaTime;
            controller.Move(yVelocity * Time.deltaTime * Vector3.up);
        }
    }
}
