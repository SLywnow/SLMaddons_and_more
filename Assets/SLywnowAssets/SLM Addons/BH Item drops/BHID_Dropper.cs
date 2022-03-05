using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BHID_Dropper : MonoBehaviour
{
    public BHID_Main main;
    public List<BHID_Dropper_Set> drops;

	public enum tg {onDestory, PlayerDamage, EnemyDieByPlayer, EnemyDieByCode, EnemyDamaged };
    public tg trigger;

	bool oncedrop;



	private void Awake()
	{
		if (trigger == tg.PlayerDamage)
		{
			GetComponent<SLM_BH_Player>().onDamage.AddListener(() => Drop());
		}
		else if (trigger == tg.EnemyDieByPlayer)
		{
			GetComponent<SLM_BH_Enemy>().eventOnDeathByPlayer.AddListener(() => Drop());
		}
		else if (trigger == tg.EnemyDieByCode)
		{
			GetComponent<SLM_BH_Enemy>().eventOnDeathByCode.AddListener(() => Drop());
		}
		else if (trigger == tg.EnemyDamaged)
		{
			GetComponent<SLM_BH_Enemy>().eventOnDamage.AddListener(() => Drop());
		}
	}

	private void OnDestroy()
	{
		if (trigger == tg.onDestory)
		{
			Drop();
		}
	}

	public void Drop()
	{
		if (!oncedrop)
		{
			foreach (BHID_Dropper_Set d in drops)
			{
				BHID_Main_Preset p = main.presets.Find(pr => pr.name == d.dropName);

				if (p != null)
				{
					for (int i = 0; i < d.dropCount; i++)
					{
						BHID_Drop obj = Instantiate(p.obj, transform.position, Quaternion.identity);
						obj.gameObject.SetActive(true);
						obj.count = d.count;
						obj.type = d.dropName;
						obj.getBy = name;
						obj.getted = false;
						if (d.useCustomSpeed)
						{
							obj.speed = d.speed;
							float angle = Random.Range(d.angle - d.randomAngle, d.angle + d.randomAngle);
							obj.vector = RotateBy(Vector2.up, angle);
						}

						if (main.magnet !=null)
						{
							obj.magnetTarget = main.magnet;
							obj.magnet = true;
						}

						obj.SetForce();
					}
				}
				else
					Debug.LogError(d.dropName + "not found in presets!");
			}
		}

		if (trigger == tg.EnemyDieByPlayer)
		{
			oncedrop = true;
		}
		else if (trigger == tg.EnemyDieByCode)
		{
			oncedrop = true;
		}
	}

	public static Vector2 RotateBy(Vector2 v, float a, bool bUseRadians = false)
	{
		if (!bUseRadians) a *= Mathf.Deg2Rad;
		var ca = Mathf.Cos(a);
		var sa = Mathf.Sin(a);
		var rx = v.x * ca - v.y * sa;

		return new Vector2((float)rx, (float)(v.x * sa + v.y * ca));
	}
}

[System.Serializable]
public class BHID_Dropper_Set
{
    public string dropName;
    public int count;
	[Min(1)] public int dropCount;
    public bool useCustomSpeed;
    public int speed;
    [Range(-360,360)] public float angle;
	[Range(-360, 360)] public float randomAngle = 0;
}
