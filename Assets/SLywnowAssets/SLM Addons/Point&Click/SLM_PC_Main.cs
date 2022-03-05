using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SLywnow;
using AutoLangSLywnow;
using UnityEngine.UI;
using UnityEngine.Events;

public class SLM_PC_Main : MonoBehaviour
{
    [Header("Scripts")]
    SLM_AddonManager AM;
    public SLM_Commands C;
    SLM_ALSLBridge B;
    SLM_NovelImages I;

    [Header("Spawn")]
    public SLM_PC_Object item;
    public Transform parent;
    public List<Sprite> sprites;

    [Header("UI")]
    public int maxInventoryItems = int.MaxValue;
    public SLM_PC_UI ui;

    [HideInInspector] public SLM_PC_Setup setup;
    [HideInInspector] public List<int> inventory;
    [HideInInspector] public List<int> inventoryLast;

    UnityAction curentactionspawn;
    UnityAction curentactiondespawn;
    SLM_PC_Save savedata;

    void Awake()
    {
        if (C != null)
        {
            AM = C.addonManager;
            B = C.ALSLBridge;
            I = C.imagesc;
            curentactiondespawn = null;
            curentactionspawn = null;

            inventory = new List<int>();
            inventoryLast = new List<int>();
            foreach (SLM_PC_ObjectSet o in setup.objs)
                inventory.Add(0);

            if (ui.bigBlock != null)
                ui.bigBlock.gameObject.SetActive(false);
            if (ui.smallBlock != null)
                ui.smallBlock.gameObject.SetActive(false);
            if (ui.bigMain != null)
                ui.bigMain.gameObject.SetActive(false);
            if (ui.closeBigButton != null)
                ui.closeBigButton.gameObject.SetActive(false);

            if (ui.smallMain != null)
                ui.smallMain.gameObject.SetActive(true);
            if (ui.openBigButton != null)
                ui.openBigButton.gameObject.SetActive(true);

            UpdateUI();

            AM.AddAddon("Point&Click", Command, new List<string>() { "runpcpreset", "offpcpreset", "disableobject", "enableobject", "hasobject", "addobject", "setobject", "objectcounttostat", "saveinventory", "loadinventory", "hasitemsininventory", "clearinventory" }, new List<bool>() { true, true, true, true, false, true, true, true, true, true, false, true });
        }
    }

    public bool Command(string comm, string[] args)
    {
        switch (comm)
        {
            case "runpcpreset":
                {
                    //runpcpreset::id

                    int id = C.ValueWorkInt(args[0]);

                    if (id >= 0 && id < setup.presets.Count)
                    {
                        if (setup.presets[id].useImageAsParrent)
                            RunWorkPreset(id, false);
                        else
                            RunPreset(id);
                    }

                    return true;
                }
            case "offpcpreset":
                {
                    //runpcpreset::id

                    int id = C.ValueWorkInt(args[0]);

                    if (id >= 0 && id < setup.presets.Count)
                    {
                        if (setup.presets[id].useImageAsParrent)
                            RunWorkPreset(id, true);
                        else
                            DisablePreset(id);
                    }

                    return true;
                }
            case "disableobject":
                {
                    //disableobject::preset::id

                    int id = C.ValueWorkInt(args[0]);

                    if (id >= 0 && id < setup.presets.Count || id==-1)
                    {
                        int idd = C.ValueWorkInt(args[1]);
                        if (idd>=0 && idd< setup.presets[id].objsupd.Count)
                        {
                            setup.presets[id].objsupd[idd].gameObject.SetActive(false);
                        }
                        if (idd >= 0 && idd < setup.presets[id].objs.Count)
						{
                            setup.presets[id].objs[idd].disabled = true;
                        }
                    }

                    return true;
                }
            case "enableobject":
                {
                    //enableobject::preset::id::count=1
                    //count <= 0 - no change count

                    int id = C.ValueWorkInt(args[0]);
                    
                    int count = 1;
                    if (args.Length >= 3)
                        count = C.ValueWorkInt(args[2]);

                    if (id >= 0 && id < setup.presets.Count)
                    {
                        int idd = C.ValueWorkInt(args[1]);
                        if (idd >= 0 && idd < setup.presets[id].objsupd.Count)
                        {
                            setup.presets[id].objsupd[idd].gameObject.SetActive(true);
                            if (count >0)
                                setup.presets[id].objsupd[idd].count = count;
                        }
                        if (idd >= 0 && idd < setup.presets[id].objs.Count)
                        {
                            setup.presets[id].objs[idd].disabled = false;
                            if (count >0)
                                setup.presets[id].objs[idd].count = count;
                        }
                    }

                    return true;
                }
            case "hasobject":
                {
                    //hasobject::id::count::true::false - check some object (-1 is any count) 

                    int id = C.ValueWorkInt(args[0]);
                    int count = C.ValueWorkInt(args[1]);
                    int tr = C.ValueWorkCommand(args[2]);
                    int fl = C.ValueWorkCommand(args[3]);

                    if (id >= 0 && id < inventory.Count)
                    {
                        if (count !=-1)
						{
                            if (inventory[id] >= count)
                                C.RunCommand(tr);
                            else
                                C.RunCommand(fl);
						}
                        else
						{
                            if (inventory[id] >= 1)
                                C.RunCommand(tr);
                            else
                                C.RunCommand(fl);
                        }
                    }

                    return true;
                }
            case "addobject":
                {
                    //addobject::id::count - add some count to inventory

                    int id = C.ValueWorkInt(args[0]);
                    int count = C.ValueWorkInt(args[1]);

                    if (id >= 0 && id < inventory.Count)
                    {
                        if (inventoryLast.Count < maxInventoryItems || inventoryLast.Contains(id))
                        {
                            inventory[id] = Mathf.Clamp(inventory[id] + count, 0, setup.objs[id].maxAmount);
                            if (inventoryLast.Contains(id))
                            {
                                if (count > 0)
                                    inventoryLast.Move(inventoryLast.IndexOf(id), inventoryLast.Count - 1);
                                else if (count < 0 && inventory[id] == 0)
                                    inventoryLast.Remove(id);
                            }
                            else if (count > 0)
                                inventoryLast.Add(id);
                        }
                    }

                    UpdateUI();
                    return true;
                }
            case "setobject":
                {
                    //setobject::id::count - force set object count
                        int id = C.ValueWorkInt(args[0]);
                        int count = C.ValueWorkInt(args[1]);

                    if (inventoryLast.Count < maxInventoryItems || inventoryLast.Contains(id))
                    {
                        if (id >= 0 && id < inventory.Count && count >= 0)
                        {
                            inventory[id] = count;
                            inventory[id] = Mathf.Clamp(inventory[id], 0, setup.objs[id].maxAmount);
                        }

                        if (inventoryLast.Contains(id))
                        {
                            if (count > 0)
                                inventoryLast.Move(inventoryLast.IndexOf(id), inventoryLast.Count - 1);
                            else
                                inventoryLast.Remove(id);
                        }
                        else if (count > 0)
                            inventoryLast.Add(id);

                        UpdateUI();
                    }

                    return true;
                }
            case "objectcounttostat":
                {
                    //hasobject::stat::id - place object count to stat

                    string stat = C.ValueWorkString(args[0]);
                    int id = C.ValueWorkInt(args[1]);

                    if (id >= 0 && id < inventory.Count)
                    {
                        C.stats.SetValue(stat, id);
                    }

                    return true;
                }
            case "hasitemsininventory":
                {
                    //hasitemsininventory::count::true::false - place object count to stat

                    int count = C.ValueWorkInt(args[0]);
                    int Ctrue = C.ValueWorkCommand(args[1]);
                    int Cfalse = C.ValueWorkCommand(args[2]);

                    if (count<=inventoryLast.Count)
                    {
                        C.RunCommand(Ctrue);
                    }
                    else
                        C.RunCommand(Cfalse);

                    return true;
                }
            case "clearinventory":
                {
                    //clearinventory;;

                    for(int i =0;i<inventory.Count;i++)
                        inventory[i] = 0;

                    inventoryLast = new List<int>();

                    UpdateUI();
                    return true;
                }
            case "saveinventory":
                {
                    //saveinventory::name;; - save all inventory state
                    //save: disabled, count, inventory

                    string key = C.ValueWorkString(args[0]);

                    savedata = new SLM_PC_Save();
                    savedata.inventory = inventory;
                    savedata.inventoryLast = inventoryLast;
                    savedata.presets = new List<SLM_PC_Presets>();

                    for (int p=0;p<setup.presets.Count;p++)
					{
                        savedata.presets.Add(new SLM_PC_Presets());
                        savedata.presets[p].objs = new List<SLM_PC_SaveObjectData>();
                        for (int o=0;o< setup.presets[p].objs.Count;o++)
						{
                            savedata.presets[p].objs.Add(new SLM_PC_SaveObjectData());
                            savedata.presets[p].objs[o].disable = setup.presets[p].objs[o].disabled;
                            savedata.presets[p].objs[o].randomenable = setup.presets[p].objs[o].randomenable;
                            savedata.presets[p].objs[o].count = setup.presets[p].objs[o].count;
                        }
                    }
                    SaveSystemAlt.SetString(key, JsonUtility.ToJson(savedata));
                    SaveSystemAlt.SaveUpdatesNotClose();

                    return true;
                }
            case "loadinventory":
                {
                    //loadinventory::id;; - load all inventory state

                    string key = C.ValueWorkString(args[0]);

                    if (SaveSystemAlt.HasKey(key))
					{
                        try
                        {
                            savedata = JsonUtility.FromJson<SLM_PC_Save>(SaveSystemAlt.GetString(key, "{ }"));


                            for (int i = 0; i < inventory.Count; i++)
                                if (i < savedata.inventory.Count)
                                    inventory[i] = savedata.inventory[i];

                            for (int i = 0; i < inventoryLast.Count; i++)
                                if (i < savedata.inventoryLast.Count)
                                    inventoryLast.Add(savedata.inventoryLast[i]);

                            for (int p = 0; p < setup.presets.Count; p++)
                            {
                                if (p < savedata.presets.Count)
                                {
                                    for (int o = 0; o < setup.presets[p].objs.Count; o++)
                                    {
                                        if (o<savedata.presets[p].objs.Count)
										{
                                            setup.presets[p].objs[o].count = savedata.presets[p].objs[o].count;
                                            setup.presets[p].objs[o].disabled = savedata.presets[p].objs[o].disable;
                                            setup.presets[p].objs[o].randomenable = savedata.presets[p].objs[o].randomenable;
                                        }
                                    }
                                }
                            }
                        }
                        catch { }
					}

                    UpdateUI();
                    return true;
                }
            case "pcuioff":
                {
                    if (ui.smallMain != null)
                        ui.smallMain.SetActive(false);
                    if (ui.bigMain != null)
                        ui.bigMain.SetActive(false);
                    if (ui.closeBigButton != null)
                        ui.closeBigButton.gameObject.SetActive(false);
                    if (ui.openBigButton != null)
                        ui.openBigButton.gameObject.SetActive(false);

                    ui.visible = false;

                    return true;
                }
            case "pcuion":
                {
                    if (ui.smallMain != null)
                        ui.smallMain.SetActive(true);
                    if (ui.bigMain != null)
                        ui.bigMain.SetActive(false);
                    if (ui.closeBigButton != null)
                        ui.closeBigButton.gameObject.SetActive(false);
                    if (ui.openBigButton != null)
                        ui.openBigButton.gameObject.SetActive(true);

                    ui.visible = true;
                    UpdateUI();

                    return true;
                }
        }
        return false;
    }

    public void RunWorkPreset(int id, bool disable)
	{
        if (disable)
        {
            curentactiondespawn = (() => DisablePreset(id));

            I.GetLayer(setup.presets[id].layerName).onEndAnimation.AddListener(curentactiondespawn);

            I.DisableLayer(setup.presets[id].layerName);
        }
        else
		{
            if (curentactiondespawn != null)
            {
                I.GetLayer(setup.presets[id].layerName).onEndAnimation.RemoveListener(curentactiondespawn);
                curentactiondespawn = null;
            }

            I.ShowLayer(setup.presets[id].layerName, -1);
            RunPreset(id);
		}
	}

    public void RunPreset(int id)
	{

        if (id >= 0 && id < setup.presets.Count)
        {
            Transform par = parent;
            if (setup.presets[id].useImageAsParrent)
                if (I.GetLayer(setup.presets[id].layerName) != null)
                    if (I.GetLayer(setup.presets[id].layerName).group != null)
                        par = I.GetLayer(setup.presets[id].layerName).group.transform;

            setup.presets[id].objsupd = new List<SLM_PC_Object>();

            for (int i=0;i< setup.presets[id].objs.Count;i++)
			{
                int idd = i;
                SLM_PC_PresetObject info = setup.presets[id].objs[idd];

                bool can = true;
                if (info.disabled)
                    can = false;
                if (!string.IsNullOrEmpty(info.checkBool))
                {
                    if (!C.ValueWorkBool(info.checkBool))
                        can = false;
                }
                if (!info.randomenable)
                {
                    int r = Random.Range(0, 100);
                    if (r > info.chanceToSpawn)
                    {
                        can = false;
                        if (!info.resetChanse)
                            info.disabled = true;
                    }
                    else if (!info.resetChanse)
                        info.randomenable = true;
                }

                if (can)
                {
                    SLM_PC_Object obj = Instantiate(item.gameObject, par).GetComponent<SLM_PC_Object>();
                    obj.gameObject.SetActive(true);
                    obj.gameObject.name = info.name;
                    obj.GetComponent<RectTransform>().sizeDelta = info.size;
                    obj.GetComponent<RectTransform>().anchoredPosition = info.pos;
                    obj.GetComponent<RectTransform>().rotation = Quaternion.AngleAxis(info.rot, Vector3.forward);

                    obj.GetComponent<Button>().interactable = info.interactible;
                    if (!info.interactible)
                        obj.GetComponent<Image>().raycastTarget = false;

                    obj.GetComponent<Button>().onClick.AddListener(() => PickObject(id, idd, obj));
                    obj.GetComponent<Image>().color = info.colorObject;

                    obj.count = info.count;
                    obj.HoldAsOne = info.pickAtOnce;
                    obj.pickable = info.pickable;
                    obj.ignoreAmount = info.ignoreAmount;
                    obj.chanceToSpawn = info.chanceToSpawn;
                    obj.checkBool = info.checkBool;
                    obj.pointWhenClick = info.pointwhenclicking;

                    obj.id = info.id;
                    if (obj.id >= 0 && obj.id < setup.objs.Count)
                    {
                        if (setup.objs[obj.id].spriteId >= 0 && setup.objs[obj.id].spriteId < sprites.Count)
                            obj.GetComponent<Image>().sprite = sprites[setup.objs[obj.id].spriteId];
                    }

                    obj.main = this;
                    setup.presets[id].objsupd.Add(obj);
                }
            }
        }
    }

    public void DisablePreset(int id) 
    {
        if (curentactiondespawn != null)
        {
            I.GetLayer(setup.presets[id].layerName).onEndAnimation.RemoveListener(curentactiondespawn);
            curentactiondespawn = null;
        }

        Transform par = parent;
        if (setup.presets[id].useImageAsParrent)
            if (I.GetLayer(setup.presets[id].layerName) != null)
                if (I.GetLayer(setup.presets[id].layerName).group != null)
                    par = I.GetLayer(setup.presets[id].layerName).group.transform;

        for (int i = par.childCount - 1; i >= 0; i--)
            Destroy(par.GetChild(i).gameObject);

        setup.presets[id].objsupd = new List<SLM_PC_Object>();
    }

    public void UpdateUI()
    {
        if (ui.visible)
        {
            if (ui.mode == SLM_PC_UI.mde.small || ui.mode == SLM_PC_UI.mde.smallAndBig)
            {
                    List<int> ids = new List<int>();

                    if (ui.smallMode == SLM_PC_UI.smallmde.list)
                    {
                        for (int i = 0; i < ui.listId.Count; i++)
                            if (inventoryLast.Contains(ui.listId[i]))
                                ids.Add(inventoryLast[inventoryLast.IndexOf(ui.listId[i])]);
                    }
                    else if (ui.smallMode == SLM_PC_UI.smallmde.last)
                    {
                        for (int i = inventoryLast.Count - 1; i >= 0; i--)
                            if (ids.Count < ui.maxcount + 1)
                                ids.Add(inventoryLast[i]);
                    }
                    else if (ui.smallMode == SLM_PC_UI.smallmde.first)
                    {
                        for (int i = 0; i < inventoryLast.Count; i++)
                            if (ids.Count < ui.maxcount + 1)
                                ids.Add(inventoryLast[i]);
                    }
                    else if (ui.smallMode==SLM_PC_UI.smallmde.asInSetUp)
				{
                    for (int i=0;i<inventory.Count;i++)
					{
                        if (inventory[i] > 0)
                            ids.Add(i);
					}
				}

                    for (int d = ui.smallParent.childCount - 1; d >= 0; d--)
                        Destroy(ui.smallParent.GetChild(d).gameObject);

                for (int i = 0; i < ids.Count; i++)
                {
                    int id = ids[i];
                    SLM_PC_UIBlock obj = Instantiate(ui.smallBlock.gameObject, ui.smallParent).GetComponent<SLM_PC_UIBlock>();
                    obj.gameObject.SetActive(true);

                    if (obj.icon != null)
                        obj.icon.sprite = sprites[setup.objs[id].spriteId];
                    if (obj.logo != null)
                    {
                        if (B != null)
                            obj.logo.text = B.GetStringEditor(setup.objs[id].alsl_name);
                        else
                            obj.logo.text = ALSL_Main.GetWorldAndFindKeys(setup.objs[id].alsl_name);
                    }
                    if (obj.count != null)
                        obj.count.text = inventory[id] + "";
                    if (obj.useObject != null)
                        obj.useObject.onClick.AddListener(() => UseObject(id));
                    if (obj.removeObject != null)
                        obj.removeObject.onClick.AddListener(() => RemoveObject(id));
                }
                if (ui.hideSmallIfEmpty && inventoryLast.Count > 0)
                {
                    ui.smallMain.SetActive(true);
                }
                else if (ui.hideSmallIfEmpty)
                {
                    ui.smallMain.SetActive(false);
                }
            }

            if (ui.mode == SLM_PC_UI.mde.big || ui.mode == SLM_PC_UI.mde.smallAndBig)
            {
                if (ui.closeBigButton != null && ui.openBigButton != null)
                {
                    ui.closeBigButton.onClick.AddListener(() => OpenCloseBigUI(true));
                    ui.openBigButton.onClick.AddListener(() => OpenCloseBigUI(false));
                    ui.closeBigButton.gameObject.SetActive(false);
                    ui.bigMain.SetActive(false);
                    
                    ui.openBigButton.gameObject.SetActive(true);
                    if (ui.hideSmallIfEmpty && inventoryLast.Count == 0)
                    {
                        ui.openBigButton.gameObject.SetActive(false);
                    }
                }
                for (int d = ui.bigParent.childCount - 1; d >= 0; d--)
                    Destroy(ui.bigParent.GetChild(d).gameObject);

                for (int i = 0; i < inventoryLast.Count; i++)
                {
                    int id = inventoryLast[i];
                    SLM_PC_UIBlock obj = Instantiate(ui.bigBlock.gameObject, ui.bigParent).GetComponent<SLM_PC_UIBlock>();
                    obj.gameObject.SetActive(true);

                    if (obj.icon != null)
                        obj.icon.sprite = sprites[setup.objs[id].spriteId];
                    if (obj.logo != null)
                    {
                        if (B != null)
                            obj.logo.text = B.GetString(setup.objs[id].alsl_name);
                        else
                            obj.logo.text = ALSL_Main.GetWorldAndFindKeys(setup.objs[id].alsl_name);
                    }
                    if (obj.count != null)
                        obj.count.text = inventory[id] + "";
                    if (obj.useObject != null)
                        obj.useObject.onClick.AddListener(() => UseObject(id));
                    if (obj.removeObject != null)
                        obj.removeObject.onClick.AddListener(() => RemoveObject(id));
                }
            }
        }
    }

    public void UseObject(int id)
	{
        if (inventoryLast.Contains(id))
		{
            inventory[id] = Mathf.Clamp(inventory[id] - setup.objs[id].removecountwhenuse, 0, setup.objs[id].maxAmount);
            if (inventory[id] == 0)
                inventoryLast.Remove(id);

            if (!string.IsNullOrEmpty(setup.objs[id].pointwhenuse))
                C.RunPoint(setup.objs[id].pointwhenuse);

            UpdateUI();
        }
	}

    public void RemoveObject(int id)
	{
        if (inventoryLast.Contains(id))
		{
            inventoryLast.Remove(id);
            inventory[id] = 0;

            UpdateUI();
        }
	}

    public void OpenCloseBigUI(bool close)
    {
        if (close)
        {
            ui.bigMain.SetActive(false);
            ui.closeBigButton.gameObject.SetActive(false);
            ui.openBigButton.gameObject.SetActive(true);
            if (ui.mode == SLM_PC_UI.mde.smallAndBig)
			{
                ui.smallMain.SetActive(true);
			}
        }
        else
        {
            ui.bigMain.SetActive(true);
            ui.closeBigButton.gameObject.SetActive(true);
            ui.openBigButton.gameObject.SetActive(false);
            if (ui.mode == SLM_PC_UI.mde.smallAndBig)
            {
                ui.smallMain.SetActive(false);
            }
        }
    }

    public void PickObject(int pre, int id, SLM_PC_Object obj)
	{
        if (obj != null)
        {
            if (obj.pickable)
            {
                if (inventory[obj.id] < setup.objs[obj.id].maxAmount && (inventoryLast.Count < maxInventoryItems || inventoryLast.Contains(obj.id)))
                {
                    if (!obj.HoldAsOne)
                    {
                        int save = inventory[obj.id];
                        inventory[obj.id] = Mathf.Clamp(inventory[obj.id] + 1, 0, setup.objs[obj.id].maxAmount);

                        if (inventory[obj.id] != save)
                        {
                            if (inventoryLast.Contains(obj.id))
                                inventoryLast.Move(inventoryLast.IndexOf(obj.id), inventoryLast.Count - 1);
                            else
                                inventoryLast.Add(obj.id);
                        }

                        if (obj.count != -1)
                        {
                            obj.count--;
                            setup.presets[pre].objs[id].count--;
                            if (obj.count <= 0)
                            {
                                setup.presets[pre].objs[id].disabled = true;
                                obj.gameObject.SetActive(false);
                            }
                        }
                    }
                    else
                    {
                        if (inventoryLast.Count < maxInventoryItems)
                        {
                            int save = inventory[obj.id];

                            if (obj.count != -1)
                                inventory[obj.id] = Mathf.Clamp(inventory[obj.id] + obj.count, 0, setup.objs[obj.id].maxAmount);

                            if (inventory[obj.id] != save)
                            {
                                if (inventoryLast.Contains(obj.id))
                                    inventoryLast.Move(inventoryLast.IndexOf(obj.id), inventoryLast.Count - 1);
                                else
                                    inventoryLast.Add(obj.id);
                            }
                        }

                        obj.count = 0;
                        setup.presets[pre].objs[id].count = 0;
                        setup.presets[pre].objs[id].disabled = true;
                        obj.gameObject.SetActive(false);
                    }


                }
                else if (obj.ignoreAmount)
                {
                    if (!obj.HoldAsOne)
                    {
                        if (obj.count != -1)
                        {
                            obj.count--;
                            setup.presets[pre].objs[id].count--;
                            if (obj.count <= 0)
                            {
                                setup.presets[pre].objs[id].disabled = true;
                                obj.gameObject.SetActive(false);
                            }
                        }
                    }
                    else
                    {
                        obj.count = 0;
                        setup.presets[pre].objs[id].count = 0;
                        setup.presets[pre].objs[id].disabled = true;
                        obj.gameObject.SetActive(false);
                    }
                }

                if (!string.IsNullOrEmpty(setup.objs[obj.id].pointwhenpick))
                    C.RunPoint(setup.objs[obj.id].pointwhenpick);
            }

            if (!string.IsNullOrEmpty(obj.pointWhenClick))
                C.RunPoint(obj.pointWhenClick);
        }

        UpdateUI();
    }
}

[System.Serializable]
public class SLM_PC_UI
{
    [HideInInspector] public bool visible=true;

    public enum mde { off, small, big, smallAndBig};
    public mde mode;

    public GameObject smallMain;
    public Transform smallParent;
    public SLM_PC_UIBlock smallBlock;
    public enum smallmde { first, last, list, asInSetUp };
    public smallmde smallMode;
    public List<int> listId;
    public int maxcount = 10;
    public bool hideSmallIfEmpty;

    public GameObject bigMain;
    public Transform bigParent;
    public SLM_PC_UIBlock bigBlock;
    public Button openBigButton;
    public Button closeBigButton;
}

[System.Serializable]
public class SLM_PC_Setup
{
    public List<SLM_PC_ObjectSet> objs;
    public List<SLM_PC_Preset> presets;
}

[System.Serializable]
public class SLM_PC_ObjectSet
{
    public string name; //name for editor
    public int spriteId = -1; //id in sprites
    public string alsl_name; //name in alsl or bridge
    public int maxAmount = 100; //maximum amount of object
    public string pointwhenpick; //point when object picked (run first before pointwhenclicking) 
    public string pointwhenuse; //point that run when item used in inventory
    public int removecountwhenuse = 1; //count that remove when use object
    public bool show;
}

[System.Serializable]
public class SLM_PC_Preset
{
    public string name;
    public bool useImageAsParrent;
    public string layerName; 
    public List<SLM_PC_PresetObject> objs;
    public List<SLM_PC_Object> objsupd; //objects that spawned 
}

[System.Serializable]
public class SLM_PC_PresetObject
{
    public string name; //name for editor 

    //RectTransform
    public Vector2 pos;
    public Vector2 size = new Vector2(100, 100);
    public float rot = 0;
    public Color colorObject = Color.white; //color of object

    public int id = 0; //id in objects list
    public bool pickAtOnce = false; //pick all count at once click (ignore -1)
    public int count = 1; //count of objects (-1 inf)
    public bool resetChanse =true; //recalculate chance every call of the preset
    public int chanceToSpawn = 100; //chance to spawn
    public bool interactible = true; //interactible in button
    public bool pickable = true; //can be picked
    public bool ignoreAmount = true; //ignore amount when run pick event
    public string checkBool; //bool to check spawn
    public string pointwhenclicking; //event on every click (also without pickable) (run second after pointwhenpick)

    public bool randomenable;
    public bool disabled; //when count = 0 or code
    public bool show;
}

[System.Serializable]
public class SLM_PC_Save
{
    public List<int> inventory;
    public List<int> inventoryLast;
    public List<SLM_PC_Presets> presets;
}

[System.Serializable]
public class SLM_PC_Presets
{
    public List<SLM_PC_SaveObjectData> objs;
}

[System.Serializable]
public class SLM_PC_SaveObjectData
{
    public bool disable;
    public bool randomenable;
    public int count;
}