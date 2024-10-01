using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SLM_BH_Camera : MonoBehaviour
{
	public SLM_BH_Controll controll;
	public Camera cam;
	public float Z = -30f;
	public float speed = 2f;
	public Vector2 offset;
	public bool useborder = false;
	public Vector2 min;
	public Vector2 max;
	public float size;

	[HideInInspector]
	public TDS_CameraControll_default def;
	[HideInInspector]
	public float changespeed;

	Transform target;
	float savesize;

	private void Start()
	{	
		cam = GetComponent<Camera>();
		size = cam.orthographicSize;
		savesize = size;
		controll.onStartBulletHell.AddListener(() => SetUp());
	}

	public void SetUp()
	{
		target = controll.curpl.transform;
		def.target = target;
		def.size = cam.orthographicSize;
	}

	void FixedUpdate()
	{
		if (!controll.stopCamera)
		{
			if (target != null)
			{
				Vector3 pos = target.position + (Vector3)offset;
				pos.z = Z;
				if (useborder) pos = new Vector3(Mathf.Clamp(pos.x, min.x, max.x), Mathf.Clamp(pos.y, min.y, max.y), Z);
				transform.position = Vector3.Lerp(transform.position, pos, speed * Time.deltaTime);
			}
		}
	}

	public float time;
	private void Update()
	{
		if (!controll.stopCamera)
		{
			if (size != savesize)
			{
				if (System.Math.Round(cam.orthographicSize, 2) != size)
				{
					time += changespeed * Time.deltaTime;
					cam.orthographicSize = Mathf.Lerp(savesize, size, time);
				}
				else
				{
					cam.orthographicSize = size;
					savesize = size;
					time = 0;
				}
			}
		}
	}
}

[System.Serializable]
public class TDS_CameraControll_default
{
	public Transform target;
	public float size = 5f;
}
