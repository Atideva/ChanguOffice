using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;
using UnityEngine.Serialization;

namespace MoreMountains.CorgiEngine
{
    [AddComponentMenu("Corgi Engine/Managers/Level Manager")]
    public class LevelManager : MMSingleton<LevelManager>, MMEventListener<CorgiEngineEvent>
    {
        public Character[] PlayerPrefabs;
        public bool AutoAttributePlayerIDs = true;

        public List<Character> SceneCharacters;

        [Header("Checkpoints")]
        public CheckPoint DebugSpawn;
        public CheckpointsAxis CheckpointAttributionAxis = CheckpointsAxis.x;
        public CheckpointDirections CheckpointAttributionDirection = CheckpointDirections.Ascending;
        [MMReadOnly] public CheckPoint CurrentCheckPoint;

        [Space(10)]
        public List<PointOfEntry> PointsOfEntry;

        [Space(10)]
        [Header("Fade")]
        public float IntroFadeDuration = 1f;
        public float OutroFadeDuration = 1f;
        public int FaderID = 0;
        public MMTweenType FadeTween = new(MMTween.MMTweenCurve.EaseInOutCubic);
        public float RespawnDelay = 2f;
        public bool ResetPointsOnRestart = true;

        [Space(10)]
        [Header("Level Bounds")]
        public BoundsModes BoundsMode = BoundsModes.ThreeD;
        [Tooltip("the level limits, camera and player won't go beyond this point.")]
        public Bounds LevelBounds = new Bounds(Vector3.zero, Vector3.one * 10);

        [MMInspectorButton("GenerateColliderBounds")]
        public bool ConvertToColliderBoundsButton;
        public Collider BoundsCollider { get; protected set; }
        public Collider2D BoundsCollider2D { get; protected set; }

        [Header("Scene Loading")]
        public MMLoadScene.LoadingSceneModes LoadingSceneMode = MMLoadScene.LoadingSceneModes.MMSceneLoadingManager;
        [MMEnumCondition("LoadingSceneMode", (int)MMLoadScene.LoadingSceneModes.MMSceneLoadingManager)]
        public string LoadingSceneName = "LoadingScreen";
        [MMEnumCondition("LoadingSceneMode", (int)MMLoadScene.LoadingSceneModes.MMAdditiveSceneLoadingManager)]
        public MMAdditiveSceneLoadingManagerSettings AdditiveLoadingSettings;

        [Header("Feedbacks")]
        public bool SetPlayerAsFeedbackRangeCenter = false;

        public TimeSpan RunningTime { get { return DateTime.UtcNow - _started; } }
        public CameraController LevelCameraController { get; set; }

        public List<Character> Players { get; protected set; }
        public List<CheckPoint> Checkpoints { get; protected set; }
        protected DateTime _started;
        protected int _savedPoints;
        protected string _nextLevel = null;
        protected BoxCollider _collider;
        protected BoxCollider2D _collider2D;
        protected Bounds _originalBounds;

        protected override void Awake()
        {
            base.Awake();
            _originalBounds = LevelBounds;
        }

        protected virtual void InstantiatePlayableCharacters()
        {
            Players = new List<Character>();

            if (GameManager.Instance.PersistentCharacter != null)
            {
                Players.Add(GameManager.Instance.PersistentCharacter);
                return;
            }

            if (GameManager.Instance.StoredCharacter != null)
            {
                var newPlayer = (Character)Instantiate(GameManager.Instance.StoredCharacter, new Vector3(0, 0, 0), Quaternion.identity);
                newPlayer.name = GameManager.Instance.StoredCharacter.name;
                Players.Add(newPlayer);
                return;
            }

            if ((SceneCharacters != null) && (SceneCharacters.Count > 0))
            {
                foreach (var character in SceneCharacters)
                {
                    Players.Add(character);
                }

                return;
            }

            if (PlayerPrefabs == null)
                return;

            // player instantiation
            if (PlayerPrefabs.Count() != 0)
            {
                foreach (var playerPrefab in PlayerPrefabs)
                {
                    var newPlayer = (Character)Instantiate(playerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                    newPlayer.name = playerPrefab.name;
                    Players.Add(newPlayer);

                    if (playerPrefab.CharacterType != Character.CharacterTypes.Player)
                    {
                        Debug.LogWarning("LevelManager : The Character you've set in the LevelManager isn't a Player, which means it's probably not going to move. You can change that in the Character component of your prefab.");
                    }
                }
            }
            else
            {
                //Debug.LogWarning ("LevelManager : The Level Manager doesn't have any Player prefab to spawn. You need to select a Player prefab from its inspector.");
                return;
            }
        }

        public virtual void Start()
        {
            InstantiatePlayableCharacters();
            if (Players == null || Players.Count == 0)
                return;

            Initialization();


            CorgiEngineEvent.Trigger(CorgiEngineEventTypes.SpawnCharacterStarts);

            // we handle the spawn of the character(s)
            if (Players.Count == 1)
                SpawnSingleCharacter();
            else
                SpawnMultipleCharacters();

            LevelGUIStart();
            CheckpointAssignment();

            CorgiEngineEvent.Trigger(CorgiEngineEventTypes.LevelStart, Players[0]);
            MMGameEvent.Trigger("Load");

            if (SetPlayerAsFeedbackRangeCenter)
            {
                MMSetFeedbackRangeCenterEvent.Trigger(Players[0].transform);
            }

            MMCameraEvent.Trigger(MMCameraEventTypes.SetConfiner, null, BoundsCollider, BoundsCollider2D);
            MMCameraEvent.Trigger(MMCameraEventTypes.SetTargetCharacter, Players[0]);
            MMCameraEvent.Trigger(MMCameraEventTypes.StartFollowing);
        }

        protected virtual void Initialization()
        {
            LevelCameraController = FindObjectOfType<CameraController>();
            _savedPoints = GameManager.Instance.Points;
            _started = DateTime.UtcNow;
            GenerateColliderBounds();

            switch (CheckpointAttributionAxis)
            {
                case CheckpointsAxis.x:
                    Checkpoints = CheckpointAttributionDirection == CheckpointDirections.Ascending ? FindObjectsOfType<CheckPoint>().OrderBy(o => o.transform.position.x).ToList() : FindObjectsOfType<CheckPoint>().OrderByDescending(o => o.transform.position.x).ToList();

                    break;
                case CheckpointsAxis.y:
                    Checkpoints = CheckpointAttributionDirection == CheckpointDirections.Ascending ? FindObjectsOfType<CheckPoint>().OrderBy(o => o.transform.position.y).ToList() : FindObjectsOfType<CheckPoint>().OrderByDescending(o => o.transform.position.y).ToList();

                    break;
                case CheckpointsAxis.z:
                    Checkpoints = CheckpointAttributionDirection == CheckpointDirections.Ascending ? FindObjectsOfType<CheckPoint>().OrderBy(o => o.transform.position.z).ToList() : FindObjectsOfType<CheckPoint>().OrderByDescending(o => o.transform.position.z).ToList();

                    break;
                case CheckpointsAxis.CheckpointOrder:
                    Checkpoints = FindObjectsOfType<CheckPoint>().OrderBy(o => o.CheckPointOrder).ToList();
                    break;
            }

            // we assign the first checkpoint
            CurrentCheckPoint = Checkpoints.Count > 0 ? Checkpoints[0] : null;
        }

        protected virtual void CheckpointAssignment()
        {
            var listeners = FindObjectsOfType<MonoBehaviour>(true).OfType<Respawnable>();
            foreach (var listener in listeners)
            {
                for (var i = Checkpoints.Count - 1; i >= 0; i--)
                {
                    var autoRespawn = (listener as MonoBehaviour).GetComponent<AutoRespawn>();
                    if (autoRespawn != null)
                    {
                        if (autoRespawn.IgnoreCheckpointsAlwaysRespawn)
                        {
                            Checkpoints[i].AssignObjectToCheckPoint(listener);
                            continue;
                        }

                        if (autoRespawn.AssociatedCheckpoints.Contains(Checkpoints[i]))
                        {
                            Checkpoints[i].AssignObjectToCheckPoint(listener);
                            continue;
                        }

                        continue;
                    }

                    var vectorDistance = ((MonoBehaviour)listener).transform.position - Checkpoints[i].transform.position;

                    float distance = CheckpointAttributionAxis switch
                    {
                        CheckpointsAxis.x => vectorDistance.x,
                        CheckpointsAxis.y => vectorDistance.y,
                        CheckpointsAxis.z => vectorDistance.z,
                        _ => 0
                    };

                    switch (distance)
                    {
                        case < 0 when (CheckpointAttributionDirection == CheckpointDirections.Ascending):
                        case > 0 when (CheckpointAttributionDirection == CheckpointDirections.Descending):
                            continue;
                    }

                    Checkpoints[i].AssignObjectToCheckPoint(listener);
                    break;
                }
            }
        }

        protected virtual void LevelGUIStart()
        {
            LevelNameEvent.Trigger(SceneManager.GetActiveScene().name);
            MMFadeOutEvent.Trigger(IntroFadeDuration, FadeTween, FaderID, false, Players.Count > 0 ? Players[0].transform.position : Vector3.zero);
        }

        protected virtual void SpawnSingleCharacter()
        {
#if UNITY_EDITOR
            if (DebugSpawn != null)
            {
                DebugSpawn.SpawnPlayer(Players[0]);
                return;
            }
            else
            {
                RegularSpawnSingleCharacter();
            }
#else
				RegularSpawnSingleCharacter();
#endif
        }

        protected virtual void RegularSpawnSingleCharacter()
        {
            var point = GameManager.Instance.GetPointsOfEntry(SceneManager.GetActiveScene().name);
            if ((point != null) && (PointsOfEntry.Count >= (point.PointOfEntryIndex + 1)))
            {
                Players[0].RespawnAt(PointsOfEntry[point.PointOfEntryIndex].Position, point.FacingDirection);
                PointsOfEntry[point.PointOfEntryIndex].EntryFeedback?.PlayFeedbacks();
                return;
            }

            if (CurrentCheckPoint == null) return;
            CurrentCheckPoint.SpawnPlayer(Players[0]);
        }

        protected virtual void SpawnMultipleCharacters()
        {
            var c = 0;
            var id = 0;
            foreach (var player in Players)
            {
                var spawned = false;
                if (AutoAttributePlayerIDs)
                    player.SetPlayerID("Player" + (id - 1), id);
                
                player.name += " - " + player.PlayerID;

                if (Checkpoints.Count > c + 1 && Checkpoints[c])
                {
                    Checkpoints[c].SpawnPlayer(player);
                    id++;
                    spawned = true;
                    c++;
                }

                if (spawned) continue;
                Checkpoints[c].SpawnPlayer(player);
                id++;
            }
        }

        public virtual void SetCurrentCheckpoint(CheckPoint newCheckPoint)
        {
            if (newCheckPoint.ForceAssignation)
            {
                CurrentCheckPoint = newCheckPoint;
                return;
            }

            if (CurrentCheckPoint == null)
            {
                CurrentCheckPoint = newCheckPoint;
                return;
            }

            if (newCheckPoint.CheckPointOrder >= CurrentCheckPoint.CheckPointOrder)
            {
                CurrentCheckPoint = newCheckPoint;
            }
        }

        public virtual void SetNextLevel(string levelName)
        {
            _nextLevel = levelName;
        }

        public virtual void GotoNextLevel()
        {
            GotoLevel(_nextLevel);
            _nextLevel = null;
        }

        public virtual void GotoLevel(string levelName, bool fadeOut = true, bool save = true)
        {
            CorgiEngineEvent.Trigger(CorgiEngineEventTypes.LevelEnd);
            if (save)
                MMGameEvent.Trigger("Save");

            if (fadeOut)
                MMFadeInEvent.Trigger(OutroFadeDuration, FadeTween, FaderID, true, Players is { Count: > 0 } ? Players[0].transform.position : Vector3.zero);

            StartCoroutine(GotoLevelCo(levelName, fadeOut));
        }

        protected virtual IEnumerator GotoLevelCo(string levelName, bool fadeOut = true)
        {
            if (Players != null && Players.Count > 0)
            {
                foreach (var player in Players)
                    player.Disable();
            }

            if (fadeOut)
            {
                if (Time.timeScale > 0.0f)
                    yield return new WaitForSeconds(OutroFadeDuration);
                else
                    yield return new WaitForSecondsRealtime(OutroFadeDuration);
            }

            CorgiEngineEvent.Trigger(CorgiEngineEventTypes.UnPause);
            CorgiEngineEvent.Trigger(CorgiEngineEventTypes.LoadNextScene);

            var destinationScene = (string.IsNullOrEmpty(levelName)) ? "StartScreen" : levelName;
            LoadScene(destinationScene);
        }

        protected virtual void LoadScene(string destinationScene)
        {
            switch (LoadingSceneMode)
            {
                case MMLoadScene.LoadingSceneModes.UnityNative:
                    SceneManager.LoadScene(destinationScene);
                    break;
                case MMLoadScene.LoadingSceneModes.MMSceneLoadingManager:
                    MMSceneLoadingManager.LoadScene(destinationScene, LoadingSceneName);
                    break;
                case MMLoadScene.LoadingSceneModes.MMAdditiveSceneLoadingManager:
                    MMAdditiveSceneLoadingManager.LoadScene(destinationScene, AdditiveLoadingSettings);
                    break;
            }
        }

        public virtual void PlayerDead(Character player)
        {
            var characterHealth = player.GetComponent<Health>();
            if (!characterHealth) return;

            if (GameManager.Instance.MaximumLives > 0)
            {
                GameManager.Instance.LoseLife();
                if (GameManager.Instance.CurrentLives <= 0)
                {
                    Cleanup();
                    CorgiEngineEvent.Trigger(CorgiEngineEventTypes.GameOver);
                    if (!string.IsNullOrEmpty(GameManager.Instance.GameOverScene))
                        LoadScene(GameManager.Instance.GameOverScene);
                }
            }

            if (Players.Count < 2)
                StartCoroutine(SoloModeRestart());
        }

        protected virtual void Cleanup()
        {
            if (GameManager.Instance.ResetLivesOnGameOver)
                GameManager.Instance.ResetLives();

            if (GameManager.Instance.ResetPersistentCharacterOnGameOver)
                GameManager.Instance.DestroyPersistentCharacter();

            if (GameManager.Instance.ResetStoredCharacterOnGameOver)
                GameManager.Instance.ClearStoredCharacter();
        }

        protected virtual IEnumerator SoloModeRestart()
        {
            MMCameraEvent.Trigger(MMCameraEventTypes.StopFollowing);
            yield return new WaitForSeconds(RespawnDelay);
            MMCameraEvent.Trigger(MMCameraEventTypes.StartFollowing);

            if (CurrentCheckPoint)
                CurrentCheckPoint.SpawnPlayer(Players[0]);

            _started = DateTime.UtcNow;

            if (ResetPointsOnRestart)
                CorgiEnginePointsEvent.Trigger(PointsMethods.Set, 0);

            ResetLevelBoundsToOriginalBounds();
            CorgiEngineEvent.Trigger(CorgiEngineEventTypes.Respawn, Players[0]);
        }

        public virtual void FreezeCharacters()
        {
            foreach (var player in Players)
            {
                player.Freeze();
            }
        }

        public virtual void UnFreezeCharacters()
        {
            foreach (var player in Players)
            {
                player.UnFreeze();
            }
        }

        public virtual void ToggleCharacterPause()
        {
            foreach (var player in Players)
            {
                var characterPause = player?.FindAbility<CharacterPause>();
                if (characterPause == null)
                {
                    break;
                }

                if (GameManager.Instance.Paused)
                {
                    characterPause.PauseCharacter();
                }
                else
                {
                    characterPause.UnPauseCharacter();
                }
            }
        }

        public virtual void ResetLevelBoundsToOriginalBounds()
        {
            SetNewLevelBounds(_originalBounds);
        }

        public virtual void SetNewMinLevelBounds(Vector3 newMinBounds)
        {
            LevelBounds.min = newMinBounds;
            UpdateBoundsCollider();
        }

        public virtual void SetNewMaxLevelBounds(Vector3 newMaxBounds)
        {
            LevelBounds.max = newMaxBounds;
            UpdateBoundsCollider();
        }

        public virtual void SetNewLevelBounds(Bounds newBounds)
        {
            LevelBounds = newBounds;
            UpdateBoundsCollider();
        }

        protected virtual void UpdateBoundsCollider()
        {
            if (_collider != null)
            {
                transform.position = LevelBounds.center;
                _collider.size = LevelBounds.extents * 2f;
            }
        }

        [ExecuteAlways]
        protected virtual void GenerateColliderBounds()
        {
            BoundsCollider = gameObject.GetComponent<Collider>();
            BoundsCollider2D = gameObject.GetComponent<CompositeCollider2D>();

            if ((BoundsCollider == null) && (BoundsCollider2D == null))
            {
                // set transform
                transform.position = LevelBounds.center;
            }

            if ((BoundsCollider == null) && (BoundsMode == BoundsModes.ThreeD))
            {
                if (gameObject.GetComponent<BoxCollider>() != null)
                {
                    DestroyImmediate(gameObject.GetComponent<BoxCollider>());
                }

                _collider = gameObject.AddComponent<BoxCollider>();
                _collider.size = LevelBounds.extents * 2f;
                gameObject.layer = LayerMask.NameToLayer("NoCollision");
            }

            if ((BoundsCollider2D == null) && (BoundsMode == BoundsModes.TwoD))
            {
                if (gameObject.GetComponent<BoxCollider2D>() != null)
                {
                    DestroyImmediate(gameObject.GetComponent<BoxCollider2D>());
                }

                var rb = gameObject.AddComponent<Rigidbody2D>();
                rb.isKinematic = true;
                rb.simulated = false;

                _collider2D = gameObject.AddComponent<BoxCollider2D>();
                _collider2D.size = LevelBounds.extents * 2f;
                _collider2D.usedByComposite = true;

                gameObject.layer = LayerMask.NameToLayer("NoCollision");

                var composite = gameObject.AddComponent<CompositeCollider2D>();
                composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
            }

            BoundsCollider = gameObject.GetComponent<Collider>();
            BoundsCollider2D = gameObject.GetComponent<CompositeCollider2D>();
        }

        public virtual void OnMMEvent(CorgiEngineEvent engineEvent)
        {
            switch (engineEvent.EventType)
            {
                case CorgiEngineEventTypes.PlayerDeath:
                    PlayerDead(engineEvent.OriginCharacter);
                    break;
            }
        }

        protected virtual void OnEnable()
        {
            this.MMEventStartListening();
        }

        protected virtual void OnDisable()
        {
            this.MMEventStopListening();
        }
    }
}