using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NamesList : MonoBehaviour
{
    public List<string> tier_1;

    public string GetRandomName()
    {
        var r = Random.Range(0, tier_1.Count);
        return tier_1[r];
    }

    public void TakeName(string msg)
    {
        if (tier_1.Contains(msg))
            tier_1.Remove(msg);
    }
}
