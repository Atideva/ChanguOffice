using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using System.Linq;
using MoreMountains.InventoryEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.CorgiEngine
{
    [AddComponentMenu("Corgi Engine/Managers/Game Manager")]
    public class GameManager : MMPersistentSingleton<GameManager>, MMEventListener<MMGameEvent>, MMEventListener<CorgiEngineEvent>, MMEventListener<CorgiEnginePointsEvent>
    {
        [Header("Settings")]
        public int TargetFrameRate = 300;

        [Header("Lives")]
        public int MaximumLives = 0;
        public int CurrentLives = 0;

        [Header("Game Over")]
        public bool ResetLivesOnGameOver = true;
        public bool ResetPersistentCharacterOnGameOver = true;
        public bool ResetStoredCharacterOnGameOver = true;
        public string GameOverScene;

        public int Points { get; private set; }
        public bool Paused { get; set; }
        public bool StoredLevelMapPosition { get; set; }
        public Vector2 LevelMapPosition { get; set; }
        public Character StoredCharacter { get; set; }
        public Character PersistentCharacter { get; set; }
        public List<PointsOfEntryStorage> PointsOfEntry { get; set; }

        protected bool _inventoryOpen = false;
        protected bool _pauseMenuOpen = false;
        protected InventoryInputManager _inventoryInputManager;
        protected int _initialMaximumLives;
        protected int _initialCurrentLives;

        protected override void Awake()
        {
            base.Awake();
            PointsOfEntry = new List<PointsOfEntryStorage>();
        }

        protected virtual void Start()
        {
            Application.targetFrameRate = TargetFrameRate;
            _initialCurrentLives = CurrentLives;
            _initialMaximumLives = MaximumLives;
        }

        public virtual void Reset()
        {
            Points = 0;
            MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, 1f, 0f, false, 0f, true);
            Paused = false;
            GUIManager.Instance.RefreshPoints();
            PointsOfEntry?.Clear();
        }

        public virtual void LoseLife()
        {
            CurrentLives--;
        }

        public virtual void GainLives(int lives)
        {
            CurrentLives += lives;
            if (CurrentLives > MaximumLives)
                CurrentLives = MaximumLives;
        }

        public virtual void AddLives(int lives, bool increaseCurrent)
        {
            MaximumLives += lives;
            if (increaseCurrent)
                CurrentLives += lives;
        }

        public virtual void ResetLives()
        {
            CurrentLives = _initialCurrentLives;
            MaximumLives = _initialMaximumLives;
        }

        public virtual void AddPoints(int pointsToAdd)
        {
            Points += pointsToAdd;
            GUIManager.Instance.RefreshPoints();
        }

        public virtual void SetPoints(int points)
        {
            Points = points;
            GUIManager.Instance.RefreshPoints();
        }

        protected virtual void SetActiveInventoryInputManager(bool status)
        {
            _inventoryInputManager = GameObject.FindObjectOfType<InventoryInputManager>();
            if (_inventoryInputManager)
                _inventoryInputManager.enabled = status;
        }

        public virtual void Pause(PauseMethods pauseMethod = PauseMethods.PauseMenu)
        {
            if ((pauseMethod == PauseMethods.PauseMenu) && _inventoryOpen) return;

            if (Time.timeScale > 0.0f)
            {
                MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, 0f, 0f, false, 0f, true);
                Instance.Paused = true;
                if ((GUIManager.HasInstance) && (pauseMethod == PauseMethods.PauseMenu))
                {
                    GUIManager.Instance.SetPause(true);
                    _pauseMenuOpen = true;
                    SetActiveInventoryInputManager(false);
                }

                if (pauseMethod == PauseMethods.NoPauseMenu)
                    _inventoryOpen = true;
            }
            else
            {
                UnPause(pauseMethod);
                CorgiEngineEvent.Trigger(CorgiEngineEventTypes.UnPause);
            }

            LevelManager.Instance.ToggleCharacterPause();
        }

        public virtual void UnPause(PauseMethods pauseMethod = PauseMethods.PauseMenu)
        {
            MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Unfreeze, 1f, 0f, false, 0f, false);
            Instance.Paused = false;
            if ((GUIManager.HasInstance) && (pauseMethod == PauseMethods.PauseMenu))
            {
                GUIManager.Instance.SetPause(false);
                _pauseMenuOpen = false;
                SetActiveInventoryInputManager(true);
            }

            if (_inventoryOpen)
                _inventoryOpen = false;

            LevelManager.Instance.ToggleCharacterPause();
        }

        public virtual void ResetAllSaves()
        {
            MMSaveLoadManager.DeleteSaveFolder("InventoryEngine");
            MMSaveLoadManager.DeleteSaveFolder("CorgiEngine");
            MMSaveLoadManager.DeleteSaveFolder("MMAchievements");
            MMSaveLoadManager.DeleteSaveFolder("MMRetroAdventureProgress");
        }

        public virtual void StorePointsOfEntry(string levelName, int entryIndex, Character.FacingDirections facingDirection)
        {
            if (PointsOfEntry.Count > 0)
            {
                foreach (var point in PointsOfEntry.Where(point => point.LevelName == levelName))
                {
                    point.FacingDirection = facingDirection;
                    point.PointOfEntryIndex = entryIndex;
                    return;
                }
            }

            PointsOfEntry.Add(new PointsOfEntryStorage(levelName, entryIndex, facingDirection));
        }

        public virtual PointsOfEntryStorage GetPointsOfEntry(string levelName) => PointsOfEntry.Count <= 0 ? null : PointsOfEntry.FirstOrDefault(point => point.LevelName == levelName);

        public virtual void ClearPointOfEntry(string levelName)
        {
            if (PointsOfEntry.Count <= 0) return;
            foreach (var point in PointsOfEntry.Where(point => point.LevelName == levelName))
                PointsOfEntry.Remove(point);
        }

        public virtual void ClearAllPointsOfEntry()
        {
            PointsOfEntry.Clear();
        }

        public virtual void SetPersistentCharacter(Character newCharacter)
        {
            PersistentCharacter = newCharacter;
        }

        public virtual void DestroyPersistentCharacter()
        {
            if (PersistentCharacter)
            {
                Destroy(PersistentCharacter.gameObject);
                SetPersistentCharacter(null);
            }

            if (!LevelManager.Instance.Players[0]) return;
            if (LevelManager.Instance.Players[0].gameObject.MMGetComponentNoAlloc<CharacterPersistence>())
                Destroy(LevelManager.Instance.Players[0].gameObject);
        }

        public virtual void StoreSelectedCharacter(Character selectedCharacter)
        {
            StoredCharacter = selectedCharacter;
        }

        public virtual void ClearStoredCharacter()
        {
            StoredCharacter = null;
        }

        public virtual void OnMMEvent(MMGameEvent gameEvent)
        {
            switch (gameEvent.EventName)
            {
                case "inventoryOpens":
                    Pause(PauseMethods.NoPauseMenu);
                    break;

                case "inventoryCloses":
                    Pause(PauseMethods.NoPauseMenu);
                    break;
            }
        }

        public virtual void OnMMEvent(CorgiEngineEvent engineEvent)
        {
            switch (engineEvent.EventType)
            {
                case CorgiEngineEventTypes.TogglePause:
                    CorgiEngineEvent.Trigger(Paused ? CorgiEngineEventTypes.UnPause : CorgiEngineEventTypes.Pause);
                    break;

                case CorgiEngineEventTypes.Pause:
                    Pause();
                    break;

                case CorgiEngineEventTypes.UnPause:
                    UnPause();
                    break;

                case CorgiEngineEventTypes.PauseNoMenu:
                    Pause(PauseMethods.NoPauseMenu);
                    break;
            }
        }

        public virtual void OnMMEvent(CorgiEnginePointsEvent pointEvent)
        {
            switch (pointEvent.PointsMethod)
            {
                case PointsMethods.Set:
                    SetPoints(pointEvent.Points);
                    break;

                case PointsMethods.Add:
                    AddPoints(pointEvent.Points);
                    break;
            }
        }

        protected virtual void OnEnable()
        {
            this.MMEventStartListening<MMGameEvent>();
            this.MMEventStartListening<CorgiEngineEvent>();
            this.MMEventStartListening<CorgiEnginePointsEvent>();
            Cursor.visible = true;
        }

        protected virtual void OnDisable()
        {
            this.MMEventStopListening<MMGameEvent>();
            this.MMEventStopListening<CorgiEngineEvent>();
            this.MMEventStopListening<CorgiEnginePointsEvent>();
        }
    }
}