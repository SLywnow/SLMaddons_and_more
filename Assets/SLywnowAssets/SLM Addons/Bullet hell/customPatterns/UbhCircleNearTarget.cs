using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class UbhCircleNearTarget : UbhBaseShot
{
	[Header("===== Circle near Target Settings =====")]

	[FormerlySerializedAs("RadiusNearTarget")]
	public float m_radiusNearTarget = 2;
	[Range(-360,360)][FormerlySerializedAs("AngleOffset")]
	public float m_angleOffset = 2;

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

	Vector2 defpos;

	private void Awake()
	{
		defpos = transform.localPosition;
	}

	public override void Shot()
	{
		if (m_bulletNum <= 0 || m_bulletSpeed <= 0f)
		{
			UbhDebugLog.LogWarning(name + " Cannot shot because BulletNum or BulletSpeed is not set.", this);
			return;
		}

		if (m_shooting)
		{
			return;
		}

		m_shooting = true;
	}

	private void Update()
	{
		if (m_shooting == false)
		{
			return;
		}

		float shiftAngle = 360f / (float)m_bulletNum;
		Vector2 targetPos = new Vector2(0, 0);
		bool targetGetted = false;

		if (m_targetTransform == null && m_setTargetFromTag)
		{
			m_targetTransform = UbhUtil.GetTransformFromTagName(m_targetTagName, m_randomSelectTagTarget, m_nearestSelectTagTarget, transform);
		}
		if (m_targetTransform != null)
		{
			targetPos = m_targetTransform.position;
			targetGetted = true;
		}

		if (targetGetted)
		{
			for (int i = 0; i < m_bulletNum; i++)
			{
				Vector2 angle = RotateBy(Vector2.up, shiftAngle * i, false) * m_radiusNearTarget;

				UbhBullet bullet = GetBullet(targetPos + angle);
				if (bullet == null)
				{
					break;
				}

				AimTarget();

				ShotBullet(bullet, m_bulletSpeed, GetYangleFromTwoPosition(targetPos + angle, m_targetTransform.position) + m_angleOffset);
			}
		}

		FiredShot();

		FinishedShot();
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
