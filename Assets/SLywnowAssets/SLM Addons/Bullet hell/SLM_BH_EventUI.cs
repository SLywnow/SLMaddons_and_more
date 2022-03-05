using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SLM_BH_EventUI : MonoBehaviour
{
    public enum checkM { weapon, preseedShift, enemyHardAI, wave };
    public enum viewM { none, enableByTime, onlyByVoid };
    public enum renderM { image, sprite };
    public enum findM { select, tag, type, fromControll};

    public GameObject mainobj;
    public checkM checkMode;
    public viewM viewMode;
    public renderM renderMode;
    public findM findMode;

    public Image image;
    public SpriteRenderer sprite;
    public float time;
    public string tag;

    public SLM_BH_Controll controll;
    public SLM_BH_Player player;
    public SLM_BH_Enemy enemy;

    public List<Sprite> weaponSprites;
    public Sprite preseedShiftSpriteOn;
    public Sprite preseedShiftSpriteOff;
    public List<Sprite> enemyHardAISprites;

    float timer=-1;

	private void Start()
	{
        Find();
    }

    public void Find()
	{
        if (findMode == findM.tag)
        {
            if (checkMode == checkM.weapon || checkMode == checkM.preseedShift)
            {
                player = GameObject.FindGameObjectWithTag(tag).GetComponent<SLM_BH_Player>();
            }
            else if (checkMode == checkM.enemyHardAI)
            {
                enemy = GameObject.FindGameObjectWithTag(tag).GetComponent<SLM_BH_Enemy>();
            }
        }
        else if (findMode == findM.type)
        {
            if (checkMode == checkM.weapon || checkMode == checkM.preseedShift)
            {
                player = FindObjectOfType<SLM_BH_Player>();
            }
            else if (checkMode == checkM.enemyHardAI)
            {
                enemy = FindObjectOfType<SLM_BH_Enemy>();
            }
        }
        UpdateImage();
    }

    int checkInt;
    bool checkBool;
    SLM_BH_Player checkPL;
    private void Update()
	{
        if (timer != -1)
        {
            if (timer>0) timer -= Time.deltaTime;
            else
			{
                mainobj.SetActive(false);
                timer = -1;
			}
        }

        if (findMode == findM.tag || findMode == findM.select || findMode == findM.type)
        {
            if (checkMode == checkM.weapon)
            {
                if (checkInt != player.curshoot)
				{
                    UpdateImage();
                    checkInt = player.curshoot;
                }
            }
            else if (checkMode == checkM.preseedShift)
            {
                if (checkBool != player.nowShift)
                {
                    UpdateImage();
                    checkBool = player.nowShift;
                }
            }
            else if (checkMode == checkM.enemyHardAI)
            {
                if (checkInt != enemy.curpat)
                {
                    UpdateImage();
                    checkInt = enemy.curpat;
                }
            }
        }
        else  if (findMode == findM.fromControll)
		{
            if (checkPL != controll.curpl && controll.curpl != null)
			{
                checkPL = controll.curpl;
                UpdateImage();
            }

            if (checkMode == checkM.weapon && controll.curpl!=null)
            {
                if (checkInt != controll.curpl.curshoot)
                {
                    UpdateImage();
                    checkInt = controll.curpl.curshoot;
                }
            }
            else if (checkMode == checkM.preseedShift && controll.curpl != null)
            {
                if (checkBool != controll.curpl.nowShift)
                {
                    UpdateImage();
                    checkBool = controll.curpl.nowShift;
                }
            }
        }
    }

    public void UpdateImage()
	{
        Sprite selected = null;

        if (checkMode == checkM.weapon)
        {
            if (findMode == findM.tag || findMode == findM.select || findMode == findM.type)
            {
                if (player !=null && player.curshoot >= 0 && player.curshoot < weaponSprites.Count)
                    selected = weaponSprites[player.curshoot];
            }
            else if (findMode == findM.fromControll)
            {
                if (controll.curpl !=null && controll.curpl.curshoot >= 0 && controll.curpl.curshoot < weaponSprites.Count)
                    selected = weaponSprites[controll.curpl.curshoot];
            }
        }
        else if (checkMode == checkM.preseedShift)
        {
            if (findMode == findM.tag || findMode == findM.select || findMode == findM.type)
            {
                if (player != null)
                {
                    if (player.nowShift)
                        selected = preseedShiftSpriteOn;
                    else
                        selected = preseedShiftSpriteOff;
                }
            }
            else if (findMode == findM.fromControll)
            {
                if (controll.curpl != null)
				{
                    if (controll.curpl.nowShift)
                        selected = preseedShiftSpriteOn;
                    else
                        selected = preseedShiftSpriteOff;
                }
            }
        }
        else if (checkMode == checkM.enemyHardAI)
        {
            if (findMode == findM.tag || findMode == findM.select || findMode == findM.type)
            {
                if (enemy != null && enemy.curpat >= 0 && enemy.curpat < enemyHardAISprites.Count)
                    selected = enemyHardAISprites[enemy.curpat];
            }
            else if (findMode == findM.fromControll)
            {
                Debug.LogError("Script can't get enemy from Comtroll, change find Mode!");
            }
        }

        if (renderMode == renderM.image)
            image.sprite = selected;
        else if (renderMode == renderM.sprite)
            sprite.sprite = selected;
    }

	public void Show()
    {
        mainobj.SetActive(true);
        UpdateImage();
    }

    public void Hide()
	{
        mainobj.SetActive(false);
    }
}
