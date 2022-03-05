using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class SLM_PC_Object : MonoBehaviour
{
    public int id;
    public bool HoldAsOne;
    public int count = -1;
    public bool pickable = true;
    public bool ignoreAmount;
    public bool resetChanceEveryCall;
    [Range(0, 100)] public int chanceToSpawn = 100;
    public string checkBool;
    public string pointWhenClick;
    [HideInInspector] public SLM_PC_Main main;
}
