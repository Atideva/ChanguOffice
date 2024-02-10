using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
    public struct ControlsModeEvent
    {
        public bool Status;
        public InputManager.MovementControls MovementControl;

        public ControlsModeEvent(bool status, InputManager.MovementControls movementControl)
        {
            Status = status;
            MovementControl = movementControl;
        }

        static ControlsModeEvent e;

        public static void Trigger(bool status, InputManager.MovementControls movementControl)
        {
            e.Status = status;
            e.MovementControl = movementControl;
            MMEventManager.TriggerEvent(e);
        }
    }
}