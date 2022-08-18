using System;
using UnityEngine;

namespace Gum
{
    public class PlayerController : MonoBehaviour
    {
        CharacterController controller;

        [Header("Movement")]
        [SerializeField]
        float speed = 5f;

        [SerializeField]
        float rotationSmoothTime = .1f;

        float currentAngle;

        float rotationVelocity;

        Vector3 _normal = Vector3.up;

        Vector3 _direction = Vector3.forward;

        [Header("Jump and gravity")]
        [SerializeField]
        float gravityMultiplier = 2;

        [SerializeField]
        float jumpHeight = 3f;

        [SerializeField]
        float _coyoteTime = 0.1f;

        float _lastGroundedTime;

        float _yVelocity;

        void Start()
        {
            controller = GetComponent<CharacterController>();
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            _normal = hit.normal;
        }

        // Update is called once per frame
        void Update()
        {
            var fell = HandleFall();

            if (fell)
            {
                return;
            }

            if (transform.position.y < 10f)
            {
                transform.position = Vector3.zero;
            }

            var (move, jump) = InputManagerScriptableObject.Movement();

            var jumpReplacement = HandleGravityAndJump(jump);

            if (move == Vector2.zero)
            {
                controller.Move (jumpReplacement);
                var proj2 = _direction - Vector3.Dot(_direction, _normal) * _normal;
                transform.rotation = Quaternion.LookRotation(proj2, _normal);

                return;
            }

            var input = new Vector3(move.x, 0, move.y).normalized;

            float directionAngle =
                Mathf.Atan2(input.x, input.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;

            currentAngle =
                Mathf
                    .SmoothDampAngle(currentAngle,
                    directionAngle,
                    ref rotationVelocity,
                    rotationSmoothTime);

            _direction = Quaternion.Euler(0, directionAngle, 0) * Vector3.forward;

            // Магия
            var proj = _direction - Vector3.Dot(_direction, _normal) * _normal;
            transform.rotation = Quaternion.LookRotation(proj, _normal);

            controller.Move(speed * Time.deltaTime * _direction + jumpReplacement);
        }

        bool HandleFall()
        {
            if (transform.position.y < 0f)
            {
                transform.position = new Vector3(0, 32f, 0);

                return true;
            }

            return false;
        }

        Vector3 HandleGravityAndJump(bool jump)
        {
            if (controller.isGrounded && _yVelocity < 0f)
            {
                // Иначе будет прыгать как по лесенке при спуске по наклонной
                _yVelocity = -2f;
            }

            if (controller.isGrounded)
            {
                _lastGroundedTime = Time.time;
            }

            var canJump = controller.isGrounded || Time.time - _lastGroundedTime < _coyoteTime;

            if (jump && canJump)
            {
                _yVelocity = Mathf.Sqrt(jumpHeight * 2f * -Physics.gravity.y);

                // Чтобы избежать двойного прыжка в интервал _coyoteTime
                _lastGroundedTime = 0;
            }

            _yVelocity += Physics.gravity.y * gravityMultiplier * Time.deltaTime;

            return _yVelocity * Time.deltaTime * Vector3.up;
        }
    }
}
