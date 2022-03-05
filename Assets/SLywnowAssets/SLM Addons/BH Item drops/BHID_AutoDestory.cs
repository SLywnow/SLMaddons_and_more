using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BHID_AutoDestory : MonoBehaviour
{
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
        if (g.GetComponent<BHID_Drop>() != null)
        {
            Destroy(g);
        }
    }
}
