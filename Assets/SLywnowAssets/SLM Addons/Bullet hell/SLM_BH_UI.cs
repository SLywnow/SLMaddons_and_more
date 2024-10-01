using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AutoLangSLywnow;
using SLywnow;

public class SLM_BH_UI : MonoBehaviour
{
    public enum tpe { playerHP, playerUlts, waveTime, waveEnemy, WaveNum, enemyHP, enemyAITime, enemyAINum, score};
    public tpe type;
    public SLM_BH_Controll controll;
    public SLM_BH_Wave wave;
    public SLM_BH_Enemy enemy;
    public SLM_BH_UI copyfrom;
    public SLM_BH_UI_Opt options;

    float savecur = 0;
    bool havesome;

	private void Update()
    {
        if (controll == null && wave == null && enemy == null && copyfrom != null)
        {
            controll = copyfrom.controll;
            wave = copyfrom.wave;
            enemy = copyfrom.enemy;
        }

        if (type==tpe.playerHP && controll!=null)
		{
            if (controll.maxhp > 0)
                UpdateUI(controll.hp, controll.maxhp);
        }
        else if (type == tpe.playerUlts && controll != null)
        {
            if (controll.maxult > 0)
                UpdateUI(controll.ult, controll.maxult);
        }
        else if (type == tpe.waveTime && wave != null)
        {
            if (wave.maxtimeWave > 0)
                UpdateUI(wave.time, wave.maxtimeWave);
        }
        else if (type == tpe.waveEnemy && wave != null)
        {
            if (wave.maxenemy > 0)
            UpdateUI(wave.enemyslist.Count, wave.maxenemy);
        }
        else if (type == tpe.WaveNum && wave != null)
        {
            if (wave.maxwave > 0)
                UpdateUI(wave.curwave, wave.maxwave);
        }
        else if (type == tpe.enemyHP && enemy != null)
        {
            UpdateUI(enemy.hp, enemy.maxhp);
        }
        else if (type == tpe.enemyAITime && enemy != null)
        {
            if (enemy.maxtime > 0)
                UpdateUI(enemy.pattimer, enemy.maxtime);
        }
        else if (type == tpe.enemyAINum && enemy != null)
        {
            if (enemy.maxpat > 0)
                UpdateUI(enemy.curpat, enemy.maxpat);
        }
        else if (type==tpe.score)
            UpdateUI(controll.score, int.MaxValue);

        if (controll == null && wave == null && enemy == null && havesome)
		{
            havesome = false;
            UpdateUI(options.defaultCur, options.defaultMax);
        }
    }

    void UpdateUI(float cur, float max)
	{
        if (savecur != cur && max > 0)
        {
            savecur = cur;
            havesome = true;

            float percent = cur / max;

            if (options.UIOption == SLM_BH_UI_Opt.uitpe.image)
                options.image.fillAmount = percent;

            //dont touch that!!!
            else if (options.UIOption == SLM_BH_UI_Opt.uitpe.slider)
            {
                options.slider.maxValue = max;
                options.slider.minValue = 0;
                options.slider.value = cur;
            }

            else if (options.UIOption == SLM_BH_UI_Opt.uitpe.objects)
            {
                if (options.parerent.childCount > savecur)
                {
                    for (int i = options.parerent.childCount - 1; i > savecur - 1 && i >= 0; i--)
                        Destroy(options.parerent.GetChild(i).gameObject);
                }
                else if (options.parerent.childCount < savecur)
                {
                    for (int i = options.parerent.childCount; i < savecur; i++)
                    {
                        GameObject obj = Instantiate(options.obj, options.parerent);
                        obj.SetActive(true);
                    }
                }
            }

            string prefix;
            string suffix;
            {
                if (options.useALSLForPrefix)
                    prefix = ALSL_Main.GetWorldAndFindKeys(options.prefix);
                else
                    prefix = options.prefix;
                if (options.useALSLForSuffix)
                    suffix = ALSL_Main.GetWorldAndFindKeys(options.suffix);
                else
                    suffix = options.suffix;
            }

            if (options.textOption == SLM_BH_UI_Opt.txttpe.cur)
                options.text.text = prefix + System.Math.Round(cur, options.round) + suffix;

            else if (options.textOption == SLM_BH_UI_Opt.txttpe.percent)
                options.text.text = prefix + System.Math.Round(percent * 100, options.round) + suffix;

            else if (options.textOption == SLM_BH_UI_Opt.txttpe.curMax)
                options.text.text = prefix + System.Math.Round(cur, options.round) + options.separator + System.Math.Round(max, options.round) + suffix;
        }
	}
}

[System.Serializable]
public class SLM_BH_UI_Opt
{
    public enum uitpe {off, objects, image, slider };
    public uitpe UIOption;
    [ShowFromEnum(nameof(UIOption),1)]
    public GameObject obj;
    [ShowFromEnum(nameof(UIOption), 1)]
    public Transform parerent;
    [ShowFromEnum(nameof(UIOption), 2)]
    public Image image;
    [ShowFromEnum(nameof(UIOption), 3)]
    public Slider slider;

    public enum txttpe { off, curMax, cur, percent };
    [Space(10)]
    public txttpe textOption;
    [ShowFromEnum(nameof(textOption), 0, true)]
    public Text text;
    [ShowFromEnum(nameof(textOption), 0, true)]
    public bool useALSLForPrefix;
    [ShowFromEnum(nameof(textOption), 0, true)]
    public bool useALSLForSuffix;
    [ShowFromEnum(nameof(textOption), 0, true)]
    public string separator = "/";
    [ShowFromEnum(nameof(textOption), 0, true)]
    public string prefix;
    [ShowFromEnum(nameof(textOption), 0, true)]
    public string suffix;
    [ShowFromEnum(nameof(textOption), 0, true)]
    public int round = 2;

    [Space(10)]
    public float defaultCur=0;
    public float defaultMax=100;
}