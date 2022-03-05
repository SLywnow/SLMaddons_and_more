using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

[CustomEditor(typeof(SLM_PC_Object))]

public class SLM_PC_ObjectEditor : Editor
{
    SerializedProperty id;
    SerializedProperty count;
    SerializedProperty pickAtOnce;
    SerializedProperty pickable;
    SerializedProperty ignoreAmount;
    SerializedProperty chanceToSpawn;
    SerializedProperty checkBool;
    SerializedProperty pointWhenClick;
    SerializedProperty resetChanceEveryCall;

    int oldid;
    int cid;

    void OnEnable()
    {
        id = serializedObject.FindProperty("id");
        count = serializedObject.FindProperty("count");
        pickAtOnce = serializedObject.FindProperty("HoldAsOne");
        pickable = serializedObject.FindProperty("pickable");
        ignoreAmount = serializedObject.FindProperty("ignoreAmount");
        chanceToSpawn = serializedObject.FindProperty("chanceToSpawn");
        checkBool = serializedObject.FindProperty("checkBool");
        pointWhenClick = serializedObject.FindProperty("pointWhenClick");
        resetChanceEveryCall = serializedObject.FindProperty("resetChanceEveryCall");
    }

    public override void OnInspectorGUI()
    {
        if (!Application.isPlaying)
        {
            SLM_PC_Object tg = (SLM_PC_Object)target;

            if (tg.main != null)
            {

                serializedObject.Update();
                if (tg.main.setup.objs.Count > 0)
                    EditorGUILayout.IntSlider(id, 0, tg.main.setup.objs.Count - 1, new GUIContent("ObjectId"));
                else
                    GUILayout.Label("Add some object first");

                if (tg.id != oldid)
                {
                    if (tg.main.setup.objs[tg.id].spriteId >= 0 && tg.main.setup.objs[tg.id].spriteId < tg.main.sprites.Count)
                    {
                        tg.GetComponent<Image>().sprite = tg.main.sprites[tg.main.setup.objs[tg.id].spriteId];
                        oldid = tg.id;
                    }
                    else
                        GUILayout.Label("Object found, but sprite not exist");
                }
                else
                    GUILayout.Label("Object not found");

                EditorGUILayout.PropertyField(pickAtOnce);
                EditorGUILayout.PropertyField(count);
                EditorGUILayout.PropertyField(pickable);
                EditorGUILayout.PropertyField(ignoreAmount);
                EditorGUILayout.PropertyField(resetChanceEveryCall);
                EditorGUILayout.IntSlider(chanceToSpawn, 1, 100, new GUIContent("Spawn chance"));
                EditorGUILayout.PropertyField(checkBool);
                EditorGUILayout.PropertyField(pointWhenClick);
                serializedObject.ApplyModifiedProperties();
            }
            else
            {
                GUILayout.Label("Editing is available only after the object");
                GUILayout.Label("is automatically spawned via the SLM PC Editor");
                tg.gameObject.SetActive(false);
            }
        }
        else
        {
            GUILayout.Label("Editing is unavailable in a playing mode");
            GUILayout.Label("Info:");
            SLM_PC_Object tg = (SLM_PC_Object)target;
            GUILayout.Label("Count: " + tg.count + " Hold as one: " + tg.HoldAsOne);
            GUILayout.Label("Spawn chance: " + tg.chanceToSpawn + "%  Reset every call: " + tg.resetChanceEveryCall);
            GUILayout.Label("Pickable: " + tg.pickable.ToString() + " Ignore Amount: " + tg.ignoreAmount.ToString());
            GUILayout.Label("Bool string: " + tg.checkBool);
            GUILayout.Label("Point when clicking: " + tg.pointWhenClick);
        }
    }
}
