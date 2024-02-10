using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerSetupUI : MonoBehaviour
{
    public TextMeshProUGUI Name;

    public void SetName(string msg)
    {
        Name.text = msg;
    }
}

[System.Serializable]
public class PlayerData
{
    public string Name;
}