using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(SLM_BH_EventUI))]
[CanEditMultipleObjects]
public class SLM_BH_UIEditor : Editor
{
    SerializedProperty checkMode;
    SerializedProperty viewMode;
    SerializedProperty renderMode;
    SerializedProperty findMode;

    SerializedProperty mainobj;
    SerializedProperty image;
    SerializedProperty sprite;
    SerializedProperty time;
    SerializedProperty tag;
    SerializedProperty controll;
    SerializedProperty player;
    SerializedProperty enemy;

    SerializedProperty weaponSprites;
    SerializedProperty preseedShiftSpriteOn;
    SerializedProperty preseedShiftSpriteOff;
    SerializedProperty enemyHardAISprites;

    void OnEnable()
    {
        checkMode = serializedObject.FindProperty("checkMode");
        viewMode = serializedObject.FindProperty("viewMode");
        renderMode = serializedObject.FindProperty("renderMode");
        findMode = serializedObject.FindProperty("findMode");

        mainobj = serializedObject.FindProperty("mainobj");
        image = serializedObject.FindProperty("image");
        sprite = serializedObject.FindProperty("sprite");
        time = serializedObject.FindProperty("time");
        tag = serializedObject.FindProperty("tag");
        controll = serializedObject.FindProperty("controll");
        player = serializedObject.FindProperty("player");
        enemy = serializedObject.FindProperty("enemy");

        weaponSprites = serializedObject.FindProperty("weaponSprites");
        preseedShiftSpriteOn = serializedObject.FindProperty("preseedShiftSpriteOn");
        preseedShiftSpriteOff = serializedObject.FindProperty("preseedShiftSpriteOff");
        enemyHardAISprites = serializedObject.FindProperty("enemyHardAISprites");
    }

    public override void OnInspectorGUI()
    {
        SLM_BH_EventUI tg = (SLM_BH_EventUI)target;

        EditorGUILayout.PropertyField(checkMode);
        EditorGUILayout.PropertyField(viewMode);
        EditorGUILayout.PropertyField(renderMode);
        EditorGUILayout.PropertyField(findMode);

        if (tg.viewMode == SLM_BH_EventUI.viewM.enableByTime)
        {
            EditorGUILayout.PropertyField(mainobj);
            EditorGUILayout.PropertyField(time);
        }
        else if (tg.viewMode == SLM_BH_EventUI.viewM.onlyByVoid)
        {
            EditorGUILayout.PropertyField(mainobj);
        }

        if (tg.renderMode==SLM_BH_EventUI.renderM.image)
            EditorGUILayout.PropertyField(image);
        else if (tg.renderMode == SLM_BH_EventUI.renderM.sprite)
            EditorGUILayout.PropertyField(sprite);

        if (tg.checkMode==SLM_BH_EventUI.checkM.weapon)
		{
            if (tg.findMode == SLM_BH_EventUI.findM.select)
            {
                EditorGUILayout.PropertyField(player);
                if (tg.player != null && tg.weaponSprites.Count != tg.player.shoots.Count)
                    tg.weaponSprites = new List<Sprite>(tg.player.shoots.Count);
            }
            else if (tg.findMode == SLM_BH_EventUI.findM.tag)
                EditorGUILayout.PropertyField(tag);
            else if (tg.findMode == SLM_BH_EventUI.findM.fromControll)
                EditorGUILayout.PropertyField(controll);

            EditorGUILayout.PropertyField(weaponSprites);
        }
        else if (tg.checkMode == SLM_BH_EventUI.checkM.preseedShift)
        {
            if (tg.findMode == SLM_BH_EventUI.findM.select)
                EditorGUILayout.PropertyField(player);
            else if (tg.findMode == SLM_BH_EventUI.findM.tag)
                EditorGUILayout.PropertyField(tag);
            else if (tg.findMode == SLM_BH_EventUI.findM.fromControll)
                EditorGUILayout.PropertyField(controll);

            EditorGUILayout.PropertyField(preseedShiftSpriteOn);
            EditorGUILayout.PropertyField(preseedShiftSpriteOff);
        }
        else if(tg.checkMode == SLM_BH_EventUI.checkM.enemyHardAI)
        {
            if (tg.findMode == SLM_BH_EventUI.findM.select)
            {
                EditorGUILayout.PropertyField(enemy);
                if (tg.enemy != null && tg.enemyHardAISprites.Count != tg.enemy.hardAI.patterns.Count)
                    tg.enemyHardAISprites = new List<Sprite>(tg.enemy.hardAI.patterns.Count);
            }
            else if (tg.findMode == SLM_BH_EventUI.findM.tag)
                EditorGUILayout.PropertyField(tag);
            else if (tg.findMode == SLM_BH_EventUI.findM.fromControll)
                EditorGUILayout.LabelField("You can't get enemy from Controll, use tag or select");

            EditorGUILayout.PropertyField(enemyHardAISprites);
        }

        serializedObject.ApplyModifiedProperties();
    }

}
