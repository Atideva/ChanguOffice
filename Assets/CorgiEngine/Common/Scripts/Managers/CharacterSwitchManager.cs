using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using MoreMountains.InventoryEngine;

namespace MoreMountains.CorgiEngine
{
    public class CharacterSwitchManager : CorgiMonoBehaviour
    {
        public bool initOnStart = true;
        public bool allowSwitchByInput = true;

        public enum NextCharacterChoices
        {
            Sequential,
            Random
        }

        [Header("Character Switch")]
        public string PlayerID = "Player1";
        public int PlayerIndex;
        public Character[] CharacterPrefabs;
        public NextCharacterChoices NextCharacterChoice = NextCharacterChoices.Sequential;
        public int CurrentIndex = 0;
        public bool CommonHealth;
        public bool MaintainPreviousCharacterFacingDirection = false;

        [Header("Visual Effects")]
        public ParticleSystem CharacterSwitchVFX;

        [Header("Debug")]
        [MMInspectorButton("ForceCharacterSwitch")]
        public bool ForceCharacterSwitchButton;
        public int DebugTargetIndex = 1;
        [MMInspectorButton("DebugCharacterSwitchToTargetIndex")]
        public bool DebugCharacterSwitchToTargetIndexButton;
        [MMReadOnly] public InputManager LinkedInputManager;
        protected Character[] _instantiatedCharacters;
        protected ParticleSystem _instantiatedVFX;
        protected CorgiEngineEvent _switchEvent = new(CorgiEngineEventTypes.CharacterSwitch, null);

        public virtual void ForceCharacterSwitch()
            => StartCoroutine(SwitchCharacter());

        public virtual void ForceCharacterSwitchTo(int newIndex)
            => StartCoroutine(SwitchCharacter(newIndex));

        protected virtual void Start()
        {
            if (!initOnStart) return;
            Init();
        }

        public void Init()
        {
            GetInputManager();
            InstantiateCharacters();
            InstantiateVFX();
        }

        protected virtual void GetInputManager()
        {
            if (string.IsNullOrEmpty(PlayerID)) return;

            LinkedInputManager = null;
            var foundInputManagers = FindObjectsOfType(typeof(InputManager)) as InputManager[];
            foreach (var foundInputManager in foundInputManagers)
            {
                if (foundInputManager.PlayerID == PlayerID)
                    LinkedInputManager = foundInputManager;
            }
        }

        protected virtual void InstantiateCharacters()
        {
            _instantiatedCharacters = new Character[CharacterPrefabs.Length];

            for (var i = 0; i < CharacterPrefabs.Length; i++)
            {
                var newCharacter = Instantiate(CharacterPrefabs[i]);
                newCharacter.name = "CharacterSwitch_" + i;
                newCharacter.gameObject.SetActive(false);
                newCharacter.transform.position = this.transform.position;
                _instantiatedCharacters[i] = newCharacter;
            }
        }

        protected virtual void InstantiateVFX()
        {
            if (CharacterSwitchVFX == null) return;
            _instantiatedVFX = Instantiate(CharacterSwitchVFX);
            _instantiatedVFX.Stop();
            _instantiatedVFX.gameObject.SetActive(false);
        }

        protected virtual void Update()
        {
            if (!allowSwitchByInput) return;
            if (!LinkedInputManager) return;
            if (LinkedInputManager.SwitchCharacterButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
                Switch();
        }

        protected virtual void DebugCharacterSwitchToTargetIndex()
            => StartCoroutine(SwitchCharacter(DebugTargetIndex));

        protected virtual void DetermineNextIndex()
        {
            switch (NextCharacterChoice)
            {
                case NextCharacterChoices.Random:
                    CurrentIndex = Random.Range(0, _instantiatedCharacters.Length);
                    break;
                case NextCharacterChoices.Sequential:
                {
                    CurrentIndex += 1;
                    if (CurrentIndex >= _instantiatedCharacters.Length)
                        CurrentIndex = 0;
                    break;
                }
            }
        }

        public void Switch()
        {
            StartCoroutine(SwitchCharacter());
        }

        protected virtual IEnumerator SwitchCharacter()
        {
            if (_instantiatedCharacters.Length <= 1) yield break;
            DetermineNextIndex();
            StartCoroutine(OperateSwitch());
        }

        protected virtual IEnumerator SwitchCharacter(int newIndex)
        {
            if (_instantiatedCharacters.Length <= 1) yield break;
            CurrentIndex = newIndex;
            if (CurrentIndex < 0 || CurrentIndex >= _instantiatedCharacters.Length)
                CurrentIndex = 0;

            StartCoroutine(OperateSwitch());
        }

        protected virtual IEnumerator OperateSwitch()
        {
            var newHealth = LevelManager.Instance.Players[PlayerIndex].gameObject.MMGetComponentNoAlloc<Health>().CurrentHealth;
            var facingRight = LevelManager.Instance.Players[PlayerIndex].IsFacingRight;
            LevelManager.Instance.Players[PlayerIndex].gameObject.SetActive(false);
            _instantiatedCharacters[CurrentIndex].SetPlayerID(PlayerID, PlayerIndex);
            _instantiatedCharacters[CurrentIndex].gameObject.SetActive(true);
            _instantiatedCharacters[CurrentIndex].transform.position = LevelManager.Instance.Players[PlayerIndex].transform.position;
            _instantiatedCharacters[CurrentIndex].transform.rotation = LevelManager.Instance.Players[PlayerIndex].transform.rotation;
            _instantiatedCharacters[CurrentIndex].MovementState.ChangeState(LevelManager.Instance.Players[PlayerIndex].MovementState.CurrentState);
            _instantiatedCharacters[CurrentIndex].ConditionState.ChangeState(LevelManager.Instance.Players[PlayerIndex].ConditionState.CurrentState);
            LevelManager.Instance.Players[PlayerIndex] = _instantiatedCharacters[CurrentIndex];

            if (_instantiatedVFX)
            {
                _instantiatedVFX.gameObject.SetActive(true);
                _instantiatedVFX.transform.position = _instantiatedCharacters[CurrentIndex].transform.position;
                _instantiatedVFX.Play();
            }

            MMEventManager.TriggerEvent(_switchEvent);
            yield return null;

            if (CommonHealth)
                LevelManager.Instance.Players[PlayerIndex].gameObject.MMGetComponentNoAlloc<Health>().SetHealth(newHealth, this.gameObject);

            if (MaintainPreviousCharacterFacingDirection)
            {
                var facingDirection = facingRight ? Character.FacingDirections.Right : Character.FacingDirections.Left;
                LevelManager.Instance.Players[PlayerIndex].Face(facingDirection);
            }
        }
    }
}