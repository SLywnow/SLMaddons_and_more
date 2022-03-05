using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Ubh hole circle shot.
/// </summary>
[AddComponentMenu("UniBulletHell/Shot Pattern/Hole Circle Shot")]
public class UbhHoleCircleShotRandom : UbhBaseShot
{
    [Header("===== HoleCircleShot Settings =====")]
    // "Set a center angle of hole. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_CenterAngle")]
    public float m_CenterAngle = 180f;
    [Range(0f, 360f), FormerlySerializedAs("HoleRandomAngle")] 
    public float m_holeRandomAngle = 90f;
    // "Set a size of hole. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_HoleSize")]
    public float m_holeSize = 20f;

    float m_holeCenterAngle = 180f;

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

        m_holeCenterAngle = UbhUtil.GetNormalizedAngle(Random.Range(m_CenterAngle - (m_holeRandomAngle / 2), m_CenterAngle + (m_holeRandomAngle / 2)));
        float startAngle = m_holeCenterAngle - (m_holeSize / 2f);
        float endAngle = m_holeCenterAngle + (m_holeSize / 2f);

        float shiftAngle = 360f / (float)m_bulletNum;

        for (int i = 0; i < m_bulletNum; i++)
        {
            float angle = shiftAngle * i;
            if (startAngle <= angle && angle <= endAngle)
            {
                continue;
            }

            UbhBullet bullet = GetBullet(transform.position);
            if (bullet == null)
            {
                break;
            }

            ShotBullet(bullet, m_bulletSpeed, angle);
        }

        FiredShot();

        FinishedShot();
    }
}