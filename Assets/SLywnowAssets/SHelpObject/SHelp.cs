using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SHelp : EventTrigger
{
	public bool active=true;
	public string text;
	public float sizeY = 40;
	public Color textColor = Color.black;
	public Color bgColor = Color.white;
	public float delayBeforeShow;
	public float delayBeforeHide=-1;
	public Vector2 offset;
	public enum tpe { topLeft, topCenter, topRight, Custom};
	public tpe posType = tpe.topCenter;
	public Vector2 anchorMin = new Vector2(0.5f, 1);
	public Vector2 anchorMax = new Vector2(0.5f, 1);
	public enum pivotP { Left, Right, Center, Custom};
	public pivotP pivotPositon = pivotP.Center;
	public Vector2 pivot = new Vector2(0.5f, 0.5f);
	public KeyCode checkKey;

	float timer=0;
	float dietimer=-1;
	bool show;
	GameObject obj;

	private void Update()
	{
		if (show && obj == null)
		{
			if (checkKey == KeyCode.None || Input.GetKey(checkKey))
			{
				if (timer < delayBeforeShow)
					timer += Time.deltaTime;
				else
				{
					SpawnObject();
				}
			}
		}

		if (checkKey != KeyCode.None && Input.GetKeyUp(checkKey) && obj != null)
		{
			obj.SetActive(false);
		}
		else if (checkKey != KeyCode.None && Input.GetKeyDown(checkKey) && obj != null)
		{
			obj.SetActive(true);
		}

		if (dietimer != -1)
			if (dietimer > 0)
			{
				dietimer -= Time.deltaTime;
			}
			else
			{
				dietimer = -1;
				DeleteObject();
			}
	}

	private void OnDestroy()
	{
		DeleteObject();
	}

	void SpawnObject()
	{
		obj = Instantiate(Resources.Load("SHelp Object") as GameObject, transform);
		((RectTransform)obj.transform).sizeDelta = new Vector2(0.6f * text.Length * sizeY, sizeY);

		Vector2 mainOffset = new Vector2(0, sizeY / 2);
		if (posType == tpe.topCenter)
		{
			anchorMin = new Vector2(0.5f, 1);
			anchorMax = new Vector2(0.5f, 1);
		}
		else if (posType == tpe.topLeft)
		{
			anchorMin = new Vector2(0, 1);
			anchorMax = new Vector2(0, 1);
		}
		else if (posType == tpe.topRight)
		{
			anchorMin = new Vector2(1, 1);
			anchorMax = new Vector2(1, 1);
		}
		else if (posType == tpe.Custom)
			mainOffset = new Vector2(0, 0);

		if (pivotPositon == pivotP.Center)
			pivot = new Vector2(0.5f, 0.5f);
		else if (pivotPositon == pivotP.Left)
			pivot = new Vector2(0, 0.5f);
		else if (pivotPositon == pivotP.Right)
			pivot = new Vector2(1, 0.5f);

		((RectTransform)obj.transform).anchorMin = anchorMin;
		((RectTransform)obj.transform).anchorMax = anchorMax;
		((RectTransform)obj.transform).pivot = pivot;
		((RectTransform)obj.transform).anchoredPosition = mainOffset + offset;
		obj.GetComponent<Image>().color = bgColor;
		obj.GetComponentInChildren<Text>().text = text;
		obj.GetComponentInChildren<Text>().color = textColor;
		int max = 10;
		Transform canvpar=null;
		Transform curpar=transform.parent;
		while (canvpar==null && max>0)
		{
			if (curpar != null)
			{
				if (curpar.GetComponent<Canvas>() != null)
				{
					canvpar = curpar;
					max = 0;
				}
				else
				{
					curpar = curpar.parent;
				}
				max--;
			}
			else
				max = 0;
		}

		if (delayBeforeHide > 0)
			dietimer = delayBeforeHide;

		if (canvpar != null)
			obj.transform.SetParent(canvpar);
	}

	void DeleteObject()
	{
		if (obj != null)
			Destroy(obj);
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (active)
		{
			show = true;
			timer = 0;
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		show = false;
		DeleteObject();
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(SHelp))]
[CanEditMultipleObjects]
public class SHelp_Editor : Editor
{
	SerializedProperty text;
	SerializedProperty sizeY;
	SerializedProperty textColor;
	SerializedProperty bgColor;
	SerializedProperty delayBeforeShow;
	SerializedProperty delayBeforeHide;
	SerializedProperty posType;
	SerializedProperty offset;
	SerializedProperty anchorMin;
	SerializedProperty anchorMax;
	SerializedProperty pivotPositon;
	SerializedProperty pivot;
	SerializedProperty checkKey;

	void OnEnable()
	{
		text = serializedObject.FindProperty("text");
		sizeY = serializedObject.FindProperty("sizeY");
		textColor = serializedObject.FindProperty("textColor");
		bgColor = serializedObject.FindProperty("bgColor");
		delayBeforeShow = serializedObject.FindProperty("delayBeforeShow");
		delayBeforeHide = serializedObject.FindProperty("delayBeforeHide");
		posType = serializedObject.FindProperty("posType");
		offset = serializedObject.FindProperty("offset");
		anchorMin = serializedObject.FindProperty("anchorMin");
		anchorMax = serializedObject.FindProperty("anchorMax");
		pivotPositon = serializedObject.FindProperty("pivotPositon");
		pivot = serializedObject.FindProperty("pivot");
		checkKey = serializedObject.FindProperty("checkKey");
	}

	public override void OnInspectorGUI()
	{
		SHelp tg = (SHelp)target;

		EditorGUILayout.PropertyField(text);
		EditorGUILayout.PropertyField(sizeY);
		EditorGUILayout.PropertyField(textColor);
		EditorGUILayout.PropertyField(bgColor);
		EditorGUILayout.PropertyField(delayBeforeShow);
		EditorGUILayout.PropertyField(delayBeforeHide);
		EditorGUILayout.PropertyField(posType);
		if (tg.posType == SHelp.tpe.Custom)
		{
			EditorGUILayout.PropertyField(anchorMin);
			EditorGUILayout.PropertyField(anchorMax);
		}
		EditorGUILayout.PropertyField(pivotPositon);
		if (tg.pivotPositon==SHelp.pivotP.Custom)
			EditorGUILayout.PropertyField(pivot);

		EditorGUILayout.PropertyField(offset);
		EditorGUILayout.PropertyField(checkKey);
		serializedObject.ApplyModifiedProperties();
	}
}
#endif