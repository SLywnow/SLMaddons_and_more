using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SDia_DragObject : EventTrigger
{
	public RectTransform main;

	Vector3 startMpos;
	Vector2 statIpos;
	public override void OnBeginDrag(PointerEventData data)
	{
		startMpos = Input.mousePosition;
		statIpos = main.position;
	}

	public override void OnDrag(PointerEventData data)
	{
		main.position = statIpos - (Vector2)(startMpos - Input.mousePosition);
	}

	public override void OnEndDrag(PointerEventData data)
	{
		
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(SDia_DragObject))]
[CanEditMultipleObjects]
public class SDia_DragObject_Editor : Editor
{
	void OnEnable()
	{
		
	}

	public override void OnInspectorGUI()
	{
		
	}
}
#endif