using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SCutomHelp : EventTrigger
{
	public float delayBeforeInvoke;
	public UnityEvent PointerEnter;
	public UnityEvent PointerExit;

	float timer;
	bool timeCount;
	bool show;

	private void Update()
	{
		if (timeCount)
		{
			if (timer > 0) timer -= Time.deltaTime;
			else
			{
				timeCount = false;
				show = true;
				PointerEnter.Invoke();
			}
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (delayBeforeInvoke > 0)
		{
			timeCount = true;
			show = false;
			timer = delayBeforeInvoke;
		}
		else
		{
			show = true;
			PointerEnter.Invoke();
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		if (show)
			PointerExit.Invoke();

		show = false;
		timeCount = false;
		timer = 0;
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(SCutomHelp))]
[CanEditMultipleObjects]
public class SCutomHelp_Editor : Editor
{
	SerializedProperty delayBeforeInvoke;
	SerializedProperty PointerEnter;
	SerializedProperty PointerExit;

	void OnEnable()
	{
		delayBeforeInvoke = serializedObject.FindProperty("delayBeforeInvoke");
		PointerEnter = serializedObject.FindProperty("PointerEnter");
		PointerExit = serializedObject.FindProperty("PointerExit");

	}
	public override void OnInspectorGUI()
	{
		SCutomHelp tg = (SCutomHelp)target;

		EditorGUILayout.PropertyField(delayBeforeInvoke);
		EditorGUILayout.PropertyField(PointerEnter);
		EditorGUILayout.PropertyField(PointerExit);

		serializedObject.ApplyModifiedProperties();
	}
}
#endif