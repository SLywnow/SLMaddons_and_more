using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using AutoLangSLywnow;

[CustomEditor(typeof(SLM_PC_Main))]

public class SLM_PC_MainEditor : Editor
{
    float scale = 1;
    bool showall;
    bool def;

    void OnEnable()
    {

    }
    public override void OnInspectorGUI()
    {
        if (!Application.isPlaying)
            DrawDefaultInspector();
        else
        {
            SLM_PC_Main tg = (SLM_PC_Main)target;

            GUILayout.Label("Inventory");


            GUILayout.BeginVertical();
            for (int i=0;i<tg.inventory.Count;i++)
			{
                if (tg.inventory[i]>0 || showall)
				{
                    Texture tx = null;
                    if (tg.setup.objs[i].spriteId >= 0 && tg.setup.objs[i].spriteId < tg.sprites.Count)
                        tx = tg.sprites[tg.setup.objs[i].spriteId].texture;

                    string ingamename = "";
                    if (tg.C.ALSLBridge!=null)
					{
                        tg.C.ALSLBridge.GetString(tg.setup.objs[i].alsl_name);
                    }
                    else
					{
                        ALSL_Main.GetWorldAndFindKeys(tg.setup.objs[i].alsl_name);
					}

                    GUILayout.Label(new GUIContent(i + " " + ingamename + " (" + tg.setup.objs[i].name + ") Count: " + tg.inventory[i], tx), GUILayout.Height(60 * scale));
                }
                EditorGUILayout.Space();
			}

            GUILayout.EndVertical();

            scale = EditorGUILayout.Slider("Size", scale, 0.1f, 2);
            showall = EditorGUILayout.Toggle("Show all items", showall);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            def = EditorGUILayout.Toggle("Show settings", def);
            if (def)
                DrawDefaultInspector();
        }
    }
}
