using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BHID_Drop : MonoBehaviour
{
    [Header("System")]
    public string type;
    public int count;
    [HideInInspector] public string getBy;
    [HideInInspector] public bool getted;

    public enum tpe { rigidbody, transform};
    [Header("Move")]
    public tpe moveType;
    public int speed;
    public Vector2 vector;

    [Header("Magnet")]
    public bool magnet;
    public Transform magnetTarget;
    public float magnetForce;

    bool movestart;
    Rigidbody2D rg;
    Transform me;

    public void SetForce()
    {
        if (moveType == tpe.rigidbody)
            rg = GetComponent<Rigidbody2D>();

        me = transform;

        if (moveType == tpe.transform)
        {
            movestart = true;
        }
        else if (moveType == tpe.rigidbody)
		{
            rg.AddForce(vector * speed);
        }
    }

	private void Update()
	{
        if (movestart)
            me.position = new Vector2(me.position.x, me.position.y) + (vector * speed * Time.deltaTime);

        if (magnet && magnetTarget !=null)
        {
            me.position = Vector2.MoveTowards(me.position, magnetTarget.position, magnetForce * Time.deltaTime);
        }
	}
}
