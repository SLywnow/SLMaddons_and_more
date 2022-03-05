using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class SLM_BH_AutoDestroy : MonoBehaviour
{
    public bool destroyEnemies = true;
    public bool destroyBullets = true;
    public bool destroyPlayer = false;

    private void OnTriggerEnter2D(Collider2D c)
    {
        HitCheck(c.gameObject);
    }

    private void OnTriggerExit2D(Collider2D c)
    {
        HitCheck(c.gameObject);
    }


    void HitCheck(GameObject g)
	{
        if (destroyEnemies)
        {
            if (g.GetComponent<SLM_BH_Enemy>() != null)
            {
                g.GetComponent<SLM_BH_Enemy>().view.Die();
                g.GetComponent<SLM_BH_Enemy>().Kill();
            }
        }
        if (destroyPlayer)
		{
            if (g.GetComponent<SLM_BH_Player>() != null)
            {
                g.GetComponent<SLM_BH_Player>().view.Die();
                Destroy(g);
            }
        }
        if (destroyBullets)
		{
            if (g.GetComponentInParent<UbhBulletSimpleSprite2d>() != null)
                Destroy(g.GetComponentInParent<UbhBulletSimpleSprite2d>().gameObject);
            else if (g.GetComponentInParent<UbhTentacleBullet>() != null)
                Destroy(g.GetComponentInParent<UbhTentacleBullet>().gameObject);
        }
	}
}
