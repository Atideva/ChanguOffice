using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
    [AddComponentMenu("Corgi Engine/Managers/Multiplayer Level Manager")]
    public class MultiplayerLevelManager : LevelManager
    {
        protected virtual void CheckMultiplayerEndGame()
        {
            var stillAlive = 0;
            var winnerID = "";
            foreach (var player in Players.Where(player
                         => player.ConditionState.CurrentState != CharacterStates.CharacterConditions.Dead))
            {
                stillAlive++;
                winnerID = player.PlayerID;
            }

            if (stillAlive == 1)
                StartCoroutine(MultiplayerEndGame(winnerID));
        }

        protected virtual IEnumerator MultiplayerEndGame(string winnerID)
        {
            yield return new WaitForSeconds(1f);

            FreezeCharacters();
            yield return new WaitForSeconds(1f);

            if (GUIManager.Instance.GetComponent<MultiplayerGUIManager>() != null)
            {
                GUIManager.Instance.GetComponent<MultiplayerGUIManager>().ShowMultiplayerEndgame();
                GUIManager.Instance.GetComponent<MultiplayerGUIManager>().SetMultiplayerEndgameText(winnerID + " WINS");
            }

            yield return new WaitForSeconds(2f);
            MMSceneLoadingManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public bool allowRespawn;

        public override void PlayerDead(Character player)
        {
            var characterHealth = player.GetComponent<Health>();
            if (!characterHealth) return;

            if (allowRespawn)
            {
                StartCoroutine(SoloModeRestart(player));
            }
            else
            {
                StartCoroutine(RemovePlayer(player));
                CheckMultiplayerEndGame();
            }
        }

        public List<CheckPoint> respawnCheckPoints;

        IEnumerator SoloModeRestart(Character player)
        {
            MMCameraEvent.Trigger(MMCameraEventTypes.StopFollowing);
            yield return new WaitForSeconds(RespawnDelay);
            MMCameraEvent.Trigger(MMCameraEventTypes.StartFollowing);

            var id = player.ID;
            respawnCheckPoints[id].SpawnPlayer(Players[id]);

            // _started = DateTime.UtcNow;
            // if (ResetPointsOnRestart)
            //     CorgiEnginePointsEvent.Trigger(PointsMethods.Set, 0);

           // ResetLevelBoundsToOriginalBounds();
            CorgiEngineEvent.Trigger(CorgiEngineEventTypes.Respawn, Players[id]);
        }

        protected virtual IEnumerator RemovePlayer(Character player)
        {
            yield return new WaitForSeconds(0.01f);
            Destroy(player.gameObject);
        }
    }
}