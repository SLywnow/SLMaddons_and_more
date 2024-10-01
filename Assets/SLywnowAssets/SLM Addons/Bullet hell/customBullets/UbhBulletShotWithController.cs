using SLywnow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UbhBulletShotWithController : UbhBullet
{
	[SerializeField]
	private Rigidbody2D m_rigidbody2d = null;
	[SerializeField]
	private Collider2D[] m_collider2ds = null;
	[SerializeField]
	private SpriteRenderer[] m_spriteRenderers = null;

	public enum md { onStart, onEnd };
	public md mode;

	public UbhShotCtrl controll;

	private bool m_isActive;

	/// <summary>
	/// Activate/Inactivate flag
	/// Override this property when you want to change the behavior at Active / Inactive.
	/// </summary>
	public override bool isActive { get { return m_isActive; } }

	/// <summary>
	/// Activate/Inactivate Bullet
	/// </summary>
	public override void SetActive(bool isActive)
	{
		m_isActive = isActive;
		if (controll == null)
			controll = GetComponent<UbhShotCtrl>();

		m_rigidbody2d.simulated = isActive;

		if (m_collider2ds != null && m_collider2ds.Length > 0)
		{
			for (int i = 0; i < m_collider2ds.Length; i++)
			{
				m_collider2ds[i].enabled = isActive;
			}
		}

		if (m_spriteRenderers != null && m_spriteRenderers.Length > 0)
		{
			for (int i = 0; i < m_spriteRenderers.Length; i++)
			{
				m_spriteRenderers[i].enabled = isActive;
			}
		}

		if (isActive && mode == md.onStart)
			StartCoroutine(waitFrame());
		else if (!isActive && mode == md.onEnd)
		{
			Run();
		}
	}

	IEnumerator waitFrame()
	{
		yield return new WaitForEndOfFrame();

		Run();
	}

	void Run()
	{
		if (controll != null)
		{
			controll.StartShotRoutine();
		}
	}
}
