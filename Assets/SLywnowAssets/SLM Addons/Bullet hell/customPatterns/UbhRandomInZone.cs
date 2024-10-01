using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using UnityEngine.Serialization;

public class UbhRandomInZone : UbhBaseShot
{
	[Header("===== RandomShotInZone Settings =====")]
	// "Center angle of random range."
	[Range(0f, 360f), FormerlySerializedAs("_RandomCenterAngle")]
	public float m_randomCenterAngle = 180f;
	// "Set a angle size of random range. (0 to 360)"
	[Range(0f, 360f), FormerlySerializedAs("_RandomRangeSize")]
	public float m_randomRangeSize = 360f;
	// "Set a minimum bullet speed of shot."
	// "BulletSpeed is ignored."
	[FormerlySerializedAs("_RandomSpeedMin")]
	public float m_randomSpeedMin = 1f;
	// "Set a maximum bullet speed of shot."
	// "BulletSpeed is ignored."
	[FormerlySerializedAs("_RandomSpeedMax")]
	public float m_randomSpeedMax = 3f;
	// "Set a minimum delay time between bullet and next bullet. (sec)"
	[FormerlySerializedAs("_RandomDelayMin")]
	public float m_randomDelayMin = 0.01f;
	// "Set a maximum delay time between bullet and next bullet. (sec)"
	[FormerlySerializedAs("_RandomDelayMax")]
	public float m_randomDelayMax = 0.1f;
	// "Evenly distribute of all bullet angle."
	[FormerlySerializedAs("_EvenlyDistribute")]
	public bool m_evenlyDistribute = true;

	[FormerlySerializedAs("Zone as transform (optional)")]
	public Transform m_zoneTransform;
	[FormerlySerializedAs("Zone min pos")]
	public Vector2 m_zoneMin;
	[FormerlySerializedAs("Zone max pos")]
	public Vector2 m_zoneMax;
	[FormerlySerializedAs("IgnoreSpawnNearTargetInRaius")]
	public float m_ignoreSpawnNearTargetInRaius = 1;

	// "Set a target with tag name."
	[FormerlySerializedAs("_SetTargetFromTag")]
	public bool m_setTargetFromTag = true;
	// "Set a unique tag name of target at using SetTargetFromTag."
	[FormerlySerializedAs("_TargetTagName"), UbhConditionalHide("m_setTargetFromTag")]
	public string m_targetTagName = "Player";
	// "Flag to select random from GameObjects of the same tag."
	[UbhConditionalHide("m_setTargetFromTag")]
	public bool m_randomSelectTagTarget;
	// "Flag to select nearest from GameObjects of the same tag."
	[UbhConditionalHide("m_setTargetFromTag")]
	public bool m_nearestSelectTagTarget;
	// "Transform of lock on target."
	// "It is not necessary if you want to specify target in tag."
	// "Overwrite Angle in direction of target to Transform.position."
	[FormerlySerializedAs("_TargetTransform")]
	public Transform m_targetTransform;
	[FormerlySerializedAs("_Aiming")]
	public bool m_aiming;

	private float m_delayTimer;

	private List<int> m_numList;
	Vector2 defpos = new Vector2(0, 0);

	private void Awake()
	{
		defpos = transform.localPosition;

		if (m_zoneTransform !=null)
		{
			Vector2 curMin;
			Vector2 curMax;

			curMin.x = (-(m_zoneTransform.localScale.x / 2)) + m_zoneTransform.position.x;
			curMin.y = (-(m_zoneTransform.localScale.y / 2)) + m_zoneTransform.position.y;
			curMax.x = (m_zoneTransform.localScale.x / 2) + m_zoneTransform.position.x;
			curMax.y = (m_zoneTransform.localScale.y / 2) + m_zoneTransform.position.y;

			m_zoneMin.x = curMin.x <= curMax.x ? curMin.x : curMax.x;
			m_zoneMin.y = curMin.y <= curMax.y ? curMin.y : curMax.y;
			m_zoneMax.x = curMin.x > curMax.x ? curMin.x : curMax.x;
			m_zoneMax.y = curMin.y > curMax.y ? curMin.y : curMax.y;
		}
	}

	public override void Shot()
	{
		if (m_bulletNum <= 0 || m_randomSpeedMin <= 0f || m_randomSpeedMax <= 0)
		{
			UbhDebugLog.LogWarning(name + " Cannot shot because BulletNum or RandomSpeedMin or RandomSpeedMax is not set.", this);
			return;
		}

		if (m_shooting)
		{
			return;
		}

		m_shooting = true;
		m_delayTimer = 0f;

		if (m_numList != null)
		{
			m_numList.Clear();
			m_numList = null;
		}

		m_numList = new List<int>(m_bulletNum);
		for (int i = 0; i < m_bulletNum; i++)
		{
			m_numList.Add(i);
		}
	}

	protected virtual void Update()
	{
		if (m_shooting == false)
		{
			return;
		}

		if (m_delayTimer >= 0f)
		{
			m_delayTimer -= UbhTimer.instance.deltaTime;
			if (m_delayTimer >= 0f)
			{
				return;
			}
		}

		int index = Random.Range(0, m_numList.Count);
		float minAngle = m_randomCenterAngle - (m_randomRangeSize / 2f);
		float maxAngle = m_randomCenterAngle + (m_randomRangeSize / 2f);
		float angleShoot = 0f;

		Vector2 newpos = new Vector2(Random.Range(m_zoneMin.x, m_zoneMax.x), Random.Range(m_zoneMin.y, m_zoneMax.y));

		AimTarget();

		if ((!m_aiming && m_ignoreSpawnNearTargetInRaius <= 0) || m_targetTransform != null)
		{
			bool accept = false;
			if (m_ignoreSpawnNearTargetInRaius > 0)
			{
				if (Vector2.Distance(newpos, m_targetTransform.position) >= m_ignoreSpawnNearTargetInRaius)
					accept = true;
			}
			else
				accept = true;

			if (accept)
			{

				UbhBullet bullet = GetBullet(newpos);
				if (bullet == null)
				{
					return;
				}

				float bulletSpeed = Random.Range(m_randomSpeedMin, m_randomSpeedMax);

				if (m_evenlyDistribute)
				{
					float oneDirectionNum = m_bulletNum >= 4 ? Mathf.Floor((float)m_bulletNum / 4f) : 1;
					float quarterIndex = Mathf.Floor((float)m_numList[index] / oneDirectionNum);
					float quarterAngle = Mathf.Abs(maxAngle - minAngle) / 4f;
					angleShoot = Random.Range(minAngle + (quarterAngle * quarterIndex), minAngle + (quarterAngle * (quarterIndex + 1f)));
				}
				else
				{
					angleShoot = Random.Range(minAngle, maxAngle);
				}

				ShotBullet(bullet, bulletSpeed, m_aiming ? GetYangleFromTwoPosition(newpos, m_targetTransform.position) : angleShoot);
				FiredShot();
			}
		}

		m_numList.RemoveAt(index);

		if (m_numList.Count <= 0)
		{
			FinishedShot();
		}
		else
		{
			m_delayTimer = Random.Range(m_randomDelayMin, m_randomDelayMax);
			if (m_delayTimer <= 0f)
			{
				Update();
			}
		}
	}


	private static float GetYangleFromTwoPosition(Vector2 fromTrans, Vector2 toTrans)
	{
		if (fromTrans == null || toTrans == null)
		{
			return 0f;
		}
		float xDistance = toTrans.x - fromTrans.x;
		float yDistance = toTrans.y - fromTrans.y;
		float angle = (Mathf.Atan2(yDistance, xDistance) * Mathf.Rad2Deg) - 90f;
		angle = UbhUtil.GetNormalizedAngle(angle);

		return angle;
	}

	private void AimTarget()
	{
		if (m_targetTransform == null && m_setTargetFromTag)
		{
			m_targetTransform = UbhUtil.GetTransformFromTagName(m_targetTagName, m_randomSelectTagTarget, m_nearestSelectTagTarget, transform);
		}
	}

	/// <summary>
	/// Rotate Vector2 clockwise by 'a'
	/// </summary>
	/// <param name="v"></param>
	/// <param name="a"></param>
	/// <returns></returns>
	public static Vector2 RotateBy(Vector2 v, float a, bool bUseRadians = false)
	{
		if (!bUseRadians) a *= Mathf.Deg2Rad;
		var ca = Math.Cos(a);
		var sa = Math.Sin(a);
		var rx = v.x * ca - v.y * sa;

		return new Vector2((float)rx, (float)(v.x * sa + v.y * ca));
	}
}
