using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BHID_Main : MonoBehaviour
{
	public SLM_Commands C;
	public List<BHID_Main_Preset> presets;

	List<string> commands;
	List<bool> bools;

	[HideInInspector] public string triggered;
	[HideInInspector] public Transform magnet;
	[HideInInspector] public float magnetTime = -1;


	void AddCommand(string command, bool next)
	{
		commands.Add(command);
		bools.Add(next);
	}

	private void Awake()
	{
		commands = new List<string>();
		bools = new List<bool>();

		AddCommand("isbhidcount", false);
		AddCommand("islastdropper", false);
		AddCommand("islastdrop", false);
		AddCommand("setdropcount", true);
		AddCommand("adddropcount", true);

		if (C.addonManager != null)
			C.addonManager.AddAddon("BulletHell Item Drops", Command, commands, bools);
	}

	public bool Command(string comm, string[] args)
	{
		switch (comm)
		{
			case "isbhidcount":
				{
					//isbhidcount::name(vw)::count(vw)::true::false;;

					string nm = C.ValueWorkString(args[0]);
					int count = C.ValueWorkInt(args[1]);
					int cTrue = C.ValueWorkCommand(args[2]);
					int cFalse = C.ValueWorkCommand(args[3]);

					BHID_Main_Preset p = presets.Find(pr => pr.name == nm);

					if (p==null)
					{
						C.Error("Preset with name " + nm + " not found!");
						return false;
					}
					else
					{
						if (p.count >= count)
							C.RunCommand(cTrue);
						else
							C.RunCommand(cFalse);
					}

					return true;
				}
			case "islastdropper":
				{
					//islashdropper::presetName(vw)::name(vw)::true::false;;
					string nm = C.ValueWorkString(args[0]);
					string check = C.ValueWorkString(args[1]);
					int cTrue = C.ValueWorkCommand(args[2]);
					int cFalse = C.ValueWorkCommand(args[3]);

					BHID_Main_Preset p = presets.Find(pr => pr.name == nm);

					if (p == null)
					{
						C.Error("Preset with name " + nm + " not found!");
						return false;
					}
					else
					{
						if (p.lastDropper == check)
							C.RunCommand(cTrue);
						else
							C.RunCommand(cFalse);
					}

					return true;
				}
			case "islastdrop":
				{
					//islastdrop::name(vw)::true::false;;
					string check = C.ValueWorkString(args[0]);
					int cTrue = C.ValueWorkCommand(args[1]);
					int cFalse = C.ValueWorkCommand(args[2]);


					if (triggered == check)
						C.RunCommand(cTrue);
					else
						C.RunCommand(cFalse);

					return true;
				}
			case "setdropcount":
				{
					//setdropcount::presetName(vw)::count(vw);;

					string nm = C.ValueWorkString(args[0]);
					int count = C.ValueWorkInt(args[1]);

					BHID_Main_Preset p = presets.Find(pr => pr.name == nm);

					if (p == null)
					{
						C.Error("Preset with name " + nm + " not found!");
						return false;
					}
					else
					{
						int id = presets.IndexOf(p);
						presets[id].count = count;
						presets[id].count = Mathf.Clamp(presets[id].count, 0, int.MaxValue);
						if (!string.IsNullOrEmpty(presets[id].statsconnect))
						{
							C.stats.SetValue(presets[id].statsconnect, presets[id].count);
						}
					}

					return true;
				}
			case "adddropcount":
				{
					//adddropcount::presetName(vw)::count(vw);;

					string nm = C.ValueWorkString(args[0]);
					int count = C.ValueWorkInt(args[1]);

					BHID_Main_Preset p = presets.Find(pr => pr.name == nm);

					if (p == null)
					{
						C.Error("Preset with name " + nm + " not found!");
						return false;
					}
					else
					{
						int id = presets.IndexOf(p);
						presets[id].count += count;
						presets[id].count = Mathf.Clamp(presets[id].count, 0, int.MaxValue);
						if (!string.IsNullOrEmpty(presets[id].statsconnect))
						{
							C.stats.SetValue(presets[id].statsconnect, presets[id].count);
						}
					}

					return true;
				}
		}
		return false;
	}

	public void SetMagnet(float time, Transform target)
	{
		magnetTime = time;
		magnet = target;
	}

	public void StopMagnet()
	{
		magnetTime = -1;
		magnet = null;
	}

	private void Update()
	{
		if (magnetTime != -1)
		{
			if (magnetTime > 0)
			{
				magnetTime -= Time.deltaTime;
			}
			else
			{
				magnet = null;
				magnetTime = -1;
			}
		}
	}

	public void GetDrop(string name, int count, string getBy)
	{
		BHID_Main_Preset p = presets.Find(pr => pr.name == name);
		
		if (p != null)
		{
			int id = presets.IndexOf(p);
			presets[id].count += count;
			presets[id].lastDropper = getBy;
			triggered = p.name;
			if (!string.IsNullOrEmpty(presets[id].statsconnect))
			{
				C.stats.SetValue(presets[id].statsconnect, presets[id].count);
			}

			presets[id].runEvent.Invoke();

			if (!string.IsNullOrEmpty(presets[id].command))
			{
				C.RunPoint(presets[id].command);
			}
		}
	}
}

[System.Serializable]
public class BHID_Main_Preset
{
	public string name;
	[InspectorName("Spawn Object")] public BHID_Drop obj;
	public int count;
	public string statsconnect;
	public string command;
	public UnityEvent runEvent;
	[HideInInspector] public string lastDropper;

}