using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SLywnow;

[RequireComponent(typeof(Collider2D))]
public class SLM_BH_Trigger : MonoBehaviour
{
	public string PlayerTag;
	public List<SLM_BH_Trigger_Data> data;
	public UnityEvent onEnter;
	public UnityEvent onExit;

	bool inTrigger;

	private void Start()
	{
		GetComponent<Collider2D>().isTrigger = true;

		foreach (SLM_BH_Trigger_Data d in data)
		{
			if (d.controll == null)
				d.controll=FindObjectOfType<SLM_BH_Controll>();
			if (d.camera == null)
				d.camera = FindObjectOfType<SLM_BH_Camera>();
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.tag == PlayerTag && !inTrigger)
		{
			inTrigger = true;
			foreach (SLM_BH_Trigger_Data d in data)
			{
				if (d.Type == SLM_BH_Trigger_Data.tpe.cameralock)
				{
					if (d.camera != null)
					{
						if (d.disableOnExit)
						{
							if (d.changeSpeed)
								d.sspeed = d.camera.speed;
							if (d.changeOffset)
								d.soffset = d.camera.offset;
							if (d.changeBorder)
							{
								d.suseBorder = d.camera.useborder;
								d.smin = d.camera.min;
								d.smax = d.camera.max;
							}
							if (d.changeSize)
								d.ssize = d.camera.size;
						}

						if (d.changeSpeed)
							d.camera.speed = d.speed;
						if (d.changeOffset)
							d.camera.offset = d.offset;
						if (d.changeBorder)
						{
							d.camera.useborder = true;
							d.camera.min = d.min;
							d.camera.max = d.max;
						}
						if (d.changeSize)
						{
							d.camera.size = d.size;
							d.camera.changespeed = d.speedchange;
						}
					}
				}
				else if (d.Type == SLM_BH_Trigger_Data.tpe.EnemyAI)
				{
					if (d.enemy != null)
					{
						if (d.isActive && !d.dontChangeAI)
						{
							if (d.enemy.easyAI.shootObject != null)
								d.enemy.easyAI.shootObject.StopShotRoutineAndPlayingShot();
							if (d.enemy.middleAI.shootObject != null)
								d.enemy.middleAI.shootObject.StopShotRoutineAndPlayingShot();
							if (d.enemy.middleAI.shootObject != null)
								d.enemy.middleAI.shootObject.StopShotRoutineAndPlayingShot();
							foreach (SLM_BH_Enemy_MiddleAiAdvanced p in d.enemy.hardAI.patterns)
								if (p.shootObject !=null)
									p.shootObject.StopShotRoutineAndPlayingShot();

							d.enemy.AIType = d.AIType;
							if (d.AIType == SLM_BH_Enemy.aitpe.easy)
								d.enemy.easyAI = d.easyAI;
							if (d.AIType == SLM_BH_Enemy.aitpe.middle)
							{
								d.enemy.curpos = 0;
								d.enemy.middleAI = d.middleAI;
							}
							if (d.AIType == SLM_BH_Enemy.aitpe.hard)
							{
								d.enemy.hardAI = d.hardAI;
								d.enemy.curpos = 0;
							}
							if (d.AIType == SLM_BH_Enemy.aitpe.trigger)
							{
								d.enemy.triggerAI = d.triggerAI;
								d.enemy.curpos = 0;
							}
							d.enemy.AIGet();
						}
						d.enemy.gameObject.SetActive(d.isActive);
					}
				}
				else if (d.Type == SLM_BH_Trigger_Data.tpe.PlayerSettings)
				{
					if (d.player != null)
					{
						if (d.disableOnExit)
						{
							d.sPlayerSpeed = d.player.speed;
							d.sPlyaerspeedShift = d.player.speedShift;
						}

						d.player.speed = d.PlayerSpeed;
						d.player.speedShift = d.PlyaerSpeedShift;
					}
				}
				else if (d.Type == SLM_BH_Trigger_Data.tpe.WinZone)
				{
					if (d.controll != null)
						d.controll.EndBH(true);
				}
				else if (d.Type == SLM_BH_Trigger_Data.tpe.Interactable)
				{
					d.canInteractable = true;
					if (d.showObjectWhenCanIntractable)
						d.objectWhenCanIntractable.SetActive(true);
					if (d.controll != null)
						d.controll.lastTrigger = this;
				}
				else if (d.Type == SLM_BH_Trigger_Data.tpe.Teleport)
				{
					tptime = d.teleportDelay;

					if (d.teleportDelay > 0 && d.freezeWhenDelay)
						d.player.freezetime = d.teleportDelay;
				}
			}

			onEnter.Invoke();
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.tag == PlayerTag)
		{
			inTrigger = false;
			foreach (SLM_BH_Trigger_Data d in data)
			{
				if (d.Type == SLM_BH_Trigger_Data.tpe.cameralock)
				{
					if (d.camera != null)
					{
						if (d.disableOnExit)
						{
							if (d.changeSpeed)
								d.camera.speed = d.sspeed;
							if (d.changeOffset)
								d.camera.offset = d.soffset;
							if (d.changeBorder)
							{
								d.camera.useborder = d.suseBorder;
								d.camera.min = d.smin;
								d.camera.max = d.smax;
							}
							if (d.changeSize)
								d.camera.size = d.ssize;
						}
					}
				}
				else if (d.Type == SLM_BH_Trigger_Data.tpe.PlayerSettings)
				{
					if (d.player != null)
					{
						if (d.disableOnExit)
						{
							d.player.speed = d.sPlayerSpeed;
							d.player.speedShift = d.sPlyaerspeedShift;
						}
					}
				}
				else if (d.Type == SLM_BH_Trigger_Data.tpe.Interactable)
				{
					d.canInteractable = false;
					if (d.showObjectWhenCanIntractable)
						d.objectWhenCanIntractable.SetActive(false);
					if (d.controll !=null && d.controll.lastTrigger == this)
						d.controll.lastTrigger = null;
				}
			}
			onExit.Invoke();
		}
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		if (collision.tag == PlayerTag)
		{
			foreach (SLM_BH_Trigger_Data d in data)
			{
				if (d.Type == SLM_BH_Trigger_Data.tpe.Damage)
				{
					if (d.player != null)
					{
						if (string.IsNullOrEmpty(d.bulletName) && !d.player.controll.damageName.Contains(d.bulletName))
							d.player.controll.hp -= (d.damage - 1);

						d.player.Damage(d.bulletName, d.isEnemy);
					}
					else if (d.controll != null && d.controll.curpl != null)
					{
						if (string.IsNullOrEmpty(d.bulletName) && !d.controll.damageName.Contains(d.bulletName))
							d.controll.hp -= (d.damage - 1);

						d.controll.curpl.Damage(d.bulletName, d.isEnemy);
					}
				}
			}
		}
	}


	float tptime=-1;

	private void Update()
	{
		if (inTrigger)
		{
			foreach (SLM_BH_Trigger_Data d in data)
			{
				if (d.Type == SLM_BH_Trigger_Data.tpe.Interactable && (d.controll == null || d.controll.curpl == null || d.controll.curpl.freezetime <= 0))
				{
					if (Input.GetButtonDown(d.intractableButton))
						d.intractableEvent.Event.Invoke();
				}
				else if (d.Type == SLM_BH_Trigger_Data.tpe.Teleport)
				{
					if (tptime <= 0)
					{
						Vector2 tpposition = d.teleportPos;
						if (d.teleportMode == SLM_BH_Trigger_Data.tpMode.target)
							tpposition = d.teleportTarget.position;

						d.player.transform.position = tpposition;
						if (d.teleportCameraToo)
							d.camera.transform.position = tpposition;
					}
					else tptime -= Time.deltaTime;
				}
			}
		}
	}

	public void IntractablePress(bool ignorebutton = false)
	{
		foreach (SLM_BH_Trigger_Data d in data)
		{
			if (d.Type == SLM_BH_Trigger_Data.tpe.Interactable)
			{
				if (d.canInteractable)
				{
					if (Input.GetButtonDown(d.intractableButton) || ignorebutton)
						d.intractableEvent.Event.Invoke();
				}
			}
		}
	}
}

[System.Serializable]
public class SLM_BH_Trigger_Data
{
	public enum tpe { cameralock, EnemyAI, PlayerSettings, Damage, WinZone, Interactable, Teleport }
	public tpe Type;

	//camera
	[ShowFromMultiple("Type", new string[2] { "0", "6" }, "enum", ShowFromMultipleAttribute.mode.or)]
	public SLM_BH_Camera camera;
	[ShowFromEnum("Type", 0)]
	public bool changeSpeed;
	[ShowFromMultiple(new string[2] {"Type", "changeSpeed"}, new string[2] { "0", "true" }, new string[2] { "enum", "bool" }, ShowFromMultipleAttribute.mode.and)]
	public float speed = 2f;
	[ShowFromEnum("Type", 0)]
	public bool changeOffset;
	[ShowFromMultiple(new string[2] { "Type", "changeOffset" }, new string[2] { "0", "true" }, new string[2] { "enum", "bool" }, ShowFromMultipleAttribute.mode.and)]
	public Vector2 offset;
	[ShowFromEnum("Type", 0)]
	public bool changeBorder;
	[ShowFromMultiple(new string[2] { "Type", "changeBorder" }, new string[2] { "0", "true" }, new string[2] { "enum", "bool" }, ShowFromMultipleAttribute.mode.and)]
	public Vector2 min;
	[ShowFromMultiple(new string[2] { "Type", "changeBorder" }, new string[2] { "0", "true" }, new string[2] { "enum", "bool" }, ShowFromMultipleAttribute.mode.and)]
	public Vector2 max;
	[ShowFromEnum("Type", 0)]
	public bool changeSize;
	[ShowFromMultiple(new string[2] { "Type", "changeSize" }, new string[2] { "0", "true" }, new string[2] { "enum", "bool" }, ShowFromMultipleAttribute.mode.and)]
	public float size;
	[ShowFromMultiple(new string[2] { "Type", "changeSize" }, new string[2] { "0", "true" }, new string[2] { "enum", "bool" }, ShowFromMultipleAttribute.mode.and)]
	public float speedchange = 1;

	//enemy AI
	[ShowFromEnum("Type", 1)]
	public SLM_BH_Enemy enemy;
	[ShowFromEnum("Type", 1)]
	public bool isActive = true;
	[ShowFromMultiple(new string[2] { "Type", "isActive" }, new string[2] { "1", "true" }, new string[2] { "enum", "bool" }, ShowFromMultipleAttribute.mode.and)]
	public bool dontChangeAI;
	[ShowFromMultiple(new string[3] { "Type", "isActive", "dontChangeAI" }, new string[3] { "1", "true", "false" }, new string[3] { "enum", "bool", "bool" }, ShowFromMultipleAttribute.mode.and)]
	public SLM_BH_Enemy.aitpe AIType;
	[ShowFromMultiple(new string[4] { "Type", "AIType", "isActive", "dontChangeAI" }, new string[4] { "1", "0", "true", "false" }, new string[4] { "enum", "enum", "bool", "bool" }, ShowFromMultipleAttribute.mode.and)]
	public SLM_BH_Enemy_EasyAI easyAI;
	[ShowFromMultiple(new string[4] { "Type", "AIType", "isActive", "dontChangeAI" }, new string[4] { "1", "1", "true", "false" }, new string[4] { "enum", "enum", "bool", "bool" }, ShowFromMultipleAttribute.mode.and)]
	public SLM_BH_Enemy_MiddleAi middleAI;
	[ShowFromMultiple(new string[4] { "Type", "AIType", "isActive", "dontChangeAI" }, new string[4] { "1", "2", "true", "false" }, new string[4] { "enum", "enum", "bool", "bool" }, ShowFromMultipleAttribute.mode.and)]
	public SLM_BH_Enemy_HardAI hardAI;
	[ShowFromMultiple(new string[4] { "Type", "AIType", "isActive", "dontChangeAI" }, new string[4] { "1", "3", "true", "false" }, new string[4] { "enum", "enum", "bool", "bool" }, ShowFromMultipleAttribute.mode.and)]
	public SLM_BH_Enemy_TriggersAI triggerAI;

	//Player
	[ShowFromMultiple("Type", new string[3] { "2", "3", "6" }, "enum", ShowFromMultipleAttribute.mode.or)]
	public SLM_BH_Player player;

	[ShowFromEnum("Type", 2)]
	public float PlayerSpeed = 10f;
	[ShowFromEnum("Type", 2)]
	public float PlyaerSpeedShift = 5f;

	//Win
	[ShowFromMultiple("Type", new string[] { "4", "3", "5" }, "enum", ShowFromMultipleAttribute.mode.or)]
	public SLM_BH_Controll controll;

	//Damage
	[ShowFromEnum("Type", 3)]
	public int damage;
	[ShowFromEnum("Type", 3)]
	public string bulletName;
	[ShowFromEnum("Type", 3)]
	public bool isEnemy;

	[ShowFromMultiple("Type", new string[2] { "0", "2" }, "enum", ShowFromMultipleAttribute.mode.or)]
	public bool disableOnExit;

	[ShowFromEnum("Type", 5)]
	public string intractableButton;
	[ShowFromEnum("Type", 5)]
	public SLM_BH_Trigger_DataEvent intractableEvent;
	[ShowFromEnum("Type", 5)]
	public bool showObjectWhenCanIntractable;
	[ShowFromMultiple(new string[2] { nameof(Type), nameof(showObjectWhenCanIntractable) }, new string[2] { "5", "true" }, new string[2] { "enum", "bool" }, ShowFromMultipleAttribute.mode.and)]
	public GameObject objectWhenCanIntractable;

	//Teleport
	public enum tpMode { target, position};
	[ShowFromEnum("Type", 6)]
	public tpMode teleportMode;
	[ShowFromMultiple(new string[2] { nameof(Type), nameof(teleportMode) }, new string[2] { "6", "0" }, "enum", ShowFromMultipleAttribute.mode.and)]
	public Transform teleportTarget;
	[ShowFromMultiple(new string[2] { nameof(Type), nameof(teleportMode) }, new string[2] { "6", "1" }, "enum", ShowFromMultipleAttribute.mode.and)]
	public Vector2 teleportPos;
	[ShowFromEnum("Type", 6)]
	public float teleportDelay;
	[ShowFromEnum("Type", 6)]
	public bool freezeWhenDelay;
	[ShowFromEnum("Type", 6)]
	public bool teleportCameraToo;



	//save
	[HideInInspector]
	public float sspeed = 2f;
	[HideInInspector]
	public Vector2 soffset;
	[HideInInspector]
	public bool suseBorder;
	[HideInInspector]
	public Vector2 smin;
	[HideInInspector]
	public Vector2 smax;
	[HideInInspector]
	public float ssize;
	[HideInInspector]
	public float sPlayerSpeed;
	[HideInInspector]
	public float sPlyaerspeedShift;
	[HideInInspector]
	public bool canInteractable;
}

[System.Serializable]
public class SLM_BH_Trigger_DataEvent
{
	public UnityEvent Event;
}