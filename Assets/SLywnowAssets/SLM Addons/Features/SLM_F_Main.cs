using SLywnow;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SLM_F_Main : MonoBehaviour
{
   SLM_AddonManager AM;
   public SLM_Commands C;
	public Transform imagesMain;
	public Image flashImage;
	public float shakeMultiplier = 1;

	public List<Text> texts;
	public List<Image> images;
	public List<Sprite> sprites;
	public List<GameObject> objects;
	public List<SLM_F_GG> ggs;
	public List<SLM_F_Anim> anims;
	public List<SLM_F_Animator> animators;
	public List<SLM_F_Timer> timers;
	public SLM_F_Slider_Auto autosliders;
	public List<SLM_F_Slider> sliders;

	void Awake()
    {
		if (shakeMultiplier == 0)
			shakeMultiplier = 1;

		AM = C.addonManager;
		AM.AddAddon("Small Features", Command, new List<string>() { "settextstring", "setimagesprite", "objecton", "objectoff", "objectswitch", "objectset", "shaking", "downshaking", "stopshaking", "flash", "flashimage", "gg", "timer", "anim", "sliderwork", "animator" }, new List<bool>() { true, true, true, true, true, true, true, true, true, true, true, false, true, true, true, true });

    }

	private void Start()
	{
		if (imagesMain != null)
			defpos = imagesMain.position;

		foreach (SLM_F_GG g in ggs)
			g.obj.SetActive(false);
	}

	public void RunCommand(string cmd)
	{
		List<string> argsL = cmd.Split("::").ToList();
		string command = argsL[0];
		argsL.RemoveAt(0);

		string[] args = argsL.ToArray();

		Command(command, args);
	}

	public bool Command(string comm, string[] args)
	{
		switch (comm)
		{
			case "settextstring":
				{
					texts[C.ValueWorkInt(args[0])].text = C.ValueWorkString(args[1]);
					return true;
				}
			case "setimage":
				{
					images[C.ValueWorkInt(args[0])].sprite = sprites[C.ValueWorkInt(args[1])];
					return true;
				}
			case "objecton":
				{
					int id = GetObjectId(args[0]);
					if (id != -1)
						objects[id].SetActive(true);

					return true;
				}
			case "objectoff":
				{
					int id = GetObjectId(args[0]);
					if (id != -1)
						objects[id].SetActive(false);
					return true;
				}
			case "objectswitch":
				{
					int id = GetObjectId(args[0]);
					if (id != -1)
						objects[id].SetActive(!objects[id].activeSelf);
					return true;
				}
			case "objectset":
				{
					int id = GetObjectId(args[0]);
					if (id != -1)
						objects[id].SetActive(C.ValueWorkBool(args[1]));
					return true;
				}
			case "shaking":
				{
					if (imagesMain != null)
					{
						//shaking::time::shift::speed;; time =-1 is inf
						shakeTime = C.ValueWorkFloat(args[0]);
						maxtime = shakeTime;
						shakeAmount = C.ValueWorkInt(args[1]);
						shakeSpeed = C.ValueWorkFloat(args[2]);
						shaketype = shaketpe.normal;
						SetPos();
					}
					else
						Debug.LogError("Add imagesMain to use shaking!");

					return true;
				}
			case "downshaking":
				{
					if (imagesMain != null)
					{
						//shaking::time::shift::speed;;
						shakeTime = C.ValueWorkFloat(args[0]);
						maxtime = shakeTime;
						shaketype = shaketpe.downshaking;
						shakeAmount = C.ValueWorkInt(args[1]);
						shakeSpeed = C.ValueWorkFloat(args[2]);
						SetPos();
					}
					else
						Debug.LogError("Add imagesMain to use shaking!");

					return true;
				}
			case "impulseshaking":
				{
					if (imagesMain != null)
					{
						//impulseshaking::time::impulsetime::shift::speed;;
						shakeTime = C.ValueWorkFloat(args[0]);
						maxtime = shakeTime;
						impmaxtime = C.ValueWorkFloat(args[1]);
						imptime = C.ValueWorkFloat(args[1]);
						shaketype = shaketpe.impulse;
						shakeAmount = C.ValueWorkInt(args[2]);
						shakeSpeed = C.ValueWorkFloat(args[3]);
						SetPos();
					}
					else
						Debug.LogError("Add imagesMain to use shaking!");

					return true;
				}
			case "stopshaking":
				{
					if (imagesMain != null)
					{
						//stopshaking;;
						shakeTime = 0;
						maxtime = 0;
						imagesMain.position = defpos;
						SetPos();
					}
					else
						Debug.LogError("Add imagesMain to use shaking!");
					return true;
				}
			case "flash":
				{
					//flash::sec::Color(RGBA)(opt)
					if (flashImage != null)
					{
						float time = C.ValueWorkFloat(args[0]);
						flashColor = Color.white;
						if (args.Length >= 2) 
						{
							if (!ColorUtility.TryParseHtmlString(C.ValueWorkString(args[1]), out flashColor))
							{
								C.Error("Can't parse string to Color!");
								return true;
							}
						}
						flashImage.sprite = null;
						flashImage.color = flashColor;
						flashmaxtime = time;
						flashtimer = time;
					}
					return true;
				}
			case "flashimage":
				{
					//flashimage::layer::image(id/name)::sec::Color(RGBA)(opt)
					if (flashImage != null)
					{
						Sprite img = null;
						if (C.imagesc !=null)
						{
							SLM_NovelImages_Block block = C.imagesc.layers.Find(f => f.name == C.ValueWorkString(args[0]));
							if (block != null)
							{
								try
								{
									img = block.sprites[C.ValueWorkInt(args[1])];
								}
								catch
								{
									img = block.sprites.Find(f => f.name == C.ValueWorkString(args[1]));
								}
							}
						}

						float time = C.ValueWorkFloat(args[2]);
						flashColor = Color.white;
						if (args.Length >= 4)
						{
							if (!ColorUtility.TryParseHtmlString(C.ValueWorkString(args[3]), out flashColor))
							{
								C.Error("Can't parse string to Color!");
								return true;
							}
						}
						flashImage.sprite = img;
						flashImage.color = flashColor;
						flashmaxtime = time;
						flashtimer = time;
					}
					return true;
				}
			case "gg":
				{
					SLM_F_GG gg = ggs.Find(g => g.name == C.ValueWorkString(args[0]));
					if (gg != null)
					{
						gg.obj.SetActive(true);
					}
					else
						C.Error("Game Over" + C.ValueWorkString(args[0]) + " not found!");
					return true;
				}
			case "timer":
				{
					string nm = C.ValueWorkString(args[0]);
					SLM_F_Timer find = timers.Find(f => f.name == nm);

					if (find != null)
					{
						if (args[1] == "delete")
						{
							//timer::name::delete;;
							DeleteTimer(find);
						}
						if (args[1] == "connect")
						{
							//timer::name::connect::varname::time/done;;
							string varname = C.ValueWorkString(args[2]);
							bool ondone = args[3] == "done";

							ConnectTimer(find, varname, ondone);
						}
						if (args[1] == "settime")
						{
							//timer::name::settime::time;;
							float time = C.ValueWorkFloat(args[2]);

							SetTimerTime(find, time);
						}
						if (args[1] == "addrntime")
						{
							//timer::name::addrntime::time;;
							float time = C.ValueWorkFloat(args[2]);

							AddRNTime(find, time);
						}
						if (args[1] == "setrntime")
						{
							//timer::name::setrntime::time;;
							float time = C.ValueWorkFloat(args[2]);

							SetRNTime(find, time);
						}
						if (args[1] == "setmultiplier")
						{
							//timer::name::multiplayer::count;;
							float mult = C.ValueWorkFloat(args[2]);

							SetTimerMultiplier(find, mult);
						}
						else if (args[1] == "start")
						{
							//timer::name::start;;

							RunTimer(find);
						}
						else if (args[1] == "stop")
						{
							//timer::name::stop;;

							StopTimer(find);
						}
						else if (args[1] == "pause")
						{
							//timer::name::pause;;

							PauseTimer(find, false, false);
						}
						else if (args[1] == "unpause")
						{
							//timer::name::unpause;;

							PauseTimer(find, true, false);
						}
						else if (args[1] == "changestate")
						{
							//timer::name::changestate;;

							PauseTimer(find, false, true);
						}
						else if (args[1] == "create")
						{
							C.Error("Timer with name " + nm + " already exist, delete old first!");
						}
					}
					else if (args[1] == "create")
					{
						//timer::name::create::varname::time;;
						string varname = C.ValueWorkString(args[2]);
						float tim = C.ValueWorkFloat(args[3]);

						CreateTimer(nm, varname, tim);
					}
					else
						C.Error("Timer with name " + nm + " not found!");

					return true;
				}
			case "sliderwork":
				{
					string nm = C.ValueWorkString(args[0]);
					SLM_F_Slider find = sliders.Find(f => f.name == nm);

					if (find != null)
					{
						if (args[1] == "on")
						{
							//sliderwork::name::on
							OnOffSlider(find, true);
						}
						else if (args[1] == "off")
						{
							//sliderwork::name::off
							OnOffSlider(find, false);
						}
						else if (args[1] == "connect")
						{
							//sliderwork::name::connect::value

							string val = C.ValueWorkString(args[2]);

							ConnectSlider(find, val);
						}
						else if (args[1] == "disconnect")
						{
							//sliderwork::name::disconnect

							DisconnectSlider(find);
						}
						else if (args[1] == "vis")
						{
							if (args[2] == "name")
							{
								//sliderwork::name::vis::name::string;;

								string text = C.ValueWorkString(args[3]);

								SetSliderText(find, text);
							}
							else if (args[2] == "icon")
							{
								//sliderwork::name::vis::icon::select/off::layer::name/id;;

								if (args[3]=="off")
								{
									OnOffSliderIcon(find, false);
								}
								else
								{
									OnOffSliderIcon(find, true);

									string layer = C.ValueWorkString(args[4]);
									string id = "";
									try
									{
										id = C.ValueWorkInt(args[5]) + "";
									}
									catch
									{
										id = C.ValueWorkString(args[5]);
									}

									SetSliderIcon(find, layer, id);
								}
							}
							else if (args[2] == "color")
							{
								//sliderwork::name::vis::color::bg/main::RGBcode;;

								bool bg = args[3] == "bg";

								string color = C.ValueWorkString(args[4]);

								SetSliderColor(find, bg, color);
							}
						}
					}
					else
						C.Error("Slider with name " + nm + " not found!");

					return true;
				}
			case "anim":
				{
					//anim::name
					string nm = C.ValueWorkString(args[0]);
					SLM_F_Anim find = anims.Find(f => f.name == nm);
					if (find != null)
						find.anim.Play(find.name);
					else
						C.Error("Animation " + nm + " not founded!");

					return true;
				}
			case "animator":
				{
					string nm = C.ValueWorkString(args[0]);
					SLM_F_Animator find = animators.Find(f => f.name == nm);

					if (find != null && find.anim != null)
					{
						string valname = C.ValueWorkString(args[2]);

						if (args[1] == "setint")
						{
							//animator::name::setint::valname::val
							int val = C.ValueWorkInt(args[3]);

							find.anim.SetInteger(valname, val);
						}
						if (args[1] == "setfloat")
						{
							//animator::name::setfloat::valname::val
							float val = C.ValueWorkFloat(args[3]);

							find.anim.SetFloat(valname, val);
						}
						if (args[1] == "setbool")
						{
							//animator::name::setbool::valname::val
							bool val = C.ValueWorkBool(args[3]);

							find.anim.SetBool(valname, val);
						}
						if (args[1] == "trigger")
						{
							//animator::name::trigger::valname

							find.anim.SetTrigger(valname);
						}
					}
					else
						C.Error("Animator with name " + nm + " not found!");
					
					return true;
				}
		}

		return false;
	}

	int GetObjectId(string input)
	{
		int ret = -1;

		try
		{
			ret = C.ValueWorkInt(input);
		}
		catch
		{
			GameObject obj = objects.Find(f => f.name == C.ValueWorkString(input));
			if (obj != null)
				ret = objects.IndexOf(obj);
		}

		return ret;

	}

	float flashtimer = -1;
	float flashmaxtime;
	Color flashColor;
	float shakeTime;
	int shakeAmount;
	float shakeSpeed;
	float maxtime;
	float imptime;
	float impmaxtime;
	enum shaketpe { normal, downshaking, impulse };
	shaketpe shaketype;

	Vector2 defpos;
	Vector2 topos;

	private void Update()
	{
		UpdateFlash();
		UpdateTimer();
	}

	#region timer

	void UpdateTimer()
	{
		foreach (SLM_F_Timer t in timers)
		{
			if (!t.pause && t.time>0)
			{
				t.time -= Time.deltaTime * t.multiplier;
				
				if (t.time<=0)
				{
					t.time = 0;
					t.onStop?.Invoke();
					t.pause = true;
					if (!string.IsNullOrEmpty(t.statDone))
						C.stats.SetValue(t.statDone, true);
				}
				if (!string.IsNullOrEmpty(t.statTime))
					C.stats.SetValue(t.statTime, t.time);
			}
		}
	}

	void CreateTimer(string name, string varname, float time)
	{
		timers.Add(new SLM_F_Timer()
		{
			name = name,
			multiplier = 1,
			time = time,
			starttime = time,
			statDone = varname,
			pause = true
		});

		SLM_F_Timer t = timers[timers.Count - 1];
		if (!string.IsNullOrEmpty(t.statDone))
			C.stats.SetValue(t.statDone, false);
	}

	void RunTimer(SLM_F_Timer timer)
	{
		timer.pause = false;
		timer.time = timer.starttime;

		if (!string.IsNullOrEmpty(timer.statDone))
			C.stats.SetValue(timer.statDone, false);
	}

	void ConnectTimer(SLM_F_Timer timer, string varname, bool ondone)
	{
		if (!string.IsNullOrEmpty(varname))
		{
			if (ondone)
			{
				timer.statDone = varname;

				if (!string.IsNullOrEmpty(timer.statDone))
					if (timer.pause && timer.time == 0)
						C.stats.SetValue(timer.statDone, true);
					else
						C.stats.SetValue(timer.statDone, false);
			}
			else
			{
				timer.statTime = varname;

				if (!string.IsNullOrEmpty(timer.statTime))
				{
					C.stats.SetValue(timer.statTime, timer.time);
					C.stats.SetValueMinMax(timer.statTime, 0, timer.starttime);
				}
			}
		}
		else
			C.Error("Enter name of stat first!");
	}

	void StopTimer(SLM_F_Timer timer)
	{
		timer.time = 0;
		timer.onStop?.Invoke();
		timer.pause = true;

		if (!string.IsNullOrEmpty(timer.statDone))
			C.stats.SetValue(timer.statDone, true);
		if (!string.IsNullOrEmpty(timer.statTime))
			C.stats.SetValue(timer.statTime, timer.time);
	}

	void AddRNTime(SLM_F_Timer timer, float add)
	{
		if (timer.time != 0 && timer.time != timer.starttime)
			timer.time = Mathf.Clamp(timer.time + add, 0, timer.starttime);
	}

	void SetRNTime(SLM_F_Timer timer, float time)
	{
		if (timer.time != 0 && timer.time != timer.starttime)
			timer.time = Mathf.Clamp(time, 0, timer.starttime);
	}

	void SetTimerTime(SLM_F_Timer timer, float time)
	{
		timer.starttime = time;
		if (!string.IsNullOrEmpty(timer.statTime))
		{
			C.stats.SetValueMinMax(timer.statTime, 0, timer.starttime);
		}
	}

	void SetTimerMultiplier(SLM_F_Timer timer, float mult)
	{
		timer.multiplier =mult;
	}

	void PauseTimer(SLM_F_Timer timer, bool unpause, bool changestat=false)
	{
		if (timer.time == timer.starttime || timer.time == 0)
		{
			return;
		}

		if (changestat)
		{
			timer.pause = !timer.pause;
		}
		else
		{
			timer.pause = !unpause;
		}
	}

	void DeleteTimer(SLM_F_Timer timer)
	{
		timers.Remove(timer);
	}

	#endregion

	#region sliders

	[Button("Get all Sliders", ButtonMode.DisabledInPlayMode)]
	void AutoGetSliders()
	{
		foreach (SLM_F_Slider s in sliders)
		{
			if (s.parentObj !=null)
			{
				s.logo = FastFind.FindChild(s.parentObj.transform, autosliders.logo).GetComponent<TMPro.TMP_Text>();
				s.icon = FastFind.FindChild(s.parentObj.transform, autosliders.icon).GetComponent<Image>();
				s.slider = FastFind.FindChild(s.parentObj.transform, autosliders.slider).GetComponent<Slider>();
				s.backround = FastFind.FindChild(s.parentObj.transform, autosliders.backround).GetComponent<Image>();
				s.fillArea = FastFind.FindChild(s.parentObj.transform, autosliders.fillArea).GetComponent<Image>();
			}
		}
	}

	void OnOffSlider(SLM_F_Slider slider, bool on)
	{
		slider.parentObj.SetActive(on);
	}

	void ConnectSlider(SLM_F_Slider slider, string varname)
	{
		SLM_Stats_Block stat = C.stats.stats.Find(f => f.name == varname);

		if (stat != null)
		{
			if (stat.type == SLM_Stats.tpe.intType || stat.type == SLM_Stats.tpe.floatType)
			{
				if (stat.min != float.MinValue && stat.max != float.MaxValue)
				{
					if (slider.connectedval != null)
						DisconnectSlider(slider);

					stat.gui.slider = slider.slider;
					stat.gui.type = SLM_Stats_GUI.tpe.slider;
					slider.connectedval = stat;

					slider.slider.value = float.Parse(stat.value) / stat.max;
				}
				else
					C.Error("Stat exist, but min/max not setted. Use setstatminmax::name::min||max first!");
			}
			else
				C.Error("Stat exist, but it's type not int or float!");
		}
		else
			C.Error("Stat with name " + varname + " not exist!");
	}

	void DisconnectSlider(SLM_F_Slider slider)
	{
		if (slider.connectedval !=null)
		{
			slider.connectedval.gui.type = SLM_Stats_GUI.tpe.off;
			slider.connectedval.gui.slider = null;
		}
	}

	void OnOffSliderIcon(SLM_F_Slider slider, bool on)
	{
		slider.icon.gameObject.SetActive(on);
	}

	void SetSliderIcon(SLM_F_Slider slider, string layer, string img)
	{
		SLM_NovelImages_Block l = C.imagesc.layers.Find(f => f.name == layer);
		if (l !=null)
		{
			Sprite image = null;

			if (int.TryParse(img, out int i))
			{
				if (i >= 0 && i < l.sprites.Count)
					image = l.sprites[i];
			}
			else
			{
				image = l.sprites.Find(f => f.name == img);
			}

			if (image != null)
			{
				slider.icon.sprite = image;
			}
			else
				C.Error("Layer founded, but image " + img + " not exist!");
		}
		else
		{
			C.Error("Image layer with name " + layer + " not found!");
		}
	}

	void SetSliderText(SLM_F_Slider slider, string text)
	{
		slider.logo.text = text;
	}

	void SetSliderColor(SLM_F_Slider slider, bool bg, string rgb)
	{
		Color color = Color.white;
		if (!ColorUtility.TryParseHtmlString(rgb, out color))
		{
			C.Error("Can't parse string to Color!");
			return;
		}

		if (bg)
			slider.backround.color = color;
		else
			slider.fillArea.color = color;
	}

	#endregion

	#region flash&shaking
	void UpdateFlash()
	{
		if (flashtimer != -1)
		{
			if (flashImage != null)
			{
				if (flashtimer > 0)
				{
					flashtimer -= Time.deltaTime;
					flashColor.a = flashtimer / flashmaxtime;
					flashImage.color = flashColor;
				}
				else
				{
					flashColor.a = 0;
					flashImage.color = flashColor;
					flashtimer = -1;
				}
			}
			else
				flashtimer = -1;
		}

		if (imagesMain != null)
		{
			Vector2 pos = imagesMain.position;
			if (shakeTime > 0 || maxtime == -1)
			{
				if (maxtime != -1)
					shakeTime -= Time.deltaTime;

				if (shaketype == shaketpe.impulse)
				{
					if (imptime > 0)
					{
						imptime -= Time.deltaTime;
					}
					else
					{
						imptime = impmaxtime;
						SetPos();
					}
				}

				if (System.Math.Round(pos.x,2) == System.Math.Round(topos.x,2) && System.Math.Round(pos.y,2) == System.Math.Round(topos.y,2))
					SetPos();
				else
					imagesMain.position = Vector2.MoveTowards(imagesMain.position, topos, shakeSpeed * 1000 * Time.deltaTime);
			}
			else
			{
				shakeTime = 0;
				topos = defpos;
				if (System.Math.Round(pos.x, 2) == System.Math.Round(topos.x, 2) && System.Math.Round(pos.y, 2) == System.Math.Round(topos.y, 2))
				{
					imagesMain.position = defpos;
					shaketype = shaketpe.normal;
				}
				else
					imagesMain.position = Vector2.MoveTowards(imagesMain.position, topos, shakeSpeed * 1000 * Time.deltaTime);
			}
		}
	}

	void SetPos()
	{
		if (shaketype == shaketpe.normal)
			topos = new Vector2(defpos.x + (shakeMultiplier * shakeAmount * (Random.Range((int)0, (int)2) == 0 ? 1 : -1)), defpos.y + (shakeMultiplier * shakeAmount * (Random.Range((int)0, (int)2) == 0 ? 1 : -1)));
		else if (shaketype == shaketpe.downshaking)
		{
			if (shakeTime > 0)
			{
				float curshakeamount = shakeMultiplier * shakeAmount * shakeTime / maxtime;
				topos = new Vector2(defpos.x + (curshakeamount * (Random.Range((int)0, (int)2) == 0 ? 1 : -1)), defpos.y + (curshakeamount * (Random.Range((int)0, (int)2) == 0 ? 1 : -1)));
			}
		}
		else if (shaketype == shaketpe.impulse)
		{
			if (impmaxtime > 0)
			{
				float curshakeamount = shakeMultiplier * shakeAmount * imptime / impmaxtime;
				topos = new Vector2(defpos.x + (curshakeamount * (Random.Range((int)0, (int)2) == 0 ? 1 : -1)), defpos.y + (curshakeamount * (Random.Range((int)0, (int)2) == 0 ? 1 : -1)));
			}
		}
	}

	#endregion
}

[System.Serializable]
public class SLM_F_GG
{
	public string name;
	public GameObject obj;
}


[System.Serializable]
public class SLM_F_Anim
{
	public string name;
	public Animation anim;
}

[System.Serializable]
public class SLM_F_Animator
{
	public string name;
	public Animator anim;
}

[System.Serializable]
public class SLM_F_Timer
{
	public string name;
	public float time;
	public float multiplier;
	public bool pause;
	public float starttime;
	public string statDone;
	public string statTime;
	public UnityEvent onStop;
	public UnityEvent onPause;
	public UnityEvent onUnpause;
}

[System.Serializable]
public class SLM_F_Slider
{
	public string name;
	public SLM_Stats_Block connectedval;
	public GameObject parentObj;
	public Slider slider;
	public Image backround;
	public Image fillArea;
	public Image icon;
	public TMPro.TMP_Text logo;
}

[System.Serializable]
public class SLM_F_Slider_Auto
{
	public string slider;
	public string backround;
	public string fillArea;
	public string icon;
	public string logo;
}