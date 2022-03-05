using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SDia_Joint : MonoBehaviour
{
	[Min(0)] public int connectionCount = int.MaxValue;
	public UnityEvent<int,int> onConnection;
	public UnityEvent<int,int> onDisconnection;

	[HideInInspector] 
	public List<string> connections;
	[HideInInspector] public bool enter;
	[HideInInspector] public bool exit;
	[HideInInspector] public SDia_Block main;

	public void Awake()
	{
		GetComponent<Button>().onClick.AddListener(() => Click());
		connections = new List<string>();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
		{

		}
	}

	public SDia_Block getConnection(int id)
	{
		SDia_Block ret = null;

		if (id >= 0 && id < connections.Count)
		{
			string[] line = connections[id].Split(' ');

			ret = main.main.blocks.Find(b => b.id == int.Parse(line[0])).block;
		}

		return ret;
	}

	public SDia_Joint getConnectionJoint(int id)
	{
		SDia_Joint ret = null;

		if (id >= 0 && id < connections.Count)
		{
			string[] line = connections[id].Split(' ');

			if (enter)
				ret = main.main.blocks.Find(b => b.id == int.Parse(line[0])).block.exits[int.Parse(line[1])];
			else if (exit)
				ret = main.main.blocks.Find(b => b.id == int.Parse(line[0])).block.enters[int.Parse(line[1])];
		}

		return ret;
	}

	public void RemoveAllConnections()
	{
		for (int i = connections.Count - 1; i >= 0; i--)
			RemoveConnection(i);
	}

	public void RemoveConnection(int id)
	{
		if (id >= 0 && id < connections.Count)
		{
			string[] line = connections[id].Split(' ');
			SDia_Joint j2 = null;
			if (enter)
				j2 = main.main.blocks.Find(b => b.id == int.Parse(line[0])).block.exits[int.Parse(line[1])];
			else if (exit)
				j2 = main.main.blocks.Find(b => b.id == int.Parse(line[0])).block.enters[int.Parse(line[1])];

			if (j2 != null)
			{
				if (enter)
					j2.connections.Remove(main.id + " " + main.enters.IndexOf(this));
				else if (exit)
					j2.connections.Remove(main.id + " " + main.exits.IndexOf(this));
				connections.RemoveAt(id);

				SDia_Connections con = main.main.conections.Find(c => c.p1 == j2.transform && c.p2 == transform);
				if (con == null)
					con = main.main.conections.Find(c => c.p2 == j2.transform && c.p1 == transform);
				Destroy(con.line.gameObject);
				main.main.conections.Remove(con);
				onDisconnection.Invoke(int.Parse(line[0]), int.Parse(line[1]));
			}
			else
				Debug.LogError("Can't remove joint's connection!");
		}
	}

	public void Click()
	{
		if (main != null)
		{
			if (main.main.currentJoint == null && exit)
				main.main.currentJoint = this;
			else if (main.main.currentJoint == this)
				main.main.currentJoint = null;
			else if (main.main.currentJoint != null && enter)
			{
				if (!main.main.currentJoint.connections.Contains(main.id + " " + main.enters.IndexOf(this)) && !main.exits.Contains(main.main.currentJoint))
				{
					if (main.main.currentJoint.connections.Count < main.main.currentJoint.connectionCount && connections.Count < connectionCount)
					{
						main.main.currentJoint.connections.Add(main.id + " " + main.enters.IndexOf(this));
						connections.Add(main.main.currentJoint.main.id + " " + main.main.currentJoint.main.exits.IndexOf(main.main.currentJoint));

						SDia_Connections c = new SDia_Connections();
						c.p1 = (RectTransform)main.main.currentJoint.transform;
						c.p2 = (RectTransform)transform;
						main.main.conections.Add(c);
						main.main.UpdateConnections();

						onConnection.Invoke(main.main.currentJoint.main.id, main.main.currentJoint.main.exits.IndexOf(main.main.currentJoint));
						main.main.currentJoint = null;
					}
				}
				else if (main.main.currentJoint.connections.Contains(main.id + " " + main.enters.IndexOf(this)))
				{
					main.main.currentJoint.connections.Remove(main.id + " " + main.enters.IndexOf(this));
					connections.Remove(main.main.currentJoint.main.id + " " + main.main.currentJoint.main.exits.IndexOf(main.main.currentJoint));
					Destroy(main.main.conections.Find(c => c.p1 == main.main.currentJoint.transform && c.p2 == transform).line.gameObject);
					main.main.conections.Remove(main.main.conections.Find(c => c.p1 == main.main.currentJoint.transform && c.p2 == transform));

					main.main.currentJoint = null;
				}
			}
		}
	}
}
