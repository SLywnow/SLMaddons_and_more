using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using SLywnow;

[RequireComponent(typeof(RectTransform))]
public class SDia_Main : MonoBehaviour
{
    public UILineConnector lineObject;
    public RectTransform gostObject;
    public Transform mainParent;
    public Transform backgroundParent;
    public bool spawnOnCenter;
    [ShowFromBool(nameof(spawnOnCenter),false)]
    public Vector2 defaultSpawnPoint;
    public bool HideBlocksOutsideScreen = true;

    public List<SDia_Spawns> types;

    [HideInInspector] public List<SDia_Connections> conections;
    [HideInInspector] public List<SDia_Objects> blocks;
    [HideInInspector] public SDia_Joint currentJoint;

    RectTransform gost;
    UILineConnector gostline;

	private void Update()
	{
		if (currentJoint !=null)
		{
            if (gost == null)
			{
                gostline = Instantiate(lineObject, backgroundParent); //creating gost obects
                gostline.gameObject.SetActive(true);
                gost = Instantiate(gostObject, backgroundParent);
                gost.gameObject.SetActive(true);

                Vector2 pos = new Vector2(0,0); //set positions
                RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)backgroundParent, Input.mousePosition, Camera.current, out pos);
                gost.anchoredPosition = pos;
                gostline.transforms = new RectTransform[] { gost, (RectTransform)currentJoint.transform };
            }
            else
			{
                gost.position = Input.mousePosition;
            }
		}
        else
		{
            if (gost !=null)
			{
                Destroy(gostline.gameObject);
                Destroy(gost.gameObject);
			}
		}
	}

    public void SpawnNew(string type)
    {
        int nid = Random.Range(0, int.MaxValue);
        while (blocks.Find(b => b.id == nid) != null)
            nid = Random.Range(0, int.MaxValue);

        if (!spawnOnCenter)
            Spawn(type, defaultSpawnPoint, nid);
        else
        {
            RectTransform r = Instantiate(gostObject, backgroundParent);
            r.parent = mainParent;
            Vector2 position = r.anchoredPosition;
            Destroy(r.gameObject);

            Spawn(type, position, nid);
        }
    }

    public SDia_Block SpawnNew(string type, Vector2 position)
	{
        int nid = Random.Range(0, 2147483647);
        while (blocks.Find(b => b.id == nid) != null)
            nid = Random.Range(0, 2147483647);

        return Spawn(type, position, nid);
    }

    public SDia_Block Spawn(string type, Vector2 position, int id)
	{

        SDia_Block ret = null;

        if (types.Find(b => b.name == type) != null && blocks.Find(b => b.id == id) == null)
        {
            ret = Instantiate(types.Find(b => b.name == type).block, mainParent);
            ret.gameObject.SetActive(true);
            ret.main = this;
            ret.id = id;
            ((RectTransform)ret.transform).anchoredPosition = position;

            blocks.Add(new SDia_Objects());
            blocks[blocks.Count - 1].id = id;
            blocks[blocks.Count - 1].block = ret;
        }

        return ret;

    }

	public void UpdateConnections()
	{
	    foreach (SDia_Connections c in conections)
		{
            if (c.line == null)
            {
                c.line = Instantiate(lineObject, backgroundParent);
                c.line.gameObject.SetActive(true);
            }
            c.line.transforms = new RectTransform[] { c.p1, c.p2 };
        }
	}
}

[System.Serializable]
public class SDia_Spawns
{
    public string name;
    public SDia_Block block;
}

[System.Serializable]
public class SDia_Objects
{
    public int id;
    public SDia_Block block;
}

    [System.Serializable]
public class SDia_Connections
{
    public RectTransform p1;
    public RectTransform p2;
    public UILineConnector line;
}

