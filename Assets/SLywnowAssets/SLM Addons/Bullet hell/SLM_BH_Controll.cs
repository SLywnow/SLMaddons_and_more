using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SLywnow;
using UnityEngine.Events;
using static UbhUtil;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SLM_BH_Controll : MonoBehaviour
{
	/*
	public enum fileMd { off,Object, stringInput};
	public fileMd fileMode;
	[ShowFromEnum("fileMode",1)]
	public Object presetFile;
	[ShowFromEnum("fileMode", 2)]
	public string json;
	[ShowFromEnum("fileMode", 0, true)]
	public SLM_BH_Player playerBase;
	[ShowFromEnum("fileMode", 0, true)]
	public SLM_BH_Enemy enemyBase;
	[ShowFromEnum("fileMode", 0, true)]
	public List<Sprite> sprites;
	[ShowFromEnum("fileMode", 0, true)]
	public List<UbhShotCtrl> shoots;*/

	SLM_AddonManager AddonManager;
	public SLM_Commands Commands;
	public SLM_BH_Wave waveController;

	public int hp=3;
	public int score=0;
	public int ult=0;
	[HideInInspector] public int maxhp = -1;
	[HideInInspector] public int maxult = -1;

	[Tooltip ("Destoy and spawn object, not change Active state")] public bool perfabmode;
	public List<SLM_BH_Controll_Preset> presets;

	public UnityEvent onStartBulletHell;
	public UnityEvent onEndBulletHell;

	public bool stopcommands=true;

	public int damagebyEnemy=1;
	public List<SLM_BH_Controll_DamageControl> damageControl;

	public string enemyTag;
	public string playerTag;
	
	public SLM_BH_AutoLoad autoLoad;

	[HideInInspector] public int runid = -1;
	int winC;
	int loseC;
	GameObject spmain;
	GameObject spbg;
	GameObject spplayer;
	GameObject spenemy;

	[HideInInspector] 
	public SLM_BH_Player curpl;
	[HideInInspector]
	public SLM_BH_Trigger lastTrigger;
	[HideInInspector] 
	public List<string> damageName;
	[HideInInspector] 
	public List<int> damages;
	[HideInInspector] 
	public List<int> damagesCount;
	[HideInInspector] 
	public string lastdamage;
	[HideInInspector] 
	public string lastenemydamage;
	[HideInInspector]
	public bool stopCamera;

	public void Awake()
	{
		AddonManager = Commands.addonManager;
		stopCamera = false;

		if (AddonManager != null)
		{
			freezetime = -1;
			maxhp = -1;
			maxult = -1;
			AddonManager.AddAddon("Bullet Hell", Command, new List<string>() { "runbh", "runbhcustom", "changeweapon", "resedamagecounts", "resedamagecount", "isdamagecount", "islastplayerdamage", "islastenemydamage", "runwave", "bhadd", "bhset", "playground", "forceendbh", "bhget", "isbiggestdamagecount" }, new List<bool>() { !stopcommands, !stopcommands, true, true, true, false, false, false, !stopcommands, true, true, true, false, true, false });

			UpdateDamageControll();

			if (autoLoad.useAutoLoad)
			{
				if (!SaveSystemAlt.IsWorking())
					SaveSystemAlt.StartWork();

				if (!string.IsNullOrEmpty(autoLoad.KeyToAutoshoot))
					autoLoad.autoShoot = SaveSystemAlt.GetBool(autoLoad.KeyToAutoshoot, autoLoad.AutoshootDef);
				else
					autoLoad.autoShoot = autoLoad.AutoshootDef;

				if (!string.IsNullOrEmpty(autoLoad.KeyToStaticBG))
					autoLoad.StaticScreen = SaveSystemAlt.GetBool(autoLoad.KeyToStaticBG, autoLoad.staticBGDef);
				else
					autoLoad.StaticScreen = autoLoad.staticBGDef;

				if (!string.IsNullOrEmpty(autoLoad.KeyToSpeed))
					autoLoad.speed = SaveSystemAlt.GetFloat(autoLoad.KeyToSpeed, autoLoad.speedDef);
				else
					autoLoad.speed = autoLoad.speedDef;

				if (!string.IsNullOrEmpty(autoLoad.KeyToShiftSpeed))
					autoLoad.shiftSpeed = SaveSystemAlt.GetFloat(autoLoad.KeyToShiftSpeed, autoLoad.shiftSpeedDef);
				else
					autoLoad.shiftSpeed = autoLoad.shiftSpeedDef;
			}
		}
	}

	void UpdateDamageControll()
	{
		damageName = new List<string>();
		damages = new List<int>();
		damagesCount = new List<int>();
		foreach (SLM_BH_Controll_DamageControl d in damageControl)
		{
			damageName.Add(d.name);
			damages.Add(d.damage);
			damagesCount.Add(0);
		}
	}

	public bool Command(string comm, string[] args)
	{
		switch (comm)
		{
			case "runbh":
				{
					//run bh with preset
					winC = Commands.ValueWorkCommand(args[1]);
					loseC = Commands.ValueWorkCommand(args[2]);

					bool intMode = false;

					try
					{
						Commands.ValueWorkInt(args[0]);
						intMode = true;
					}
					catch { }

					if (intMode)
						RunBH(Commands.ValueWorkInt(args[0]));
					else
						RunBH(Commands.ValueWorkString(args[0]));

					return true;
				}
			case "runbhcustom":
				{
					//run bh with custom setup
					winC = Commands.ValueWorkCommand(args[1]);
					loseC = Commands.ValueWorkCommand(args[2]);

					bool intMode = false;

					try
					{
						Commands.ValueWorkInt(args[0]);
						intMode = true;
					}
					catch { }

					if (intMode)
						RunBH(Commands.ValueWorkInt(args[0]), true, Commands.ValueWorkInt(args[3]), Commands.ValueWorkInt(args[4]), Commands.ValueWorkInt(args[5]));
					else
						RunBH(Commands.ValueWorkString(args[0]), true, Commands.ValueWorkInt(args[3]), Commands.ValueWorkInt(args[4]), Commands.ValueWorkInt(args[5]));

					return true;
				}
			case "changeweapon":
				{
					//change player weapon
					ChangeWeapon(Commands.ValueWorkBool(args[0]));
					return true;
				}
			case "resedamagecounts":
				{
					//reset all damageCounts
					for (int i = 0; i < damagesCount.Count; i++)
						damagesCount[i] = 0;

					return true;
				}
			case "resedamagecount":
				{
					//reset damagecount by name
					int id = damageName.IndexOf(Commands.ValueWorkString(args[0]));
					if (id > 0 && id < damagesCount.Count)
						damagesCount[id] = 0;

					return true;
				}
			case "isdamagecount":
				{
					//isdamagecount::name::count::true::false
					int id = damageName.IndexOf(Commands.ValueWorkString(args[0]));
					int count = Commands.ValueWorkInt(args[1]);
					int tru = Commands.ValueWorkCommand(args[2]);
					int flse = Commands.ValueWorkCommand(args[3]);
					if (id >= 0 && id < damagesCount.Count)
					{
						if (damagesCount[id] >= count)
							Commands.RunCommand(tru);
						else
							Commands.RunCommand(flse);
					}
					else
					{
						Commands.Error("Name " + Commands.ValueWorkString(args[0]) + " not exist in DamageControll");
						return false;
					}

					return true;
				}
			case "isbiggestdamagecount":
				{
					//isbiggestdamagecount::name::true::false
					int id = damageName.IndexOf(Commands.ValueWorkString(args[0]));
					int tru = Commands.ValueWorkCommand(args[1]);
					int flse = Commands.ValueWorkCommand(args[2]);
					if (id >= 0 && id < damagesCount.Count)
					{
						if (damagesCount.IndexOf(damagesCount.Max()) == id)
							Commands.RunCommand(tru);
						else
							Commands.RunCommand(flse);
					}
					else
					{
						Commands.Error("Name " + Commands.ValueWorkString(args[0]) + " not exist in DamageControll");
						return false;
					}

					return true;
				}
			case "islastplayerdamage":
				{
					//islastplayerdamage::name::true::false
					int id = damageName.IndexOf(Commands.ValueWorkString(args[0]));
					int tru = Commands.ValueWorkCommand(args[1]);
					int flse = Commands.ValueWorkCommand(args[2]);
					if (id >= 0 && id < damagesCount.Count)
					{
						if (lastdamage == Commands.ValueWorkString(args[0]))
							Commands.RunCommand(tru);
						else
							Commands.RunCommand(flse);
					}
					else
					{
						Commands.Error("Name " + Commands.ValueWorkString(args[0]) + " not exist in DamageControll");
						return false;
					}

					return true;
				}
			case "islastenemydamage":
				{
					//islastplayerdamage::name::true::false
					int id = damageName.IndexOf(Commands.ValueWorkString(args[0]));
					int tru = Commands.ValueWorkCommand(args[1]);
					int flse = Commands.ValueWorkCommand(args[2]);
					if (id >= 0 && id < damagesCount.Count)
					{
						if (lastenemydamage == Commands.ValueWorkString(args[0]))
							Commands.RunCommand(tru);
						else
							Commands.RunCommand(flse);
					}
					else
					{
						Commands.Error("Name" + Commands.ValueWorkString(args[0]) + " not exist in DamageControll");
						return false;
					}

					return true;
				}
			case "runwave":
				{
					//runwave::wavenum
					if (waveController.curwave != -1)
					{
						int id = Commands.ValueWorkInt(args[0]);
						waveController.RunWave(id);
					}

					return true;
				}
			case "bhadd":
				{
					//bhadd::whattoadd::count(vw);;
					//whattoadd: hp,ult,score
					int add = Commands.ValueWorkInt(args[1]);
					if (args[0] == "hp")
					{
						hp += add;
						if (hp <= 0)
							EndBH(false);
					}
					else if (args[0] == "ult")
					{
						ult += add;
						if (ult < 0)
							ult = 0;
					}
					else if (args[0] == "score")
					{
						score += add;
						if (score < 0)
							score = 0;
					}
					return true;
				}
			case "bhset":
				{
					//bhadd::whattoadd::count(vw);;
					//whattoadd: hp,ult,score
					int add = Commands.ValueWorkInt(args[1]);
					if (args[0] == "hp")
					{
						hp = add;
						if (hp <= 0)
							EndBH(false);
					}
					else if (args[0] == "ult")
					{
						ult = add;
						if (ult < 0)
							ult = 0;
					}
					else if (args[0] == "score")
					{
						score = add;
						if (score < 0)
							score = 0;
					}
					return true;
				}
			case "bhget":
				{
					//bhget::whattoget::valuename;; - get to VMS_Stats some value

					string valuename = Commands.ValueWorkString(args[1]);

					if (args[0] == "hp")
					{
						Commands.stats.SetValue(valuename, hp);
					}
					else if (args[0] == "ult")
					{
						Commands.stats.SetValue(valuename, ult);
					}
					else if (args[0] == "score")
					{
						Commands.stats.SetValue(valuename, score);
					}
					else if (args[0] == "lastenemybullet")
					{
						//last bullet that shoot player
						Commands.stats.SetValue(valuename, lastdamage);
					}
					else if (args[0] == "lastplayerbullet")
					{
						//last bullet that shoot enemy
						Commands.stats.SetValue(valuename, lastenemydamage);
					}

					return true;
				}
			case "playground":
				{
					/*
					 * playground::imortality::time;; - set imortality to player
					 * playground::freeze::player::time;; - freeze player
					 * playground::freeze::enemy::time;; - freeze all enemies
					 * playground::freeze::bulelts::time;;
					 * playground::freeze::enemy&bullets::time;;
					 * playground::freeze::all::time;; - freeze all
					 * playground::clear::tag;; - clear all bullets with tag
					 * playground::weapon::id;; - set weapon by id
					 * playground::weapon::next;; - set next weapon
					 * playground::weapon::prev;; - set prev weapon
					 * playground::camera::stop/run;; - set prev weapon
					 * playground::damage::player/enemy::count - damage player or all enemies
					 */

					if (args[0] == "imortality")
					{
						float time = Commands.ValueWorkFloat(args[1]);

						SetPlayerImutineTimer(time);
					}
					else if (args[0] == "freeze")
					{
						float time = Commands.ValueWorkFloat(args[2]);

						if (args[1] == "player")
						{
							FreezeAllPlayers(time);
						}
						else if (args[1] == "enemy")
						{
							FreezeAllEnemy(time);
						}
						else if (args[1] == "bulelts")
						{
							FreezeAllBullets(time);
						}
						else if (args[1] == "enemy&bullets")
						{
							FreezeBulletsAndEnemies(time);
						}
						else if (args[1] == "all")
						{
							FreezeAll(time);
						}
					}
					else if (args[0] == "clear")
					{
						string tag = Commands.ValueWorkString(args[1]);

						ClearBullets(tag);
					}
					else if (args[0] == "weapon")
					{
						if (args[1] == "next")
						{
							ChangeWeapon(false);
						}
						else if (args[1] == "prev")
						{
							ChangeWeapon(true);
						}
						else
						{
							int id = Commands.ValueWorkInt(args[1]);

							SetWeapon(id);
						}
					}
					else if (args[0] == "camera")
					{
						if (args[1] == "run")
						{
							stopCamera = false;
						}
						else if (args[1] == "stop")
						{
							stopCamera = true;
						}
					}
					else if (args[0] == "damage")
					{
						if (args[1] == "player")
						{
							DamagePlayer(Commands.ValueWorkInt(args[2]));
						}
						else if (args[1] == "enemy")
						{
							DamageEnemy(Commands.ValueWorkInt(args[2]));
						}
					}

					return true;
				}
			case "forceendbh":
				{
					//forceendbh::win?::continuefromthatcommand;;


					bool win = Commands.ValueWorkBool(args[0]);
					bool cont = false;

					if (args.Length >= 2)
						cont = Commands.ValueWorkBool(args[1]);

					EndBH(win, cont);

					/*if (runid == -1)
					{
						if (!cont)
						{
							Commands.Error("Bullet hell isn't running, set continueFromThatCommand true to solve this error!");
							return false;
						}
					}*/
					
					if (cont)
					{
						if (Commands.blocks[Commands.currentid].autorunnextcommand)
							Commands.RunNextCommand();
					}

					return true;
				}
		}

		return false;
	}


	public void RunBH(string name, bool c = false, int fhp = 0, int fult = -1, int fscore = -1)
	{
		SLM_BH_Controll_Preset p = presets.Find(f => f.name == name);
		if (p != null)
			RunBH(presets.IndexOf(p), c, fhp, fult, fscore);
		else
			Commands.Error("BulletHell preset " + name + " not found!");
	}

	public void RunBH(int id, bool c=false, int fhp=0, int fult=-1, int fscore=-1)
	{
		if (id < presets.Count && id >= 0)
		{
			runid = id;
			if (perfabmode)
			{
				if (presets[runid].main != null)
				{
					spmain = Instantiate(presets[runid].main);
					spmain.SetActive(true);
				}
				if (presets[runid].bg != null)
				{
					spbg = Instantiate(presets[runid].bg);
					spbg.SetActive(true);
				}
				if (presets[runid].player != null)
				{
					spplayer = Instantiate(presets[runid].player).gameObject;
					spplayer.name = presets[runid].player.name;
					spplayer.SetActive(true);
					curpl = spplayer.GetComponent<SLM_BH_Player>();
				}
				if (presets[runid].enemy != null)
				{
					spenemy = Instantiate(presets[runid].enemy);
					spenemy.name = presets[runid].enemy.name;
					spenemy.SetActive(true);
				}
			}
			else
			{
				if (presets[runid].main != null)
					presets[runid].main.SetActive(true);
				if (presets[runid].bg != null)
					presets[runid].bg.SetActive(true);
				if (presets[runid].player != null)
				{
					presets[runid].player.gameObject.SetActive(true);
					curpl = presets[runid].player;
					presets[runid].player.Restart();
				}
				if (presets[runid].enemy != null)
					presets[runid].enemy.SetActive(true);
			}

			if (autoLoad.useAutoLoad && curpl.moveMode != SLM_BH_Player.MoveMode.rigitbody)
			{
				curpl.speed = autoLoad.speed;
				curpl.speedShift = autoLoad.shiftSpeed;
				curpl.autoShoot = autoLoad.autoShoot;
			}

			if (waveController != null && presets[runid].waveId >= 0)
			{
				waveController.RunWavePreset(presets[runid].waveId);
			}

			if (!c)
			{
				if (presets[runid].foseSetHp > 0)
				{
					hp = presets[runid].foseSetHp;
					maxhp = hp;
				}
				else if (maxhp == -1)
					maxhp = hp;

				if (presets[runid].foseSetUlt > 0)
				{
					ult = presets[runid].foseSetUlt;
					maxult = ult;
				}
				else if (maxult == -1)
					maxult = ult;

				if (presets[runid].foseSetScore > 0)
					score = presets[runid].foseSetScore;
			}
			else
			{
				if (fhp > 0)
				{
					hp = fhp;
					maxhp = hp;
				}
				else if (maxhp == -1)
					maxhp = hp;

				if (fult >= 0)
				{
					ult = fult;
					maxult = ult;
				}
				else if (maxult == -1)
					maxult = ult;

				if (fscore >= 0)
					score = fscore;
			}


			onStartBulletHell.Invoke();

		}
	}

	//to use in UnityEvent
	public void EndBH(bool win)
	{
		EndBH(win, false);
	}

	public void EndBH(bool win, bool dontruncommand = false)
	{
		if (runid != -1)
		{
			if (runid < presets.Count)
			{
				if (waveController != null)
					waveController.StopAllWaves();

				curpl = null;

				if (perfabmode)
				{
					if (spmain != null)
						Destroy(spmain);
					if (spbg != null)
						Destroy(spbg);
					if (spplayer != null)
						Destroy(spplayer);
					if (spenemy != null)
						Destroy(spenemy);
				}
				else
				{
					if (presets[runid].bg != null)
						presets[runid].bg.SetActive(false);
					if (presets[runid].player != null)
					{
						{
							presets[runid].player.gameObject.SetActive(false);
						}
					}
					if (presets[runid].enemy != null)
						presets[runid].enemy.SetActive(false);

					if (presets[runid].main != null)
						presets[runid].main.SetActive(false);
				}

				runid = -1;

				if (!dontruncommand)
				{
					if (win)
						Commands.RunCommand(winC);
					else
						Commands.RunCommand(loseC);
				}
			}

			onEndBulletHell.Invoke();
		}
	}

	public void ChangeWeapon(bool prev)
	{
		if (curpl !=null)
		{
			curpl.UpdateShoot(prev);
		}
	}
	public void SetWeapon(int id)
	{
		if (curpl != null)
		{
			curpl.SetShoot(id);
		}
	}

	public void DamagePlayer(int damage)
	{
		if (curpl !=null)
		{
			curpl.Damage("", false, damage);
		}
	}

	public void DamageEnemy(int damage)
	{
		if (!string.IsNullOrEmpty(enemyTag))
		{
			List<GameObject> en = GameObject.FindGameObjectsWithTag(enemyTag).ToList();

			foreach (GameObject g in en)
				if (g.activeInHierarchy)
				{
					if (g.GetComponent<SLM_BH_Enemy>() != null)
						g.GetComponent<SLM_BH_Enemy>().ForceDamage(damage);
					else if (g.GetComponentInParent<SLM_BH_Enemy>() != null)
						g.GetComponentInParent<SLM_BH_Enemy>().ForceDamage(damage);
				}
		}
	}

	public void SetPlayerImutineTimer(float time)
	{
		if (!string.IsNullOrEmpty(playerTag))
		{
			List<GameObject> en = GameObject.FindGameObjectsWithTag(playerTag).ToList();

			foreach (GameObject g in en)
				if (g.activeInHierarchy)
				{
					if (g.GetComponent<SLM_BH_Player>() != null)
						g.GetComponent<SLM_BH_Player>().imutineTimer = time;
					else if (g.GetComponentInParent<SLM_BH_Player>() != null)
						g.GetComponentInParent<SLM_BH_Player>().imutineTimer = time;
				}
		}
	}

	public void ClearBullets(string tag)
	{
		GameObject pool = GameObject.Find("UbhObjectPool");
		if (pool != null)
			for (int i = pool.transform.childCount - 1; i >= 0; i--)
				if (string.IsNullOrEmpty(tag) || pool.transform.GetChild(i).tag == tag)
					Destroy(pool.transform.GetChild(i).gameObject);
	}


	public void FreezeAll(float time)
	{
		if ((time > 0 || time == -1))
		{
			FreezeAllBullets(time);

			FreezeAllEnemy(time);

			FreezeAllPlayers(time);
		}
	}

	public void FreezeBulletsAndEnemies(float time)
	{
		if ((time > 0 || time ==-1) && GameObject.Find("UbhTimer") !=null)
		{
			FreezeAllBullets(time);

			FreezeAllEnemy(time);
		}
	}

	float freezetime=-1;
	private void Update()
	{
		if (freezetime != -1)
		{
			if (freezetime >= 0)
				freezetime -= Time.deltaTime;
			else
			{
				UbhTimer t = GameObject.Find("UbhTimer").GetComponent<UbhTimer>();
				t.Resume();
				freezetime = -1;
			}
		}
	}

	public void FreezeAllEnemy(float time)
	{
		if (time > 0 || time ==-1)
		{
			if (!string.IsNullOrEmpty(enemyTag))
			{
				List<GameObject> en = GameObject.FindGameObjectsWithTag(enemyTag).ToList();

				foreach (GameObject g in en)
					if (g.activeInHierarchy)
					{
						if (g.GetComponent<SLM_BH_Enemy>() != null)
							g.GetComponent<SLM_BH_Enemy>().freezetime = time;
						else if (g.GetComponentInParent<SLM_BH_Enemy>() != null)
							g.GetComponentInParent<SLM_BH_Enemy>().freezetime = time;
					}
			}
		}
	}

	public void FreezeAllPlayers(float time)
	{
		if (time > 0 || time ==-1)
		{
			if (curpl !=null)
			{
				curpl.freezetime = time;
			}
			else if (!string.IsNullOrEmpty(playerTag))
			{
				List<GameObject> en = GameObject.FindGameObjectsWithTag(playerTag).ToList();

				foreach (GameObject g in en)
					if (g.activeInHierarchy)
					{
						if (g.GetComponent<SLM_BH_Player>() != null)
							g.GetComponent<SLM_BH_Player>().freezetime = time;
						else if (g.GetComponentInParent<SLM_BH_Player>() != null)
							g.GetComponentInParent<SLM_BH_Player>().freezetime = time;
					}
			}
		}
	}

	public void FreezeAllBullets(float time)
	{
		if ((time > 0 || time == -1) && GameObject.Find("UbhTimer") != null)
		{
			UbhTimer t = GameObject.Find("UbhTimer").GetComponent<UbhTimer>();
			t.Pause();
			freezetime = time;
		}
	}
}

[System.Serializable]
public class SLM_BH_Controll_Preset
{
	public string name;
	public int foseSetHp =-1;
	public int foseSetUlt = -1;
	public int foseSetScore = -1;
	public GameObject main;
	public int waveId;
	public GameObject bg;
	public SLM_BH_Player player;
	public GameObject enemy;
}

[System.Serializable]
public class SLM_BH_Controll_DamageControl
{
	public string name;
	public int damage;
}


[System.Serializable]
public class SLM_BH_View
{
	public tpe TypeOfMove;
	public tpedown TypeOfDamage;
	public tpedown TypeOfDie;
	public SLM_BH_Sprites sprites;
	public SLM_BH_Animator animator;
	public SLM_BH_Animation animation;

	public enum tpe { sprites,animator, animation};
	public enum tpedown { animator, animation };

	/// <summary>
	/// 
	/// </summary>
	/// <param name="dir">r,l,u,d</param>
	public void Move(string dir)
	{
		if (dir == "r")
		{
			if (TypeOfMove == tpe.sprites && sprites.moveRight != null)
			{
				sprites.spriteRenderer.sprite = sprites.moveRight;
				sprites.spriteRenderer.flipX = false;
			}
			if (TypeOfMove == tpe.animation && !string.IsNullOrEmpty(animation.moveRight))
				animation.animator.Play(animation.moveRight);
			if (TypeOfMove == tpe.animator && !string.IsNullOrEmpty(animator.moveRight))
				animator.animator.SetBool(animator.moveRight, true);
		}
		else if (dir == "l")
		{
			if (TypeOfMove == tpe.sprites && sprites.moveRight != null)
			{
				sprites.spriteRenderer.sprite = sprites.moveRight;
				sprites.spriteRenderer.flipX = true;
			}
			if (TypeOfMove == tpe.animation && !string.IsNullOrEmpty(animation.moveLeft))
				animation.animator.Play(animation.moveLeft);
			if (TypeOfMove == tpe.animator && !string.IsNullOrEmpty(animator.moveLeft))
				animator.animator.SetBool(animator.moveLeft, true);
		}
		else if (dir == "u")
		{
			if (TypeOfMove == tpe.sprites && sprites.moveUp != null)
			{
				sprites.spriteRenderer.sprite = sprites.moveUp;
			}
			if (TypeOfMove == tpe.animation && !string.IsNullOrEmpty(animation.moveUp))
				animation.animator.Play(animation.moveUp);
			if (TypeOfMove == tpe.animator && !string.IsNullOrEmpty(animator.moveUp))
				animator.animator.SetBool(animator.moveUp, true);
		}
		else if (dir == "d")
		{
			if (TypeOfMove == tpe.sprites && sprites.moveDown != null)
			{
				sprites.spriteRenderer.sprite = sprites.moveDown;
			}
			if (TypeOfMove == tpe.animation && !string.IsNullOrEmpty(animation.moveDown))
				animation.animator.Play(animation.moveDown);
			if (TypeOfMove == tpe.animator && !string.IsNullOrEmpty(animator.moveDown))
				animator.animator.SetBool(animator.moveDown, true);
		}
		else if (dir == "move" && sprites.move != null)
		{
			sprites.spriteRenderer.sprite = sprites.move;
		}
	}

	public void StopMove()
	{
		if (TypeOfMove == tpe.sprites && sprites.main != null)
		{
			sprites.spriteRenderer.sprite = sprites.main;
			sprites.spriteRenderer.flipX = false;
		}
		if (TypeOfMove == tpe.animation && !string.IsNullOrEmpty(animation.main))
			animation.animator.Play(animation.main);
		if (TypeOfMove == tpe.animator && !string.IsNullOrEmpty(animator.main))
			animator.animator.SetBool(animator.main, true);
	}

	public void Damage()
	{
		if (TypeOfDamage == tpedown.animation && !string.IsNullOrEmpty(animation.damage))
			animation.animator.Play(animation.damage);
		if (TypeOfDamage == tpedown.animator && !string.IsNullOrEmpty(animation.damage))
			animator.animator.SetBool(animation.damage, true);
	}

	public void Die()
	{
		if (TypeOfDie == tpedown.animation && !string.IsNullOrEmpty(animation.die))
			animation.animator.Play(animation.die);
		if (TypeOfDie == tpedown.animator && !string.IsNullOrEmpty(animation.die))
			animator.animator.SetBool(animation.die, true);
	}
}

	[System.Serializable]
public class SLM_BH_Sprites
{
	public SpriteRenderer spriteRenderer;
	public bool RotateToMoveDir;
	[ShowFromBool("RotateToMoveDir", true)]
	public int rotationOffset;

	public Sprite main;

	[ShowFromBool("RotateToMoveDir", true)]
	public Sprite move;

	[ShowFromBool("RotateToMoveDir",false)]
	public Sprite moveRight;
	[ShowFromBool("RotateToMoveDir", false)]
	public Sprite moveUp;
	[ShowFromBool("RotateToMoveDir", false)]
	public Sprite moveDown;
	public Sprite Damage;
}

[System.Serializable]
public class SLM_BH_Animator
{
	public Animator animator;
	public string main;
	public string moveRight;
	public string moveLeft;
	public string moveUp;
	public string moveDown;
	public string damage;
	public string die;
}

[System.Serializable]
public class SLM_BH_Animation
{
	public Animation animator;
	public string main;
	public string moveRight;
	public string moveLeft;
	public string moveUp;
	public string moveDown;
	public string damage;
	public string die;
}

[System.Serializable]
public class SLM_BH_AutoLoad
{
	public bool useAutoLoad=false;
	[ShowFromBool("useAutoLoad")]
	public string KeyToAutoshoot;
	[ShowFromBool("useAutoLoad")]
	public bool AutoshootDef=false;
	[ShowFromBool("useAutoLoad")]
	public string KeyToSpeed;
	[ShowFromBool("useAutoLoad")]
	public float speedDef=10f;
	[ShowFromBool("useAutoLoad")]
	public string KeyToShiftSpeed;
	[ShowFromBool("useAutoLoad")]
	public float shiftSpeedDef=5f;
	[ShowFromBool("useAutoLoad")]
	public string KeyToStaticBG;
	[ShowFromBool("useAutoLoad")]
	public bool staticBGDef=false;

	[HideInInspector] public bool autoShoot;
	[HideInInspector] public bool StaticScreen;
	[HideInInspector] public float speed;
	[HideInInspector] public float shiftSpeed;
}

/*
[System.Serializable]
public class SLM_BH_JSON
{
	//player
	public List<SLM_BH_JSON_Player> players;

	//enemies
	public List<SLM_BH_JSON_Enemies> enemies;

	//wave
	public List<SLM_BH_JSON_Waves> waves;
}

[System.Serializable]
public class SLM_BH_JSON_Player
{
	public bool useShift;
	public bool useUltimate;


}

[System.Serializable]
public class SLM_BH_JSON_Enemies
{
	

}

[System.Serializable]
public class SLM_BH_JSON_Waves
{
	

}
*/