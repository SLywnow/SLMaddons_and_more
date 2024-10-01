using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class UbhSpiralNearPlayer : UbhBaseShot
{
	[Header("===== SpiralShotNearTarget Settings =====")]
	// "Set a starting angle of shot. (0 to 360)"
	[Range(0f, 360f), FormerlySerializedAs("_StartAngle")]
	public float m_startAngle = 180f;
	// "Set a shift angle of spiral. (-360 to 360)"
	[Range(-360f, 360f), FormerlySerializedAs("_ShiftAngle")]
	public float m_shiftAngle = 5f;
	// "Set a delay time between bullet and next bullet. (sec)"
	[FormerlySerializedAs("_BetweenDelay")]
	public float m_betweenDelay = 0.2f;

	[FormerlySerializedAs("RadiusNearTarget")]
	public float m_radiusNearTarget = 2;
	[FormerlySerializedAs("UpdateTargetPosition")]
	public bool m_updateTargetPos;

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

	Vector2 defpos;
	Vector2 targetPos = new Vector2(0, 0);
	private int m_nowIndex;
	private float m_delayTimer;

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
		m_nowIndex = 0;
		m_delayTimer = 0f;

		if (m_targetTransform == null && m_setTargetFromTag)
		{
			m_targetTransform = UbhUtil.GetTransformFromTagName(m_targetTagName, m_randomSelectTagTarget, m_nearestSelectTagTarget, transform);
		}
		if (m_targetTransform != null)
		{
			targetPos = m_targetTransform.position;
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

		Vector2 angle = RotateBy(Vector2.up, m_startAngle + (m_shiftAngle * m_nowIndex), false) * m_radiusNearTarget;
		
		bool targetGetted = false;

		if (m_updateTargetPos)
		{
			if (m_targetTransform == null && m_setTargetFromTag)
			{
				m_targetTransform = UbhUtil.GetTransformFromTagName(m_targetTagName, m_randomSelectTagTarget, m_nearestSelectTagTarget, transform);
			}
			if (m_targetTransform != null)
			{
				targetPos = m_targetTransform.position;
				targetGetted = true;
			}
		}
		else if (m_targetTransform != null)
			targetGetted = true;

		if (targetGetted)
		{

			UbhBullet bullet = GetBullet(targetPos + angle);
			if (bullet == null)
			{
				FinishedShot();
				return;
			}

			AimTarget();

			ShotBullet(bullet, m_bulletSpeed, m_aiming ? GetYangleFromTwoPosition(targetPos + angle, m_targetTransform.position) : m_startAngle + (m_shiftAngle * m_nowIndex) + 180);
			FiredShot();
		}

		m_nowIndex++;

		if (m_nowIndex >= m_bulletNum)
		{
			FinishedShot();
		}
		else
		{
			m_delayTimer = m_betweenDelay;
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
