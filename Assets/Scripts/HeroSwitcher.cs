using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using UnityEngine;

public class HeroSwitcher : MonoBehaviour
{
    public Character[] heroes;
    [MMReadOnly] public CharacterSwitchManager[] switchers;

    void Start()
    {
        switchers = GetComponentsInChildren<CharacterSwitchManager>();
        foreach (var switcher in switchers)
        {
            switcher.CharacterPrefabs = heroes;
            switcher.Init();
        }
    }
}