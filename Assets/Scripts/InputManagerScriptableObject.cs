using UnityEngine;
using UnityEngine.InputSystem;

namespace Gum
{
    [CreateAssetMenu(fileName = "InputManager", menuName = "ScriptableObjects/InputManager")]
    public class InputManagerScriptableObject : ScriptableObject
    {
        static double JumpTapTime = 1;

        static float JumpTapRadius = 10;

        static (double touchId, int frame) lastJump;

        public static (Vector2 move, bool Jump) Movement()
        {
            var move = Vector2.zero;
            var jump = false;

            if (Keyboard.current.dKey.IsPressed())
            {
                move.x += 1;
            }

            if (Keyboard.current.aKey.IsPressed())
            {
                move.x -= 1;
            }

            if (Keyboard.current.wKey.IsPressed())
            {
                move.y += 1;
            }

            if (Keyboard.current.sKey.IsPressed())
            {
                move.y -= 1;
            }

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                jump = true;
            }

            if (move.x != 0 || move.y != 0 || jump)
            {
                return (move.normalized, jump);
            }

            foreach (var touch in Touchscreen.current.touches)
            {
                var state = touch.ReadValue();
                var start = state.startPosition;
                var position = state.position;
                var startTime = state.startTime;
                var inProgress = state.isInProgress;
                var touchId = state.startTime;

                var isMovementTouch = start.x < Screen.width / 2;
                var isJumpTouch = !isMovementTouch;

                if (isMovementTouch && inProgress)
                {
                    move = position - start;
                    continue;
                }

                if (
                    isJumpTouch &&
                    !inProgress &&
                    Time.realtimeSinceStartup - startTime < JumpTapTime &&
                    !(lastJump.frame != Time.frameCount && lastJump.touchId == touchId)
                )
                {
                    lastJump.touchId = touchId;
                    lastJump.frame = Time.frameCount;

                    var delta = position - start;
                    jump = delta.magnitude < JumpTapRadius;
                }
            }

            return (move, jump);
        }
    }
}
