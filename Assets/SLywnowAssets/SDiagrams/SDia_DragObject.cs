using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SDia_DragObject : EventTrigger
{
	public RectTransform main;

	Vector3 startMpos;
	Vector2 statIpos;
	public override void OnBeginDrag(PointerEventData data)
	{
		startMpos = Input.mousePosition;
		statIpos = main.anchoredPosition;
	}

	public override void OnDrag(PointerEventData data)
	{
		main.anchoredPosition = statIpos - (Vector2)(startMpos - Input.mousePosition);
	}

	public override void OnEndDrag(PointerEventData data)
	{
		
	}
}