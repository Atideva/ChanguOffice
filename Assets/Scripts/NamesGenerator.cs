using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NamesGenerator : MonoBehaviour
{
    public float generateRate = 10;
    public float generateTries = 10;

    public void GenerateNames(NamesList list, UI ui)
    {
        ClearNames(ui);
        StartCoroutine(Generate(list, ui));
    }

    public PlayerNames names = new();
    public float delayBetweenPlayers = 0.7f;

    IEnumerator Generate(NamesList list, UI ui)
    {
        yield return new WaitForSeconds(delayBetweenPlayers);
        for (var i = 0; i < generateTries; i++)
        {
            yield return new WaitForSeconds(1 / generateRate);
            names.name_1 = list.GetRandomName();
            Refresh(ui);
            if (i >= generateTries)
                list.TakeName(names.name_1);
        }


        yield return new WaitForSeconds(delayBetweenPlayers);
        for (var i = 0; i < generateTries; i++)
        {
            yield return new WaitForSeconds(1 / generateRate);
            names.name_2 = list.GetRandomName();
            Refresh(ui);
            if (i >= generateTries)
                list.TakeName(names.name_2);
        }


        yield return new WaitForSeconds(delayBetweenPlayers);
        for (var i = 0; i < generateTries; i++)
        {
            yield return new WaitForSeconds(1 / generateRate);
            names.name_3 = list.GetRandomName();
            Refresh(ui);
            if (i >= generateTries)
                list.TakeName(names.name_3);
        }


        yield return new WaitForSeconds(delayBetweenPlayers);
        for (var i = 0; i < generateTries; i++)
        {
            yield return new WaitForSeconds(1 / generateRate);
            names.name_4 = list.GetRandomName();
            Refresh(ui);
            if (i >= generateTries)
                list.TakeName(names.name_4);
        }
    }

    void ClearNames(UI ui)
    {
        names.name_1 = string.Empty;
        names.name_2 = string.Empty;
        names.name_3 = string.Empty;
        names.name_4 = string.Empty;
        Refresh(ui);
    }

    void Refresh(UI ui)
        => ui.SetPlayerNames(names);
}