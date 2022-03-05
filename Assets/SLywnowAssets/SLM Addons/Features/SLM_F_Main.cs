using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SLM_F_Main : MonoBehaviour
{
    SLM_AddonManager AM;
    public SLM_Commands C;
	public Transform imagesMain;
	public Image flashImage;

	public List<Text> texts;
	public List<Image> images;
	public List<Sprite> sprites;
	public List<GameObject> objects;
	public List<SLM_F_GG> ggs;

	void Awake()
    {
		AM = C.addonManager;
		AM.AddAddon("Small Features", Command, new List<string>() { "settextstring", "setimagesprite", "objecton", "objectoff", "objectswitch", "objectset", "shaking", "downshaking", "stopshaking", "flash", "gg" }, new List<bool>() { true, true, true, true, true, true, true, true, true, true, false });

    }

	private void Start()
	{
		if (imagesMain != null)
			defpos = imagesMain.position;

		foreach (SLM_F_GG g in ggs)
			g.obj.SetActive(false);
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
					objects[C.ValueWorkInt(args[0])].SetActive(true);
					return true;
				}
			case "objectoff":
				{
					objects[C.ValueWorkInt(args[0])].SetActive(false);
					return true;
				}
			case "objectswitch":
				{
					objects[C.ValueWorkInt(args[0])].SetActive(!objects[C.ValueWorkInt(args[0])].activeSelf);
					return true;
				}
			case "objectset":
				{
					objects[C.ValueWorkInt(args[0])].SetActive(C.ValueWorkBool(args[1]));
					return true;
				}
			case "shaking":
				{
					if (imagesMain != null)
					{
						//shaking::time::shift::speed;; time =-1 is inf
						shakeTime = C.ValueWorkFloat(args[0]);
						shakeAmount = C.ValueWorkInt(args[1]);
						shakeSpeed = C.ValueWorkFloat(args[2]);
						downshaking = false;
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
						downshaking = true;
						shakeAmount = C.ValueWorkInt(args[1]);
						shakeSpeed = C.ValueWorkFloat(args[2]);
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
								return false;
							}
						}
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
		}

		return false;
	}

	float flashtimer = -1;
	float flashmaxtime;
	Color flashColor;
	float shakeTime;
	int shakeAmount;
	float shakeSpeed;
	float maxtime;
	bool downshaking;

	Vector2 defpos;
	Vector2 topos;

	private void Update()
	{
		if (flashtimer !=-1)
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
			if (shakeTime > 0 || shakeTime ==-1)
			{
				if (shakeTime != -1)
					shakeTime -= Time.deltaTime;

				if (Mathf.Round(pos.x) == Mathf.Round(topos.x) && Mathf.Round(pos.y) == Mathf.Round(topos.y))
					SetPos();
				else
					imagesMain.position = Vector2.MoveTowards(imagesMain.position, topos, shakeSpeed * 1000 * Time.deltaTime);
			}
			else
			{
				shakeTime = 0;
				topos = defpos;
				if (Mathf.Round(pos.x) == Mathf.Round(topos.x) && Mathf.Round(pos.y) == Mathf.Round(topos.y))
				{
					imagesMain.position = defpos;
					downshaking = false;
				}
				else
					imagesMain.position = Vector2.MoveTowards(imagesMain.position, topos, shakeSpeed * 1000 * Time.deltaTime);
			}
		}
	}

	void SetPos()
	{
		if (!downshaking)
			topos = new Vector2(defpos.x + (shakeAmount * (Random.Range((int)0, (int)2) == 0 ? 1 : -1)), defpos.y + (shakeAmount * (Random.Range((int)0, (int)2) == 0 ? 1 : -1)));
		else
		{
			if (shakeTime>0)
			{
				float curshakeamount = shakeAmount * shakeTime / maxtime;
				topos = new Vector2(defpos.x + (curshakeamount * (Random.Range((int)0, (int)2) == 0 ? 1 : -1)), defpos.y + (curshakeamount * (Random.Range((int)0, (int)2) == 0 ? 1 : -1)));
			}
		}
	}
}

[System.Serializable]
public class SLM_F_GG
{
	public string name;
	public GameObject obj;
}