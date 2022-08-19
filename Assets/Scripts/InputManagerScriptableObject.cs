using UnityEngine;
using UnityEngine.InputSystem;

namespace Gum
{
    [CreateAssetMenu(fileName = "InputManager", menuName = "ScriptableObjects/InputManager")]
    public class InputManagerScriptableObject : ScriptableObject
    {
        public const float JumpTapRadius = 10;

        static (double startTime, int frame) lastJump;

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

            var touches = Touchscreen.current.touches;

            for (int i = 0; i < touches.Count && i < 2; i++)
            {
                var state = touches[i].ReadValue();

                var startPosition = state.startPosition;
                var position = state.position;
                var startTime = state.startTime;
                var inProgress = state.isInProgress;

                var isMovementTouch = startPosition.x < Screen.width / 2;
                var isJumpTouch = !isMovementTouch;

                if (isMovementTouch && inProgress)
                {
                    move = position - startPosition;
                }

                if (
                    isJumpTouch &&
                    !inProgress &&
                    !(lastJump.frame != Time.frameCount && lastJump.startTime == startTime)
                )
                {
                    lastJump.startTime = startTime;
                    lastJump.frame = Time.frameCount;

                    var delta = position - startPosition;
                    jump = delta.magnitude < JumpTapRadius;
                }
            }

            return (move, jump);
        }
    }
}
