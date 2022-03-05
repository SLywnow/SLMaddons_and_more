using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SLM_BH_BG : UbhMonoBehaviour
{
    public SLM_BH_Controll controll;
    public string TextureProperty = "_MainTex";

    public Vector2 Speed;

    public bool wiggle;
    public Vector2 Max;
    public Vector2 Min;
    public int RoundCount = 1;
    public float speed;

    Vector2 m_offset = UbhUtil.VECTOR2_ZERO;
    Vector2 m_dir = UbhUtil.VECTOR2_ZERO;
    Vector2 toff = UbhUtil.VECTOR2_ZERO;

    bool staticBG;

	public void Start()
	{
        if (controll != null)
            staticBG = controll.autoLoad.StaticScreen;
        if (wiggle)
            GetRandomDir();
    }

    public void GetRandomDir()
	{
        m_dir = new Vector2(Random.Range(Min.x, Max.x), Random.Range(Min.y, Max.y));           
	}

	private void Update()
    {
        if (!staticBG)
        {
            if (!wiggle)
            {
                float y = Mathf.Repeat(Time.time * Speed.y, 1f);
                float x = Mathf.Repeat(Time.time * Speed.x, 1f);
                m_offset.x = x;
                m_offset.y = y;
                renderer.sharedMaterial.SetTextureOffset(TextureProperty, m_offset);
            }
            else
            {
                toff = renderer.sharedMaterial.GetTextureOffset(TextureProperty);
                if (System.Math.Round(Vector2.Distance(toff, m_dir), RoundCount) == 0)
                {
                    GetRandomDir();
                }
                else
                {
                    renderer.sharedMaterial.SetTextureOffset(TextureProperty, Vector2.Lerp(toff, m_dir, speed * Time.deltaTime));
                }

            }
        }
    }
}
