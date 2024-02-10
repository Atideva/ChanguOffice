using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGame : MonoBehaviour
{
    public NamesList nameList;
    public NamesGenerator nameGenerator;
    public UI ui;

    void Start()
    {
        nameGenerator.GenerateNames(nameList, ui);
    }
}