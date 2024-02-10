using System;
using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.Events;
using System.Collections.Generic;

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

    [AddComponentMenu("Corgi Engine/Managers/Input Manager")]
    public class InputManager : MMSingleton<InputManager>
    {
        [Header("Settings")]
        public bool InputDetectionActive = true;
        public bool ResetButtonStatesOnFocusLoss = true;

        [Header("Player binding")]
        [MMInformation("The first thing you need to set on your InputManager is the PlayerID. This ID will be used to bind the input manager to your character(s). You'll want to go with Player1, Player2, Player3 or Player4.", MMInformationAttribute.InformationType.Info, false)]
        public string PlayerID = "Player1";

        public enum InputForcedMode
        {
            None,
            Mobile,
            Desktop
        }

        public enum MovementControls
        {
            Joystick,
            Arrows
        }

        [Header("Mobile controls")]
        [MMInformation("If you check Auto Mobile Detection, the engine will automatically switch to mobile controls when your build target is Android or iOS. You can also force mobile or desktop (keyboard, gamepad) controls using the dropdown below.\nNote that if you don't need mobile controls and/or GUI this component can also work on its own, just put it on an empty GameObject instead.", MMInformationAttribute.InformationType.Info, false)]
        public bool AutoMobileDetection = true;
        public InputForcedMode ForcedMode;
        public bool HideMobileControlsInEditor = false;
        public MovementControls MovementControl = MovementControls.Joystick;
        public bool DelayedButtonPresses = false;
        public bool IsMobile { get; protected set; }

        [Header("Movement settings")]
        [MMInformation("Turn SmoothMovement on to have inertia in your controls (meaning there'll be a small delay between a press/release of a direction and your character moving/stopping). You can also define here the horizontal and vertical thresholds.", MMInformationAttribute.InformationType.Info, false)]
        public bool SmoothMovement = true;
        public Vector2 Threshold = new Vector2(0.1f, 0.4f);
        public MMInput.IMButton JumpButton { get; protected set; }
        public MMInput.IMButton SwimButton { get; protected set; }
        public MMInput.IMButton GlideButton { get; protected set; }
        public MMInput.IMButton InteractButton { get; protected set; }
        public MMInput.IMButton JetpackButton { get; protected set; }
        public MMInput.IMButton FlyButton { get; protected set; }
        public MMInput.IMButton RunButton { get; protected set; }
        public MMInput.IMButton DashButton { get; protected set; }
        public MMInput.IMButton RollButton { get; protected set; }
        public MMInput.IMButton GrabButton { get; protected set; }
        public MMInput.IMButton ThrowButton { get; protected set; }
        public MMInput.IMButton ShootButton { get; protected set; }
        public MMInput.IMButton SecondaryShootButton { get; protected set; }
        public MMInput.IMButton ReloadButton { get; protected set; }
        public MMInput.IMButton PushButton { get; protected set; }
        public MMInput.IMButton GripButton { get; protected set; }
        public MMInput.IMButton PauseButton { get; protected set; }
        public MMInput.IMButton RestartButton { get; protected set; }
        public MMInput.IMButton SwitchCharacterButton { get; protected set; }
        public MMInput.IMButton SwitchWeaponButton { get; protected set; }
        public MMInput.IMButton TimeControlButton { get; protected set; }
        public MMInput.ButtonStates ShootAxis { get; protected set; }
        public MMInput.ButtonStates SecondaryShootAxis { get; protected set; }
        public Vector2 PrimaryMovement { get { return _primaryMovement; } }
        public Vector2 SecondaryMovement { get { return _secondaryMovement; } }

        protected List<MMInput.IMButton> ButtonList;
        protected Vector2 _primaryMovement = Vector2.zero;
        protected Vector2 _secondaryMovement = Vector2.zero;
        protected string _axisHorizontal;
        protected string _axisVertical;
        protected string _axisSecondaryHorizontal;
        protected string _axisSecondaryVertical;
        protected string _axisShoot;
        protected string _axisShootSecondary;

        protected virtual void Start()
        {
            Initialization();
        }

        protected virtual void Initialization()
        {
            ControlsModeDetection();
            InitializeButtons();
            InitializeAxis();
        }

        public virtual void ControlsModeDetection()
        {
            ControlsModeEvent.Trigger(false, MovementControls.Joystick);
            IsMobile = false;
            if (AutoMobileDetection)
            {
#if UNITY_ANDROID || UNITY_IPHONE
                    ControlsModeEvent.Trigger(true, MovementControl);
				    IsMobile = true;
#endif
            }

            switch (ForcedMode)
            {
                case InputForcedMode.Mobile:
                    ControlsModeEvent.Trigger(true, MovementControl);
                    IsMobile = true;
                    break;
                case InputForcedMode.Desktop:
                    ControlsModeEvent.Trigger(false, MovementControls.Joystick);
                    IsMobile = false;
                    break;
            }

            if (HideMobileControlsInEditor)
            {
#if UNITY_EDITOR
                ControlsModeEvent.Trigger(false, MovementControls.Joystick);
                IsMobile = false;
#endif
            }
        }

        protected virtual void InitializeButtons()
        {
            ButtonList = new List<MMInput.IMButton>();
            ButtonList.Add(JumpButton = new MMInput.IMButton(PlayerID, "Jump", JumpButtonDown, JumpButtonPressed, JumpButtonUp));
            ButtonList.Add(SwimButton = new MMInput.IMButton(PlayerID, "Swim", SwimButtonDown, SwimButtonPressed, SwimButtonUp));
            ButtonList.Add(GlideButton = new MMInput.IMButton(PlayerID, "Glide", GlideButtonDown, GlideButtonPressed, GlideButtonUp));
            ButtonList.Add(InteractButton = new MMInput.IMButton(PlayerID, "Interact", InteractButtonDown, InteractButtonPressed, InteractButtonUp));
            ButtonList.Add(JetpackButton = new MMInput.IMButton(PlayerID, "Jetpack", JetpackButtonDown, JetpackButtonPressed, JetpackButtonUp));
            ButtonList.Add(RunButton = new MMInput.IMButton(PlayerID, "Run", RunButtonDown, RunButtonPressed, RunButtonUp));
            ButtonList.Add(GripButton = new MMInput.IMButton(PlayerID, "Grip", GripButtonDown, GripButtonPressed, GripButtonUp));
            ButtonList.Add(DashButton = new MMInput.IMButton(PlayerID, "Dash", DashButtonDown, DashButtonPressed, DashButtonUp));
            ButtonList.Add(RollButton = new MMInput.IMButton(PlayerID, "Roll", RollButtonDown, RollButtonPressed, RollButtonUp));
            ButtonList.Add(FlyButton = new MMInput.IMButton(PlayerID, "Fly", FlyButtonDown, FlyButtonPressed, FlyButtonUp));
            ButtonList.Add(ShootButton = new MMInput.IMButton(PlayerID, "Shoot", ShootButtonDown, ShootButtonPressed, ShootButtonUp));
            ButtonList.Add(SecondaryShootButton = new MMInput.IMButton(PlayerID, "SecondaryShoot", SecondaryShootButtonDown, SecondaryShootButtonPressed, SecondaryShootButtonUp));
            ButtonList.Add(ReloadButton = new MMInput.IMButton(PlayerID, "Reload", ReloadButtonDown, ReloadButtonPressed, ReloadButtonUp));
            ButtonList.Add(SwitchWeaponButton = new MMInput.IMButton(PlayerID, "SwitchWeapon", SwitchWeaponButtonDown, SwitchWeaponButtonPressed, SwitchWeaponButtonUp));
            ButtonList.Add(PauseButton = new MMInput.IMButton(PlayerID, "Pause", PauseButtonDown, PauseButtonPressed, PauseButtonUp));
            ButtonList.Add(RestartButton = new MMInput.IMButton(PlayerID, "Restart", RestartButtonDown, RestartButtonPressed, RestartButtonUp));
            ButtonList.Add(PushButton = new MMInput.IMButton(PlayerID, "Push", PushButtonDown, PushButtonPressed, PushButtonUp));
            ButtonList.Add(SwitchCharacterButton = new MMInput.IMButton(PlayerID, "SwitchCharacter", SwitchCharacterButtonDown, SwitchCharacterButtonPressed, SwitchCharacterButtonUp));
            ButtonList.Add(TimeControlButton = new MMInput.IMButton(PlayerID, "TimeControl", TimeControlButtonDown, TimeControlButtonPressed, TimeControlButtonUp));
            ButtonList.Add(GrabButton = new MMInput.IMButton(PlayerID, "Grab", GrabButtonDown, GrabButtonPressed, GrabButtonUp));
            ButtonList.Add(ThrowButton = new MMInput.IMButton(PlayerID, "Throw", ThrowButtonDown, ThrowButtonPressed, ThrowButtonUp));
        }

        protected virtual void InitializeAxis()
        {
            _axisHorizontal = PlayerID + "_Horizontal";
            _axisVertical = PlayerID + "_Vertical";
            _axisSecondaryHorizontal = PlayerID + "_SecondaryHorizontal";
            _axisSecondaryVertical = PlayerID + "_SecondaryVertical";
            _axisShoot = PlayerID + "_ShootAxis";
            _axisShootSecondary = PlayerID + "_SecondaryShootAxis";
        }

        protected virtual void LateUpdate()
        {
            ProcessButtonStates();
        }

        protected virtual void Update()
        {
            if (!IsMobile && InputDetectionActive)
            {
                SetMovement();
                SetSecondaryMovement();
                SetShootAxis();
                GetInputButtons();
            }
        }

        protected virtual void GetInputButtons()
        {
            foreach (MMInput.IMButton button in ButtonList)
            {
                if (Input.GetButton(button.ButtonID))
                    button.TriggerButtonPressed();

                if (Input.GetButtonDown(button.ButtonID))
                    button.TriggerButtonDown();

                if (Input.GetButtonUp(button.ButtonID))
                    button.TriggerButtonUp();
            }
        }

        public virtual void ProcessButtonStates()
        {
            // for each button, if we were at ButtonDown this frame, we go to ButtonPressed. If we were at ButtonUp, we're now Off
            foreach (MMInput.IMButton button in ButtonList)
            {
                if (button.State.CurrentState == MMInput.ButtonStates.ButtonDown)
                {
                    if (DelayedButtonPresses)
                        StartCoroutine(DelayButtonPress(button));
                    else
                        button.State.ChangeState(MMInput.ButtonStates.ButtonPressed);
                }

                if (button.State.CurrentState == MMInput.ButtonStates.ButtonUp)
                {
                    if (DelayedButtonPresses)
                        StartCoroutine(DelayButtonRelease(button));
                    else
                        button.State.ChangeState(MMInput.ButtonStates.Off);
                }
            }
        }

        IEnumerator DelayButtonPress(MMInput.IMButton button)
        {
            yield return null;
            button.State.ChangeState(MMInput.ButtonStates.ButtonPressed);
        }

        IEnumerator DelayButtonRelease(MMInput.IMButton button)
        {
            yield return null;
            button.State.ChangeState(MMInput.ButtonStates.Off);
        }

        public virtual void SetMovement()
        {
            if (IsMobile || !InputDetectionActive) return;

            if (SmoothMovement)
            {
                _primaryMovement.x = Input.GetAxis(_axisHorizontal);
                _primaryMovement.y = Input.GetAxis(_axisVertical);
            }
            else
            {
                _primaryMovement.x = Input.GetAxisRaw(_axisHorizontal);
                _primaryMovement.y = Input.GetAxisRaw(_axisVertical);
            }
        }

        public virtual void SetSecondaryMovement()
        {
            if (IsMobile || !InputDetectionActive) return;

            if (SmoothMovement)
            {
                _secondaryMovement.x = Input.GetAxis(_axisSecondaryHorizontal);
                _secondaryMovement.y = Input.GetAxis(_axisSecondaryVertical);
            }
            else
            {
                _secondaryMovement.x = Input.GetAxisRaw(_axisSecondaryHorizontal);
                _secondaryMovement.y = Input.GetAxisRaw(_axisSecondaryVertical);
            }
        }

        protected virtual void SetShootAxis()
        {
            if (!IsMobile && InputDetectionActive)
            {
                ShootAxis = MMInput.ProcessAxisAsButton(_axisShoot, Threshold.y, ShootAxis, MMInput.AxisTypes.Positive);
                SecondaryShootAxis = MMInput.ProcessAxisAsButton(_axisShootSecondary, Threshold.y, SecondaryShootAxis, MMInput.AxisTypes.Positive);
            }
        }

        public virtual void SetMovement(Vector2 movement)
        {
            if (IsMobile && InputDetectionActive)
            {
                _primaryMovement.x = movement.x;
                _primaryMovement.y = movement.y;
            }
        }

        public virtual void SetSecondaryMovement(Vector2 movement)
        {
            if (IsMobile && InputDetectionActive)
            {
                _secondaryMovement.x = movement.x;
                _secondaryMovement.y = movement.y;
            }
        }

        public virtual void SetHorizontalMovement(float horizontalInput)
        {
            if (IsMobile && InputDetectionActive)
                _primaryMovement.x = horizontalInput;
        }

        public virtual void SetVerticalMovement(float verticalInput)
        {
            if (IsMobile && InputDetectionActive)
                _primaryMovement.y = verticalInput;
        }

        public virtual void SetSecondaryHorizontalMovement(float horizontalInput)
        {
            if (IsMobile && InputDetectionActive)
                _secondaryMovement.x = horizontalInput;
        }

        public virtual void SetSecondaryVerticalMovement(float verticalInput)
        {
            if (IsMobile && InputDetectionActive)
                _secondaryMovement.y = verticalInput;
        }

        protected virtual void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && ResetButtonStatesOnFocusLoss && (ButtonList != null))
                ForceAllButtonStatesTo(MMInput.ButtonStates.ButtonUp);
        }

        public virtual void ForceAllButtonStatesTo(MMInput.ButtonStates newState)
        {
            foreach (MMInput.IMButton button in ButtonList)
                button.State.ChangeState(newState);
        }

        public virtual void JumpButtonDown() { JumpButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void JumpButtonPressed() { JumpButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void JumpButtonUp() { JumpButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void SwimButtonDown() { SwimButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void SwimButtonPressed() { SwimButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void SwimButtonUp() { SwimButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void GlideButtonDown() { GlideButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void GlideButtonPressed() { GlideButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void GlideButtonUp() { GlideButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void InteractButtonDown() { InteractButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void InteractButtonPressed() { InteractButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void InteractButtonUp() { InteractButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void DashButtonDown() { DashButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void DashButtonPressed() { DashButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void DashButtonUp() { DashButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void RollButtonDown() { RollButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void RollButtonPressed() { RollButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void RollButtonUp() { RollButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void FlyButtonDown() { FlyButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void FlyButtonPressed() { FlyButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void FlyButtonUp() { FlyButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void RunButtonDown() { RunButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void RunButtonPressed() { RunButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void RunButtonUp() { RunButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void JetpackButtonDown() { JetpackButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void JetpackButtonPressed() { JetpackButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void JetpackButtonUp() { JetpackButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void ReloadButtonDown() { ReloadButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void ReloadButtonPressed() { ReloadButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void ReloadButtonUp() { ReloadButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void PushButtonDown() { PushButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void PushButtonPressed() { PushButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void PushButtonUp() { PushButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void ShootButtonDown() { ShootButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void ShootButtonPressed() { ShootButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void ShootButtonUp() { ShootButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void GripButtonDown() { GripButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void GripButtonPressed() { GripButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void GripButtonUp() { GripButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void SecondaryShootButtonDown() { SecondaryShootButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void SecondaryShootButtonPressed() { SecondaryShootButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void SecondaryShootButtonUp() { SecondaryShootButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void PauseButtonDown() { PauseButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void PauseButtonPressed() { PauseButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void PauseButtonUp() { PauseButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void RestartButtonDown() { RestartButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void RestartButtonPressed() { RestartButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void RestartButtonUp() { RestartButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void SwitchWeaponButtonDown() { SwitchWeaponButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void SwitchWeaponButtonPressed() { SwitchWeaponButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void SwitchWeaponButtonUp() { SwitchWeaponButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void SwitchCharacterButtonDown() { SwitchCharacterButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void SwitchCharacterButtonPressed() { SwitchCharacterButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void SwitchCharacterButtonUp() { SwitchCharacterButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void TimeControlButtonDown() { TimeControlButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void TimeControlButtonPressed() { TimeControlButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void TimeControlButtonUp() { TimeControlButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void GrabButtonDown() { GrabButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void GrabButtonPressed() { GrabButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void GrabButtonUp() { GrabButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void ThrowButtonDown() { ThrowButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void ThrowButtonPressed() { ThrowButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void ThrowButtonUp() { ThrowButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }
    }
}