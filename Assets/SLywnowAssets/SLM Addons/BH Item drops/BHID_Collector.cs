using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BHID_Collector : MonoBehaviour
{
    public BHID_Main main;
    public string dropTag;

    private void OnTriggerEnter2D(Collider2D c)
    {
        if (c.GetComponent<BHID_Drop>() != null)
		{
            BHID_Drop d = c.GetComponent<BHID_Drop>();
            if (!d.getted)
            {
                main.GetDrop(d.type, d.count, gameObject.name);
                d.getted = true;
            }
            Destroy(d.gameObject);
        }
    }

    public void SetMeForceMagnet()
	{
        List<GameObject> objs = GameObject.FindGameObjectsWithTag(dropTag).ToList();

        foreach (GameObject obj in objs)
		{
            BHID_Drop d = obj.GetComponent<BHID_Drop>();
            if (d !=null)
			{
                d.magnetTarget = transform;
                d.magnet = true;
            }
		}
	}

    public void SetMeAsMainMagnet()
	{
        SetMeForceMagnet();

        main.magnet = transform;
        main.magnetTime = -1;
    }

    public void SetMeAsMainMagnetForTime(float time)
    {
        SetMeForceMagnet();

        main.magnet = transform;
        main.magnetTime = time;
    }
}
