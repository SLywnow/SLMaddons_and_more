using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Collider2D))]
public class SLM_BH_EntityTrigger : MonoBehaviour
{
	public List<string> targetTags;
    public UnityEvent<Collider2D> onTriggerEnter;
    public UnityEvent<Collider2D> onTriggerExit;


	private void Start()
	{
		GetComponent<Collider2D>().isTrigger = true;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (targetTags.Contains(collision.gameObject.tag))
			onTriggerEnter.Invoke(collision);
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (targetTags.Contains(collision.gameObject.tag))
			onTriggerExit.Invoke(collision);
	}
}


#if UNITY_EDITOR
[CustomEditor(typeof(SLM_BH_EntityTrigger))]
[CanEditMultipleObjects]
public class SLM_BH_EntityTriggerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		EditorGUILayout.LabelField("Used by other SLM BH scripts");
	}
}
#endif