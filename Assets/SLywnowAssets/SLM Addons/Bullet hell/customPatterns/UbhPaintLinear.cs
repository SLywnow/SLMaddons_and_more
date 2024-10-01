using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class UbhPaintLinear : UbhBaseShot
{
	private static readonly string[] SPLIT_VAL = { "\n", "\r", "\r\n" };

	[Header("===== PaintShotLinear Settings =====")]
	// "Set a paint data text file. (ex.[UniBulletHell] > [Example] > [PaintShotData] in Project view)"
	// "BulletNum is ignored."
	[FormerlySerializedAs("_PaintDataText")]
	public TextAsset m_paintDataText;

	[FormerlySerializedAs("_PaintTextData")]
	[Multiline]
	public string m_paintTextData;
	// "Set a center angle of shot. (0 to 360) (center of first line)"
	[Range(0f, 360f), FormerlySerializedAs("_Angle")]
	public float m_Angle = 180f;
	[FormerlySerializedAs("_BetweenSize")]
	public float m_betweenSize = 0.2f;
	[FormerlySerializedAs("_CenterPaint")]
	public bool m_centerPaint;
	[FormerlySerializedAs("_RotatePaint")]
	public bool m_rotatePaint;

	public override void Shot()
	{
		if (m_bulletSpeed == 0f || ((m_paintDataText == null || string.IsNullOrEmpty(m_paintDataText.text)) && string.IsNullOrEmpty(m_paintTextData)))
		{
			UbhDebugLog.LogError(name + " Cannot shot because BulletSpeed or PaintDataText is not set.", this);
			return;
		}

		List<List<bool>> m_paintData = LoadPaintData();
		if (m_paintData == null || m_paintData.Count <= 0)
		{
			UbhDebugLog.LogError(name + " Cannot shot because PaintDataText load error.", this);
			return;
		}

		float startPosY = ((m_paintData.Count - 1) / 2) * -m_betweenSize;
		float startPosX = 0;
		if (!m_centerPaint)
		{
			int maxcount = 0;
			foreach (List<bool> i in m_paintData)
				if (maxcount < i.Count) maxcount = i.Count;

			startPosX = ((maxcount - 1) / 2) * -m_betweenSize;
		}

		for (int y = 0; y < m_paintData.Count; y++)
		{
			if (m_centerPaint)
				startPosX = ((m_paintData[y].Count - 1) / 2) * -m_betweenSize;

			for (int x = 0; x < m_paintData[y].Count;x++)
			{

				if (m_paintData[y][x])
				{
					Vector2 posOffset = new Vector2(startPosX + m_betweenSize * x, startPosY + m_betweenSize * y);

					if (m_rotatePaint)
						posOffset = RotateBy(posOffset, m_Angle);

					Vector2 pos = transform.position;
					pos += posOffset;

					UbhBullet bullet = GetBullet(pos);
					if (bullet == null)
					{
						break;
					}

					float angle = m_Angle;

					ShotBullet(bullet, m_bulletSpeed, angle);
				}
			}
		}

		FiredShot();
		FinishedShot();
	}

	private List<List<bool>> LoadPaintData()
	{
		if ((m_paintDataText == null || string.IsNullOrEmpty(m_paintDataText.text)) && string.IsNullOrEmpty(m_paintTextData))
		{
			UbhDebugLog.LogError(name + " Cannot load paint data because PaintDataText file is null or empty.", this);
			return null;
		}

		string[] lines = m_paintTextData.Split(SPLIT_VAL, System.StringSplitOptions.RemoveEmptyEntries);

		if (m_paintDataText != null && !string.IsNullOrEmpty(m_paintDataText.text)) 
		{
			lines = m_paintDataText.text.Split(SPLIT_VAL, System.StringSplitOptions.RemoveEmptyEntries);
		}

		var paintData = new List<List<bool>>(lines.Length);

		for (int i = 0; i < lines.Length; i++)
		{
			// lines beginning with "#" are ignored as comments.
			if (lines[i].StartsWith("#"))
			{
				continue;
			}
			// add line
			paintData.Add(new List<bool>(lines[i].Length));

			for (int j = 0; j < lines[i].Length; j++)
			{
				// bullet is fired into position of "*".
				paintData[paintData.Count - 1].Add(lines[i][j] == '*');
			}
		}

		// reverse because fire from bottom left.
		paintData.Reverse();

		return paintData;
	}

	public static Vector2 RotateBy(Vector2 v, float a, bool bUseRadians = false)
	{
		if (!bUseRadians) a *= Mathf.Deg2Rad;
		var ca = System.Math.Cos(a);
		var sa = System.Math.Sin(a);
		var rx = v.x * ca - v.y * sa;

		return new Vector2((float)rx, (float)(v.x * sa + v.y * ca));
	}
}
