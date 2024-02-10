using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
    [AddComponentMenu("Corgi Engine/Character/Damage/Kill Player on Touch")]
    public class KillPlayerOnTouch : CorgiMonoBehaviour
    {
        protected virtual void OnTriggerEnter2D(Collider2D collider)
        {
            var character = collider.GetComponent<Character>();

            if (character == null) return;
            if (character.CharacterType != Character.CharacterTypes.Player) return;

            if (character.ConditionState.CurrentState != CharacterStates.CharacterConditions.Dead)
                character.CharacterHealth.Kill();
        }
    }
}