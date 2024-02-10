using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using System;
using System.Linq;

namespace MoreMountains.CorgiEngine
{
    [SelectionBase]
    [AddComponentMenu("Corgi Engine/Character/Core/Character")]
    public class Character : CorgiMonoBehaviour
    {
        public enum CharacterTypes
        {
            Player,
            AI
        }

        public enum FacingDirections
        {
            Left,
            Right
        }

        public enum SpawnFacingDirections
        {
            Default,
            Left,
            Right
        }

        [MMInformation("The Character script is the mandatory basis for all Character abilities. Your character can either be a Non Player Character, controlled by an AI, or a Player character, controlled by the player. In this case, you'll need to specify a PlayerID, which must match the one specified in your InputManager. Usually 'Player1', 'Player2', etc.", MMInformationAttribute.InformationType.Info, false)]
        public CharacterTypes CharacterType = CharacterTypes.AI;
        public string PlayerID = "";
        public CharacterStates CharacterState { get; protected set; }

        [Header("Direction")]
        [MMInformation("It's usually good practice to build all your characters facing right. If that's not the case of this character, select Left instead.", MMInformationAttribute.InformationType.Info, false)]
        public FacingDirections InitialFacingDirection = FacingDirections.Right;

        [MMInformation("Here you can force a direction the character should face when spawning. If set to default, it'll match your model's initial facing direction.", MMInformationAttribute.InformationType.Info, false)]
        [Tooltip("the direction the character will face on spawn")]
        public SpawnFacingDirections DirectionOnSpawn = SpawnFacingDirections.Default;
        public bool IsFacingRight { get; set; }

        [Header("Animator")]
        [MMInformation("The engine will try and find an animator for this character. If it's on the same gameobject it should have found it. If it's nested somewhere, you'll need to bind it below. You can also decide to get rid of it altogether, in that case, just uncheck 'use mecanim'.", MMInformationAttribute.InformationType.Info, false)]
        public Animator CharacterAnimator;
        public bool UseDefaultMecanim = true;
        public bool PerformAnimatorSanityChecks = true;
        public bool DisableAnimatorLogs = false;

        [Header("Model")]
        [MMInformation("Leave this unbound if this is a regular, sprite-based character, and if the SpriteRenderer and the Character are on the same GameObject. If not, you'll want to parent the actual model to the Character object, and bind it below. See the 3D demo characters for an example of that. The idea behind that is that the model may move, flip, but the collider will remain unchanged.", MMInformationAttribute.InformationType.Info, false)]
        public GameObject CharacterModel;
        public GameObject CameraTarget;
        public float CameraTargetSpeed = 5f;

        [Header("Abilities")]
        public List<GameObject> AdditionalAbilityNodes;

        [MMInformation("You can also decide if the character must automatically flip when going backwards or not. Additionnally, if you're not using sprites, you can define here how the character's model's localscale will be affected by flipping. By default it flips on the x axis, but you can change that to fit your model.", MMInformationAttribute.InformationType.Info, false)]
        public bool FlipModelOnDirectionChange = true;
        [MMCondition("FlipModelOnDirectionChange", true)]
        public Vector3 ModelFlipValue = new Vector3(-1, 1, 1);
        public bool RotateModelOnDirectionChange;
        [MMCondition("RotateModelOnDirectionChange", true)]
        public Vector3 ModelRotationValue = new Vector3(0f, 180f, 0f);
        [MMCondition("RotateModelOnDirectionChange", true)]
        public float ModelRotationSpeed = 0f;

        [Header("Health")]
        public Health CharacterHealth;

        [Header("Events")]
        [MMInformation("Here you can define whether or not you want to have that character trigger events when changing state. See the MMTools' State Machine doc for more info.", MMInformationAttribute.InformationType.Info, false)]
        public bool SendStateChangeEvents = true;
        public bool SendStateUpdateEvents = true;

        [Header("Airborne")]
        public float AirborneDistance = 0.5f;
        public float AirborneMinimumTime = 0.1f;
        public bool Airborne => _controller.DistanceToTheGround > AirborneDistance || _controller.DistanceToTheGround == -1;

        [Header("AI")]
        public AIBrain CharacterBrain;
        public MMStateMachine<CharacterStates.MovementStates> MovementState;
        public MMStateMachine<CharacterStates.CharacterConditions> ConditionState;
        public CameraController SceneCamera { get; protected set; }
        public InputManager LinkedInputManager { get; protected set; }
        public Animator _animator { get; protected set; }
        public HashSet<int> _animatorParameters { get; set; }
        public bool CanFlip { get; set; }

        protected const string _groundedAnimationParameterName = "Grounded";
        protected const string _fallingAnimationParameterName = "Falling";
        protected const string _airborneAnimationParameterName = "Airborne";
        protected const string _xSpeedAnimationParameterName = "xSpeed";
        protected const string _ySpeedAnimationParameterName = "ySpeed";
        protected const string _xSpeedAbsoluteAnimationParameterName = "xSpeedAbsolute";
        protected const string _ySpeedAbsoluteAnimationParameterName = "ySpeedAbsolute";
        protected const string _worldXSpeedAnimationParameterName = "WorldXSpeed";
        protected const string _worldYSpeedAnimationParameterName = "WorldYSpeed";
        protected const string _collidingLeftAnimationParameterName = "CollidingLeft";
        protected const string _collidingRightAnimationParameterName = "CollidingRight";
        protected const string _collidingBelowAnimationParameterName = "CollidingBelow";
        protected const string _collidingAboveAnimationParameterName = "CollidingAbove";
        protected const string _idleSpeedAnimationParameterName = "Idle";
        protected const string _aliveAnimationParameterName = "Alive";
        protected const string _facingRightAnimationParameterName = "FacingRight";
        protected const string _randomAnimationParameterName = "Random";
        protected const string _randomConstantAnimationParameterName = "RandomConstant";
        protected const string _flipAnimationParameterName = "Flip";

        protected int _groundedAnimationParameter;
        protected int _fallingAnimationParameter;
        protected int _airborneSpeedAnimationParameter;
        protected int _xSpeedAnimationParameter;
        protected int _ySpeedAnimationParameter;
        protected int _xSpeedAbsoluteAnimationParameter;
        protected int _ySpeedAbsoluteAnimationParameter;
        protected int _worldXSpeedAnimationParameter;
        protected int _worldYSpeedAnimationParameter;
        protected int _collidingLeftAnimationParameter;
        protected int _collidingRightAnimationParameter;
        protected int _collidingBelowAnimationParameter;
        protected int _collidingAboveAnimationParameter;
        protected int _idleSpeedAnimationParameter;
        protected int _aliveAnimationParameter;
        protected int _facingRightAnimationParameter;
        protected int _randomAnimationParameter;
        protected int _randomConstantAnimationParameter;
        protected int _flipAnimationParameter;

        protected CorgiController _controller;
        protected SpriteRenderer _spriteRenderer;
        protected Color _initialColor;
        protected CharacterAbility[] _characterAbilities;
        protected float _originalGravity;
        protected bool _spawnDirectionForced = false;
        protected Vector3 _targetModelRotation;
        protected DamageOnTouch _damageOnTouch;
        protected Vector3 _cameraTargetInitialPosition;
        protected Vector3 _cameraOffset = Vector3.zero;
        protected bool _abilitiesCachedOnce = false;
        protected float _animatorRandomNumber;
        protected CharacterPersistence _characterPersistence;
        protected Coroutine _conditionChangeCoroutine;
        protected CharacterStates.CharacterConditions _lastState;

        protected virtual void Awake()
        {
            Initialization();
        }

        public virtual void Initialization()
        {
            MovementState = new MMStateMachine<CharacterStates.MovementStates>(gameObject, SendStateChangeEvents);
            ConditionState = new MMStateMachine<CharacterStates.CharacterConditions>(gameObject, SendStateChangeEvents);

            MovementState.ChangeState(CharacterStates.MovementStates.Idle);

            IsFacingRight = InitialFacingDirection != FacingDirections.Left;

            if (CameraTarget == null)
            {
                CameraTarget = new GameObject();
                CameraTarget.transform.SetParent(transform);
                CameraTarget.transform.localPosition = Vector3.zero;
                CameraTarget.name = "CameraTarget";
            }

            _cameraTargetInitialPosition = CameraTarget.transform.localPosition;

            SetInputManager();
            GetMainCamera();
            CharacterState = new CharacterStates();
            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            _controller = gameObject.GetComponent<CorgiController>();
            _characterPersistence = gameObject.GetComponent<CharacterPersistence>();
            CacheAbilitiesAtInit();
            if (CharacterBrain == null)
                CharacterBrain = gameObject.GetComponent<AIBrain>();
            if (CharacterBrain)
                CharacterBrain.Owner = gameObject;
            if (CharacterHealth == null)
                CharacterHealth = gameObject.GetComponent<Health>();
            _damageOnTouch = gameObject.GetComponent<DamageOnTouch>();
            CanFlip = true;
            AssignAnimator();

            _originalGravity = _controller.Parameters.Gravity;

            ForceSpawnDirection();
        }

        public virtual void GetMainCamera()
        {
            if (Camera.main)
                SceneCamera = Camera.main.GetComponent<CameraController>();
        }

        protected virtual void CacheAbilitiesAtInit()
        {
            if (_abilitiesCachedOnce) return;
            CacheAbilities();
        }

        public virtual void CacheAbilities()
        {
            _characterAbilities = gameObject.GetComponents<CharacterAbility>();
            if (AdditionalAbilityNodes is { Count: > 0 })
            {
                var tempAbilityList = _characterAbilities.ToList();
                foreach (var tempArray in AdditionalAbilityNodes.Select(g =>
                             g.GetComponentsInChildren<CharacterAbility>()))
                    tempAbilityList.AddRange(tempArray);
                _characterAbilities = tempAbilityList.ToArray();
            }

            _abilitiesCachedOnce = true;
        }

        public T FindAbility<T>() where T : CharacterAbility
        {
            CacheAbilitiesAtInit();
            var searchedAbilityType = typeof(T);
            foreach (var ability in _characterAbilities)
            {
                if (ability is T characterAbility)
                    return characterAbility;
            }

            return null;
        }

        public List<T> FindAbilities<T>() where T : CharacterAbility
        {
            CacheAbilitiesAtInit();

            var resultList = new List<T>();
            var searchedAbilityType = typeof(T);
            foreach (var ability in _characterAbilities)
            {
                if (ability is T characterAbility)
                    resultList.Add(characterAbility);
            }

            return resultList;
        }

        public virtual void ChangeAnimator(Animator newAnimator)
        {
            _animator = newAnimator;
            if (!_animator) return;
            InitializeAnimatorParameters();
            if (DisableAnimatorLogs)
                _animator.logWarnings = false;
        }

        public virtual void AssignAnimator()
        {
            _animator = CharacterAnimator ? CharacterAnimator : gameObject.GetComponent<Animator>();
            if (!_animator) return;
            InitializeAnimatorParameters();
            if (DisableAnimatorLogs)
                _animator.logWarnings = false;
        }

        public virtual void SetInputManager()
        {
            if (CharacterType == CharacterTypes.AI)
            {
                LinkedInputManager = null;
                UpdateInputManagersInAbilities();
                return;
            }

            if (!string.IsNullOrEmpty(PlayerID))
            {
                LinkedInputManager = null;
                var foundInputManagers = FindObjectsOfType(typeof(InputManager)) as InputManager[];
                foreach (var foundInputManager in foundInputManagers)
                {
                    if (foundInputManager.PlayerID == PlayerID)
                        LinkedInputManager = foundInputManager;
                }
            }

            UpdateInputManagersInAbilities();
        }

        public virtual void SetInputManager(InputManager inputManager)
        {
            LinkedInputManager = inputManager;
            UpdateInputManagersInAbilities();
        }

        protected virtual void UpdateInputManagersInAbilities()
        {
            if (_characterAbilities == null) return;
            foreach (var a in _characterAbilities)
                a.SetInputManager(LinkedInputManager);
        }

        public virtual void ResetInput()
        {
            if (_characterAbilities == null) return;
            foreach (var ability in _characterAbilities)
                ability.ResetInput();
        }

        public virtual void SetPlayerID(string newPlayerID, int id)
        {
            ID = id;
            PlayerID = newPlayerID;
            SetInputManager();
        }

        protected virtual void Update()
        {
            EveryFrame();
        }

        protected virtual void EveryFrame()
        {
            HandleCharacterStatus();
            EarlyProcessAbilities();

            if (Time.timeScale != 0f)
            {
                ProcessAbilities();
                LateProcessAbilities();
                HandleCameraTarget();
            }

            UpdateAnimators();
            RotateModel();
        }

        protected virtual void RotateModel()
        {
            if (!RotateModelOnDirectionChange)
            {
                return;
            }

            CharacterModel.transform.localEulerAngles = ModelRotationSpeed > 0f ? Vector3.Lerp(CharacterModel.transform.localEulerAngles, _targetModelRotation, Time.deltaTime * ModelRotationSpeed) : _targetModelRotation;
        }

        protected virtual void EarlyProcessAbilities()
        {
            foreach (var ability in _characterAbilities)
            {
                if (ability.enabled && ability.AbilityInitialized)
                    ability.EarlyProcessAbility();
            }
        }

        protected virtual void ProcessAbilities()
        {
            foreach (var ability in _characterAbilities)
            {
                if (ability.enabled && ability.AbilityInitialized)
                    ability.ProcessAbility();
            }
        }

        protected virtual void LateProcessAbilities()
        {
            foreach (var ability in _characterAbilities)
            {
                if (ability.enabled && ability.AbilityInitialized)
                    ability.LateProcessAbility();
            }
        }

        protected virtual void InitializeAnimatorParameters()
        {
            if (!_animator) return;

            _animatorParameters = new HashSet<int>();

            MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _groundedAnimationParameterName, out _groundedAnimationParameter, AnimatorControllerParameterType.Bool, _animatorParameters);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _fallingAnimationParameterName, out _fallingAnimationParameter, AnimatorControllerParameterType.Bool, _animatorParameters);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _airborneAnimationParameterName, out _airborneSpeedAnimationParameter, AnimatorControllerParameterType.Bool, _animatorParameters);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _xSpeedAnimationParameterName, out _xSpeedAnimationParameter, AnimatorControllerParameterType.Float, _animatorParameters);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _ySpeedAnimationParameterName, out _ySpeedAnimationParameter, AnimatorControllerParameterType.Float, _animatorParameters);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _xSpeedAbsoluteAnimationParameterName, out _xSpeedAbsoluteAnimationParameter, AnimatorControllerParameterType.Float, _animatorParameters);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _ySpeedAbsoluteAnimationParameterName, out _ySpeedAbsoluteAnimationParameter, AnimatorControllerParameterType.Float, _animatorParameters);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _worldXSpeedAnimationParameterName, out _worldXSpeedAnimationParameter, AnimatorControllerParameterType.Float, _animatorParameters);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _worldYSpeedAnimationParameterName, out _worldYSpeedAnimationParameter, AnimatorControllerParameterType.Float, _animatorParameters);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _collidingLeftAnimationParameterName, out _collidingLeftAnimationParameter, AnimatorControllerParameterType.Bool, _animatorParameters);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _collidingRightAnimationParameterName, out _collidingRightAnimationParameter, AnimatorControllerParameterType.Bool, _animatorParameters);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _collidingBelowAnimationParameterName, out _collidingBelowAnimationParameter, AnimatorControllerParameterType.Bool, _animatorParameters);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _collidingAboveAnimationParameterName, out _collidingAboveAnimationParameter, AnimatorControllerParameterType.Bool, _animatorParameters);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _idleSpeedAnimationParameterName, out _idleSpeedAnimationParameter, AnimatorControllerParameterType.Bool, _animatorParameters);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _aliveAnimationParameterName, out _aliveAnimationParameter, AnimatorControllerParameterType.Bool, _animatorParameters);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _facingRightAnimationParameterName, out _facingRightAnimationParameter, AnimatorControllerParameterType.Bool, _animatorParameters);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _randomAnimationParameterName, out _randomAnimationParameter, AnimatorControllerParameterType.Float, _animatorParameters);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _randomConstantAnimationParameterName, out _randomConstantAnimationParameter, AnimatorControllerParameterType.Int, _animatorParameters);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _flipAnimationParameterName, out _flipAnimationParameter, AnimatorControllerParameterType.Trigger, _animatorParameters);


            var randomConstant = UnityEngine.Random.Range(0, 1000);
            MMAnimatorExtensions.UpdateAnimatorInteger(_animator, _randomConstantAnimationParameter, randomConstant, _animatorParameters);
        }

        protected virtual void UpdateAnimators()
        {
            if (!UseDefaultMecanim || !_animator) return;
            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _groundedAnimationParameter, _controller.State.IsGrounded, _animatorParameters, PerformAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _fallingAnimationParameter, MovementState.CurrentState == CharacterStates.MovementStates.Falling, _animatorParameters, PerformAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _airborneSpeedAnimationParameter, Airborne, _animatorParameters, PerformAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _aliveAnimationParameter, ConditionState.CurrentState != CharacterStates.CharacterConditions.Dead, _animatorParameters, PerformAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _xSpeedAnimationParameter, _controller.Speed.x, _animatorParameters, PerformAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _ySpeedAnimationParameter, _controller.Speed.y, _animatorParameters, PerformAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _xSpeedAbsoluteAnimationParameter, Mathf.Abs(_controller.Speed.x), _animatorParameters, PerformAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _ySpeedAbsoluteAnimationParameter, Mathf.Abs(_controller.Speed.y), _animatorParameters, PerformAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _worldXSpeedAnimationParameter, _controller.WorldSpeed.x, _animatorParameters, PerformAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _worldYSpeedAnimationParameter, _controller.WorldSpeed.y, _animatorParameters, PerformAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _collidingLeftAnimationParameter, _controller.State.IsCollidingLeft, _animatorParameters, PerformAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _collidingRightAnimationParameter, _controller.State.IsCollidingRight, _animatorParameters, PerformAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _collidingBelowAnimationParameter, _controller.State.IsCollidingBelow, _animatorParameters, PerformAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _collidingAboveAnimationParameter, _controller.State.IsCollidingAbove, _animatorParameters, PerformAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _idleSpeedAnimationParameter, MovementState.CurrentState == CharacterStates.MovementStates.Idle, _animatorParameters, PerformAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _facingRightAnimationParameter, IsFacingRight, _animatorParameters);

            UpdateAnimationRandomNumber();
            MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _randomAnimationParameter, _animatorRandomNumber, _animatorParameters, PerformAnimatorSanityChecks);

            foreach (var ability in _characterAbilities)
            {
                if (ability.enabled && ability.AbilityInitialized)
                    ability.UpdateAnimator();
            }
        }

        protected virtual void UpdateAnimationRandomNumber()
        {
            _animatorRandomNumber = UnityEngine.Random.Range(0f, 1f);
        }

        protected virtual void HandleCharacterStatus()
        {
            switch (ConditionState.CurrentState)
            {
                case CharacterStates.CharacterConditions.Dead when CharacterHealth:
                {
                    if (CharacterHealth.GravityOffOnDeath)
                        _controller.GravityActive(false);

                    if (CharacterHealth.ApplyDeathForce && CharacterHealth.DeathForce.x == 0f)
                        _controller.SetHorizontalForce(0);

                    break;
                }
                case CharacterStates.CharacterConditions.Dead:
                    _controller.SetHorizontalForce(0);
                    return;
                case CharacterStates.CharacterConditions.Frozen:
                    _controller.GravityActive(false);
                    _controller.SetForce(Vector2.zero);
                    break;
            }
        }

        public virtual void Freeze()
        {
            _controller.GravityActive(false);
            _controller.SetForce(Vector2.zero);
            if (ConditionState.CurrentState != CharacterStates.CharacterConditions.Frozen)
                _conditionStateBeforeFreeze = ConditionState.CurrentState;

            ConditionState.ChangeState(CharacterStates.CharacterConditions.Frozen);
        }

        protected CharacterStates.CharacterConditions _conditionStateBeforeFreeze;
       public int ID { get; private set; }

        public virtual void UnFreeze()
        {
            _controller.GravityActive(true);
            ConditionState.ChangeState(_conditionStateBeforeFreeze);
        }

        public virtual void RecalculateRays()
        {
            _controller.SetRaysParameters();
        }

        public virtual void Disable()
        {
            enabled = false;
            _controller.enabled = false;
            gameObject.MMGetComponentNoAlloc<Collider2D>().enabled = false;
        }

        public virtual void RespawnAt(Transform spawnPoint, FacingDirections facingDirection)
        {
            if (!gameObject.activeInHierarchy) return;

            UnFreeze();
            Face(facingDirection);
            ConditionState.ChangeState(CharacterStates.CharacterConditions.Normal);
            gameObject.MMGetComponentNoAlloc<Collider2D>().enabled = true;
            _controller.CollisionsOn();
            transform.position = spawnPoint.position;
            Physics2D.SyncTransforms();

            if (!CharacterHealth) return;
            if (_characterPersistence)
            {
                if (_characterPersistence.Initialized)
                {
                    if (CharacterHealth)
                        CharacterHealth.UpdateHealthBar(false);
                    return;
                }
            }

            CharacterHealth.ResetHealthToMaxHealth();
            CharacterHealth.Revive();
        }

        public virtual void Flip(bool IgnoreFlipOnDirectionChange = false)
        {
            if (!FlipModelOnDirectionChange && !RotateModelOnDirectionChange && !IgnoreFlipOnDirectionChange) return;
            if (!CanFlip) return;

            if (!FlipModelOnDirectionChange && !RotateModelOnDirectionChange && IgnoreFlipOnDirectionChange)
            {
                if (CharacterModel)
                    CharacterModel.transform.localScale = Vector3.Scale(CharacterModel.transform.localScale, ModelFlipValue);
                else
                {
                    if (_spriteRenderer)
                        _spriteRenderer.flipX = !_spriteRenderer.flipX;
                }
            }

            FlipModel();

            if (_animator)
                MMAnimatorExtensions.SetAnimatorTrigger(_animator, _flipAnimationParameter, _animatorParameters, PerformAnimatorSanityChecks);

            IsFacingRight = !IsFacingRight;


            foreach (var ability in _characterAbilities)
            {
                if (ability.enabled)
                    ability.Flip();
            }
        }

        public virtual void FlipModel()
        {
            if (FlipModelOnDirectionChange)
            {
                if (CharacterModel)
                    CharacterModel.transform.localScale = Vector3.Scale(CharacterModel.transform.localScale, ModelFlipValue);
                else if (_spriteRenderer)
                    _spriteRenderer.flipX = !_spriteRenderer.flipX;
            }

            if (!RotateModelOnDirectionChange) return;
            if (!CharacterModel) return;
            _targetModelRotation += ModelRotationValue;
            _targetModelRotation.x %= 360;
            _targetModelRotation.y %= 360;
            _targetModelRotation.z %= 360;
        }

        protected virtual void ForceSpawnDirection()
        {
            if (DirectionOnSpawn == SpawnFacingDirections.Default || _spawnDirectionForced)
                return;

            _spawnDirectionForced = true;
            if (DirectionOnSpawn == SpawnFacingDirections.Left)
                Face(FacingDirections.Left);
            if (DirectionOnSpawn == SpawnFacingDirections.Right)
                Face(FacingDirections.Right);
        }

        public virtual void Face(FacingDirections facingDirection)
        {
            if (!CanFlip) return;

            if (facingDirection == FacingDirections.Right)
            {
                if (!IsFacingRight) Flip(true);
            }
            else if (IsFacingRight) Flip(true);
        }

        public virtual void ChangeCharacterConditionTemporarily(
            CharacterStates.CharacterConditions newCondition,
            float duration, bool resetControllerForces, bool disableGravity)
        {
            if (_conditionChangeCoroutine != null)
                StopCoroutine(_conditionChangeCoroutine);
            _conditionChangeCoroutine = StartCoroutine(ChangeCharacterConditionTemporarilyCo(newCondition, duration, resetControllerForces, disableGravity));
        }

        protected virtual IEnumerator ChangeCharacterConditionTemporarilyCo(
            CharacterStates.CharacterConditions newCondition,
            float duration, bool resetControllerForces, bool disableGravity)
        {
            if (ConditionState.CurrentState != newCondition)
                _lastState = ConditionState.CurrentState;

            ConditionState.ChangeState(newCondition);
            if (resetControllerForces)
                _controller?.SetForce(Vector2.zero);

            if (disableGravity && _controller)
                _controller.GravityActive(false);

            yield return MMCoroutine.WaitFor(duration);
            ConditionState.ChangeState(_lastState);
            if (disableGravity && _controller)
                _controller.GravityActive(true);
        }

        protected virtual void HandleCameraTarget()
            => CameraTarget.transform.localPosition = Vector3.Lerp(CameraTarget.transform.localPosition, _cameraTargetInitialPosition + _cameraOffset, Time.deltaTime * CameraTargetSpeed);

        public virtual void SetCameraTargetOffset(Vector3 offset)
        {
            _cameraOffset = offset;
        }

        public virtual void Reset()
        {
            _spawnDirectionForced = false;
            if (_characterAbilities == null)
                return;

            if (_characterAbilities.Length == 0)
                return;

            foreach (var ability in _characterAbilities)
            {
                if (ability.enabled)
                    ability.ResetAbility();
            }
        }

        protected virtual void OnRevive()
        {
            ForceSpawnDirection();
            if (CharacterBrain)
                CharacterBrain.enabled = true;
            if (_damageOnTouch)
                _damageOnTouch.enabled = true;
        }

        protected virtual void OnDeath()
        {
            if (CharacterBrain)
            {
                CharacterBrain.TransitionToState("");
                CharacterBrain.enabled = false;
            }

            if (_damageOnTouch)
                _damageOnTouch.enabled = false;
        }

        protected virtual void OnEnable()
        {
            if (!CharacterHealth) return;
            CharacterHealth.OnRevive += OnRevive;
            CharacterHealth.OnDeath += OnDeath;
        }

        protected virtual void OnDisable()
        {
            if (CharacterHealth)
                CharacterHealth.OnDeath -= OnDeath;
        }
    }
}