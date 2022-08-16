using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Gum
{
    public class SkyLikeCinemachineInputProvider : Cinemachine.CinemachineInputProvider
    {
        Vector2 _previousValue;
        bool _isSuitableEvents;

        void Awake()
        {
            XYAxis.action.performed += XYAxisPerformed;
        }

        private void XYAxisPerformed(InputAction.CallbackContext obj)
        {
            var touch = obj.ReadValue<TouchState>();

            switch(touch.phase)
            {
                case UnityEngine.InputSystem.TouchPhase.Began:
                    var value = touch.position;
                    _isSuitableEvents = value.x > Screen.width / 2;
                    _previousValue = value;
                    return;
            }
        }

        public override float GetAxisValue(int axis)
        {
            if (!enabled)
            {
                return 0;
            }

            var action = ResolveForPlayer(axis, axis == 2 ? ZAxis : XYAxis);

            if (action == null || !action.inProgress || !_isSuitableEvents)
            {
                return 0;
            }

            var touch = action.ReadValue<TouchState>();
            var value = touch.position;
            var delta = value - _previousValue;
            var isHorizontalSwipe = Mathf.Abs(delta.x) >= Mathf.Abs(delta.y);

            _previousValue = value;

            return axis switch
            {
                0 => isHorizontalSwipe ? delta.x : 0,
                1 => !isHorizontalSwipe ? delta.y : 0,
                _ => 0
            };
        }

        protected override void OnDisable()
        {
            XYAxis.action.performed -= XYAxisPerformed;
        }
    }
}
