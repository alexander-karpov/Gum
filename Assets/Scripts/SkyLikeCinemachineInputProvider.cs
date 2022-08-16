using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace Gum
{
    public class SkyLikeCinemachineInputProvider : Cinemachine.CinemachineInputProvider
    {
        Vector2 _delta;

        int _touchId = -1;

        void Awake()
        {
            XYAxis.action.performed += XYAxisPerformed;
        }

        private void XYAxisPerformed(InputAction.CallbackContext obj)
        {
            var touch = obj.ReadValue<TouchState>();

            if (touch.phase == TouchPhase.Began)
            {
                var value = touch.position;
                var isSuitableTouch = value.x > Screen.width / 2;

                if (isSuitableTouch)
                {
                    _touchId = touch.touchId;
                    _delta = Vector2.zero;
                }
            }

            if (touch.phase == TouchPhase.Moved)
            {
                if (touch.touchId == _touchId)
                {
                    _delta += touch.delta;
                }
            }

            if (touch.phase == TouchPhase.Ended)
            {
                if (touch.touchId == _touchId)
                {
                    _touchId = -1;
                    _delta = Vector2.zero;
                }
            }
        }

        public override float GetAxisValue(int axis)
        {
            if (!enabled || _delta == Vector2.zero)
            {
                return 0;
            }

            var isHorizontalSwipe = Mathf.Abs(_delta.x) >= Mathf.Abs(_delta.y);
            var sign = Mathf.Sign(isHorizontalSwipe ? _delta.x : _delta.y);

            if (axis == 0 && isHorizontalSwipe || axis == 1 && !isHorizontalSwipe)
            {
                var value = _delta.magnitude * sign;
                _delta = Vector2.zero;

                return value;
            }

            return 0;
        }

        protected override void OnDisable()
        {
            XYAxis.action.performed -= XYAxisPerformed;
        }
    }
}
