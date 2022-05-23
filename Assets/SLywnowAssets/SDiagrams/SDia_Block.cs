using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SLywnow;

[RequireComponent(typeof(CanvasGroup))]
public class SDia_Block : MonoBehaviour
{
	[HideInInspector] public SDia_Main main;
	public SDia_DragObject dragObject;
	public List<SDia_Joint> enters;
    public List<SDia_Joint> exits;
	public List<SDia_Block_Object> blockObjects;

	//[HideInInspector]
	public int id;
	[HideInInspector] public int type;

	CanvasGroup canvasGroup;

	public void Start()
	{
		if (dragObject != null)
			dragObject.main = GetComponent<RectTransform>();
		ProcessJoint();
		canvasGroup = GetComponent<CanvasGroup>();
	}

	private void Update()
	{
		if (main.HideBlocksOutsideScreen)
		{
			if (!RectTransormExtensions.IsVisible((RectTransform)transform))
			{
				if (canvasGroup.alpha != 0)
					canvasGroup.alpha = 0;
			}
			else
			{
				if (canvasGroup.alpha != 1)
					canvasGroup.alpha = 1;
			}
		}
	}

	public void ProcessJoint()
	{
		foreach (SDia_Joint j in enters)
		{
			if (!j.exit)
			{
				j.main = this;
				j.enter = true;
			}
		}

		foreach (SDia_Joint j in exits)
		{
			if (!j.enter)
			{
				j.main = this;
				j.exit = true;
			}
		}
	}


	public void SetConnection(bool enter, int jointId, int connectionId, int connectionJointId)
	{
		SDia_Block b = main.blocks.Find(f => f.id == connectionId).block;
		SDia_Joint j = null;
		if (enter && enters.Count > jointId && jointId >= 0)
			j = enters[jointId];
		else if (!enter && exits.Count > jointId && jointId >= 0)
			j = exits[jointId];

		if (b != null && j != null)
		{
			SDia_Joint j2 = null;
			if (enter && b.exits.Count > connectionJointId && connectionJointId >= 0)
				j2 = b.exits[connectionJointId];
			else if (!enter && b.enters.Count > connectionJointId && connectionJointId >= 0)
				j2 = b.enters[connectionJointId];

			if (j2 != null && j.connections.Count < j.connectionCount && j2.connections.Count < j2.connectionCount)
			{
				if (enter)
				{
					if (!j2.connections.Contains(id + " " + enters.IndexOf(j)))
					{
						j2.connections.Add(id + " " + enters.IndexOf(j));
						j.connections.Add(b.id + " " + b.exits.IndexOf(j2));
					}
					else
						return;
				}
				else
				{
					if (!j2.connections.Contains(id + " " + exits.IndexOf(j)))
					{
						j2.connections.Add(id + " " + exits.IndexOf(j));
						j.connections.Add(b.id + " " + b.enters.IndexOf(j2));
					}
					else
						return;
				}

				SDia_Connections c = new SDia_Connections();

				if (enter)
				{
					c.p1 = (RectTransform)j2.transform;
					c.p2 = (RectTransform)j.transform;
				}
				else
				{
					c.p1 = (RectTransform)j.transform;
					c.p2 = (RectTransform)j2.transform;
				}

				main.conections.Add(c);
				main.UpdateConnections();
			}
		}
	}

	public void Delete()
	{
		foreach (SDia_Joint j in enters)
		{
			j.RemoveAllConnections();
		}
		foreach (SDia_Joint j in exits)
		{
			j.RemoveAllConnections();
		}

		main.blocks.Remove(main.blocks.Find(b => b.id == id));
		Destroy(gameObject);
	}

	public GameObject getObject(string name)
	{
		if (blockObjects.Find((o) => o.name == name) != null)
			return blockObjects.Find((o) => o.name == name).obj;
		else
			return null;
	}

	public T getObjectComponent<T>(string name)
	{
		if (blockObjects.Find((o) => o.name == name) != null)
		{
			return blockObjects.Find((o) => o.name == name).obj.GetComponent<T>();
		}
		else
			return default(T);
	}
}

[System.Serializable]
public class SDia_Block_Object
{
	public string name;
	public GameObject obj;
}