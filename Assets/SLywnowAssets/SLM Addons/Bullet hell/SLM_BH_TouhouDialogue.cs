using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SLM_BH_TouhouDialogue : MonoBehaviour
{
    public SLM_Commands C;
    public Color off;
    public float speed = 0.4f;

    List<Image> imgs;
    List<string> names;
    List<int> worked;
    List<bool> direction;
    List<float> curent;

    void Awake()
    {
        C.addonManager.AddAddon("Touhou Dialogues", Command, new List<string> { "thdshow", "thdhide" }, new List<bool> { true, true });

        imgs = new List<Image>();
        names = new List<string>();
        worked = new List<int>();
        direction = new List<bool>();
        curent = new List<float>();
        foreach (SLM_NovelImages_Block b in C.imagesc.layers)
        {
            imgs.Add(b.imageobj);
            names.Add(b.name);
        }
    }

    public bool Command(string comm, string[] args)
    {
        switch (comm)
        {
            case "thdshow":
				{
                    //thdshow::name(NovelImages);;
                    if (names.Contains(C.ValueWorkString(args[0])))
                    {
                        int id = names.IndexOf(C.ValueWorkString(args[0]));
                        if (worked.Contains(id))
						{
                            int idd = worked.IndexOf(id);
                            direction[idd] = true;
                        }
                        else
						{
                            worked.Add(id);
                            direction.Add(true);
                            curent.Add(0);
                        }
                    }
                    else
                    {
                        C.Error("Layer with " + C.ValueWorkString(args[0]) + " not found!");
                        return false;
                    }

                    return true;
				}
            case "thdhide":
                {
                    //thdshow::name(NovelImages);;

                    if (names.Contains(C.ValueWorkString(args[0])))
                    {
                        int id = names.IndexOf(C.ValueWorkString(args[0]));
                        if (worked.Contains(id))
                        {
                            int idd = worked.IndexOf(id);
                            direction[idd] = false;
                        }
                        else
                        {
                            worked.Add(id);
                            direction.Add(false);
                            curent.Add(1);
                        }
                    }
                    else
                    {
                        C.Error("Layer with " + C.ValueWorkString(args[0]) + " not found!");
                        return false;
                    }

                    return true;
                }
            default:
				{
                    return false;
				}
        } 
    }

    void Update()
    {
        for (int i=0;i<worked.Count;i++)
		{
            int id = worked[i];

            if (direction[i])
            {
                if (imgs[id].color==Color.white)
				{
                    direction.RemoveAt(i);
                    curent.RemoveAt(i);
                    worked.RemoveAt(i);
                } 
                else
				{
                    curent[i] += speed * Time.deltaTime;
                    curent[i] = Mathf.Clamp01(curent[i]);
                    imgs[id].color = new Color(Mathf.Lerp(off.r, Color.white.r, curent[i]), Mathf.Lerp(off.g, Color.white.g, curent[i]), Mathf.Lerp(off.b, Color.white.b, curent[i]), Mathf.Lerp(off.a, Color.white.a, curent[i]));

                }
            }
            else
			{
                if (imgs[id].color == off)
                {
                    direction.RemoveAt(i);
                    curent.RemoveAt(i);
                    worked.RemoveAt(i);
                }
                else
                {
                    curent[i] -= speed * Time.deltaTime;
                    curent[i] = Mathf.Clamp01(curent[i]);
                    imgs[id].color = new Color(Mathf.Lerp(off.r, Color.white.r, curent[i]), Mathf.Lerp(off.g, Color.white.g, curent[i]), Mathf.Lerp(off.b, Color.white.b, curent[i]), Mathf.Lerp(off.a, Color.white.a, curent[i]));
                }
            }
        }
    }
}
