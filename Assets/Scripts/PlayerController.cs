using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace Gum
{
    public class PlayerController : MonoBehaviour
    {
        CharacterController controller;

        [SerializeField]
        InputActionReference TouchAction;

        [Header("Movement")]
        [SerializeField]
        float speed = 5f;

        [SerializeField]
        float rotationSmoothTime = .1f;

        float currentAngle;

        float rotationVelocity;

        int _movementTouchId = -1;

        Vector2 _movementTouchStartPoint;

        Vector2 _movementTouchCurrentPoint;

        [Header("Jump and gravity")]
        [SerializeField]
        float gravityMultiplier = 2;

        [SerializeField]
        float jumpHeight = 3f;

        float _yVelocity;

        bool _jumpPrepared;

        void Start()
        {
            controller = GetComponent<CharacterController>();
        }

        void OnEnable()
        {
            TouchAction.action.Enable();
            TouchAction.action.performed += TouchPerformed;
        }

        void OnDisable()
        {
            TouchAction.action.performed -= TouchPerformed;
        }

        private void TouchPerformed(InputAction.CallbackContext obj)
        {
            var touch = obj.ReadValue<TouchState>();

            if (touch.isTap)
            {
                // Прыжок тапом по плавой части экрана
                _jumpPrepared = touch.isTap && touch.position.x > Screen.width / 2;
                Debug.Log (_jumpPrepared);
                Debug.Log(touch.phase);
            }

            if (touch.phase == TouchPhase.Began)
            {
                var isMovementTouch = touch.position.x < Screen.width / 2;

                if (isMovementTouch)
                {
                    _movementTouchId = touch.touchId;
                    _movementTouchStartPoint = touch.position;
                    _movementTouchCurrentPoint = touch.position;
                }
            }

            if (touch.phase == TouchPhase.Moved)
            {
                if (touch.touchId == _movementTouchId)
                {
                    _movementTouchCurrentPoint = touch.position;
                }
            }

            if (touch.phase == TouchPhase.Ended)
            {
                if (touch.touchId == _movementTouchId)
                {
                    _movementTouchId = -1;
                    _movementTouchStartPoint = touch.position;
                    _movementTouchCurrentPoint = touch.position;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            var jumpReplacement = HandleGravityAndJump();

            var move = _movementTouchCurrentPoint - _movementTouchStartPoint;

            if (move == Vector2.zero)
            {
                controller.Move (jumpReplacement);
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

            var direction = Quaternion.Euler(0, directionAngle, 0) * Vector3.forward;

            transform.rotation = Quaternion.Euler(0, currentAngle, 0);
            controller.Move(speed * Time.deltaTime * direction + jumpReplacement);
        }

        Vector3 HandleGravityAndJump()
        {
            if (controller.isGrounded)
            {
                if (_jumpPrepared)
                {
                    _yVelocity = Mathf.Sqrt(jumpHeight * 2f * -Physics.gravity.y);
                }
                else
                {
                    _yVelocity = 0;
                }
            }

            _yVelocity += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
            _jumpPrepared = false;

            return _yVelocity * Time.deltaTime * Vector3.up;
        }
    }
}
