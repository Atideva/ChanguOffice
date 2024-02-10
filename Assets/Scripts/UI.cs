using UnityEngine;

public class UI : MonoBehaviour
{
    public PlayerSetupUI playerSetup_1;
    public PlayerSetupUI playerSetup_2;
    public PlayerSetupUI playerSetup_3;
    public PlayerSetupUI playerSetup_4;

    public void SetPlayerNames(PlayerNames names)
    {
        playerSetup_1.SetName(names.name_1);
        playerSetup_2.SetName(names.name_2);
        playerSetup_3.SetName(names.name_3);
        playerSetup_4.SetName(names.name_4);
    }

    void Start()
    {
    }
}