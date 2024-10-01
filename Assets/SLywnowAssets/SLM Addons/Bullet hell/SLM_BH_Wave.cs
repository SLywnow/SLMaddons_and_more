using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SLywnow;
using UnityEngine.Events;

public class SLM_BH_Wave : MonoBehaviour
{
    public SLM_BH_Controll controll;
    public int autoRunWave = -1;

    public List<SLM_BH_Wave_WavePreset> presets;
    
    List<SLM_BH_Wave_Wave> waveSettings;
    [HideInInspector] 
    public List<SLM_BH_Enemy> enemyslist;
    int curid = -1;
    [HideInInspector] 
    public int curwave = -1;
    [HideInInspector] 
    public int maxwave = 0;
    bool waverun = false;
    int waveencount = 0;
    int waveenrun = 0;
    float sttimer=-1;
    int stid;
    [HideInInspector] 
    public float maxtimeWave;
    [HideInInspector] 
    public float time;
    [HideInInspector] 
    public float maxenemy;

    void Start()
    {
        enemyslist = new List<SLM_BH_Enemy>();
        if (autoRunWave >= 0)
        {
            RunWavePreset(autoRunWave);
        }
    }


    void Update()
    {
        if (curid != -1 && curwave != -1 && waverun)
        {
            if (waveSettings != null && waveSettings[curwave].whenWaveEnds == SLM_BH_Wave_Wave.waveend.timer || waveSettings[curwave].whenWaveEnds == SLM_BH_Wave_Wave.waveend.any)
            {
                if (time > 0)
                    time -= Time.deltaTime;
                else
                    RunNextWave();
            }
            if (waveSettings != null && waveSettings[curwave].whenWaveEnds == SLM_BH_Wave_Wave.waveend.allDied || waveSettings[curwave].whenWaveEnds == SLM_BH_Wave_Wave.waveend.any)
            {
                if (enemyslist.Count == 0 && waverun)
                    RunNextWave();
            }
        }

        if (sttimer!=-1)
		{
            if (sttimer > 0)
                sttimer -= Time.deltaTime;
            else
			{
                sttimer = -1;
                if (stid >= 0 && stid < presets.Count)
                {
                    curid = stid;
                    waveSettings = presets[stid].waveSettings;
                    RunFirstWave();
                }
            }
		}
    }

    public void RunWavePreset(int id)
    {
        maxwave = presets[id].waveSettings.Count;
        if (presets[id].startDelay > 0)
        {
            sttimer = presets[id].startDelay;
            stid = id;
        }
        else
        {
            sttimer = -1;
            if (id >= 0 && id < presets.Count)
            {
                curid = id;
                waveSettings = presets[id].waveSettings;
                RunFirstWave();
            }
        }
    }

    public void RunWave(int id)
    {
        if (curid != -1 && waveSettings != null)
        {
            waverun = false;
            waveenrun = 0;
            maxenemy = 0;
            waveSettings[id].onStart.Invoke();
            StopAllCoroutines();
            
            if (waveSettings[id].whenWaveEnds == SLM_BH_Wave_Wave.waveend.timer || waveSettings[id].whenWaveEnds == SLM_BH_Wave_Wave.waveend.any)
            {
                time = waveSettings[id].time;
                maxtimeWave = waveSettings[id].time;
            }
            else
			{
                maxtimeWave = 0;
            }

            waveencount = waveSettings[id].enemies.Count;

            for (int e = 0; e < waveSettings[id].enemies.Count; e++)
            {
                if (waveSettings[id].enemies[e].SpawnType == SLM_BH_Wave_EnemyLine.spawntype.onePoint)
                {
                    StartCoroutine(GenerateEnemyOnePoint(curid, waveSettings[id].enemies[e].enemy, waveSettings[id].enemies[e].timeDelay, waveSettings[id].enemies[e].point, waveSettings[id].enemies[e].count, waveSettings[id].enemies[e].timeOffset, waveSettings[id].enemies[e].autosetUi, waveSettings[id].enemies[e].forceEasyAPI, waveSettings[id].enemies[e].EasyAPIDirection));
                    maxenemy+= waveSettings[id].enemies[e].count;
                }
                else if (waveSettings[id].enemies[e].SpawnType == SLM_BH_Wave_EnemyLine.spawntype.lineX)
                {
                    StartCoroutine(GenerateEnemyLine(curid, waveSettings[id].enemies[e].enemy, waveSettings[id].enemies[e].point, waveSettings[id].enemies[e].timeDelay, waveSettings[id].enemies[e].count, waveSettings[id].enemies[e].positionOffset, waveSettings[id].enemies[e].timeOffset, true, waveSettings[id].enemies[e].autosetUi, waveSettings[id].enemies[e].forceEasyAPI, waveSettings[id].enemies[e].EasyAPIDirection));
                    maxenemy += waveSettings[id].enemies[e].count;
                }
                else if (waveSettings[id].enemies[e].SpawnType == SLM_BH_Wave_EnemyLine.spawntype.lineY)
                {
                    StartCoroutine(GenerateEnemyLine(curid, waveSettings[id].enemies[e].enemy, waveSettings[id].enemies[e].point, waveSettings[id].enemies[e].timeDelay, waveSettings[id].enemies[e].count, waveSettings[id].enemies[e].positionOffset, waveSettings[id].enemies[e].timeOffset, false, waveSettings[id].enemies[e].autosetUi, waveSettings[id].enemies[e].forceEasyAPI, waveSettings[id].enemies[e].EasyAPIDirection));
                    maxenemy += waveSettings[id].enemies[e].count;
                }
                else if (waveSettings[id].enemies[e].SpawnType == SLM_BH_Wave_EnemyLine.spawntype.lineObject)
                {
                    StartCoroutine(GenerateEnemyPos(curid, waveSettings[id].enemies[e].enemy, waveSettings[id].enemies[e].timeDelay, waveSettings[id].enemies[e].timeOffset, waveSettings[id].enemies[e].line, null, null, waveSettings[id].enemies[e].autosetUi, waveSettings[id].enemies[e].forceEasyAPI, waveSettings[id].enemies[e].EasyAPIDirection));
                    maxenemy += waveSettings[id].enemies[e].line.positionCount;
                }
                else if (waveSettings[id].enemies[e].SpawnType == SLM_BH_Wave_EnemyLine.spawntype.transform)
                {
                    StartCoroutine(GenerateEnemyPos(curid, waveSettings[id].enemies[e].enemy, waveSettings[id].enemies[e].timeDelay, waveSettings[id].enemies[e].timeOffset, null, waveSettings[id].enemies[e].positionT, null, waveSettings[id].enemies[e].autosetUi, waveSettings[id].enemies[e].forceEasyAPI, waveSettings[id].enemies[e].EasyAPIDirection));
                    maxenemy += waveSettings[id].enemies[e].positionT.Count;
                }
                else if (waveSettings[id].enemies[e].SpawnType == SLM_BH_Wave_EnemyLine.spawntype.vectors)
                {
                    StartCoroutine(GenerateEnemyPos(curid, waveSettings[id].enemies[e].enemy, waveSettings[id].enemies[e].timeDelay, waveSettings[id].enemies[e].timeOffset, null, null, waveSettings[id].enemies[e].positionV, waveSettings[id].enemies[e].autosetUi, waveSettings[id].enemies[e].forceEasyAPI, waveSettings[id].enemies[e].EasyAPIDirection));
                    maxenemy += waveSettings[id].enemies[e].positionV.Count;
                }
            }

            if (waveSettings[id].enemies.Count == 0)
                waverun = true;
        }
    }

    IEnumerator GenerateEnemyOnePoint(int saveid, SLM_BH_Enemy enemy, float timeDelay, Vector2 point, int count, float timeOffset, List<SLM_BH_UI> ui, bool forceEAPI, Vector2 EAPIdir)
    {
        float timeD = 0;

        while (timeD < timeDelay)
        {
            timeD += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < count; i++)
        {
            if (saveid == curid)
            {
                SLM_BH_Enemy spawn = Instantiate(enemy.gameObject, point, Quaternion.identity).GetComponent<SLM_BH_Enemy>();

                spawn.gameObject.SetActive(true);
                spawn.gameObject.name = enemy.gameObject.name;
                spawn.controll = controll;
                enemyslist.Add(spawn);
                spawn.destroyondeath = true;
                spawn.eventOnDeathByCode.AddListener(() => MeDead(spawn));
                spawn.eventOnDeathByPlayer.AddListener(() => MeDead(spawn));
                if (forceEAPI)
                    spawn.easyAI.moveDir = EAPIdir;
                if (ui != null && ui.Count == count)
                    if (ui[i] != null)
                        ui[i].enemy = spawn;


                float timeO = 0;
                while (timeO < timeOffset)
                {
                    timeO += Time.deltaTime;
                    yield return null;
                }
            }
            else
                yield break;
        }

        waveenrun++;
        if (waveenrun >= waveencount)
            waverun = true;

        yield break;
    }

    IEnumerator GenerateEnemyLine(int saveid, SLM_BH_Enemy enemy, Vector2 point, float timeDelay, int count, float positionOffset, float timeOffset, bool X, List<SLM_BH_UI> ui, bool forceEAPI, Vector2 EAPIdir)
    {
        float timeD = 0;

        while (timeD < timeDelay)
        {
            timeD += Time.deltaTime;
            yield return null;
        }

        Vector2 v = point;
        for (int i = 0; i < count; i++)
        {
            if (saveid == curid)
            {
                SLM_BH_Enemy spawn = Instantiate(enemy.gameObject, v, Quaternion.identity).GetComponent<SLM_BH_Enemy>();

                spawn.gameObject.SetActive(true);
                spawn.gameObject.name = enemy.gameObject.name;
                spawn.destroyondeath = true;
                spawn.controll = controll;
                enemyslist.Add(spawn);
                spawn.eventOnDeathByCode.AddListener(() => MeDead(spawn));
                spawn.eventOnDeathByPlayer.AddListener(() => MeDead(spawn));
                if (forceEAPI)
                    spawn.easyAI.moveDir = EAPIdir;
                if (ui != null && ui.Count == count)
                    if (ui[i] != null)
                        ui[i].enemy = spawn;

                if (X)
                    v.x += positionOffset;
                else
                    v.y += positionOffset;

                float timeO = 0;
                while (timeO < timeOffset)
                {
                    timeO += Time.deltaTime;
                    yield return null;
                }
            }
            else
                yield break;
        }

        waveenrun++;
        if (waveenrun >= waveencount)
            waverun = true;

        yield break;
    }

    IEnumerator GenerateEnemyPos(int saveid, SLM_BH_Enemy enemy, float timeDelay, float timeOffset, LineRenderer line, List<Transform> pointT, List<Vector2> pointV, List<SLM_BH_UI> ui, bool forceEAPI, Vector2 EAPIdir)
    {
        float timeD = 0;

        while (timeD < timeDelay)
        {
            timeD += Time.deltaTime;
            yield return null;
        }

        if (line != null)
        {
            int max = line.positionCount;
            for (int i = 0; i < max; i++)
            {
                if (saveid == curid)
                {
                    SLM_BH_Enemy spawn = Instantiate(enemy.gameObject, line.GetPosition(i), Quaternion.identity).GetComponent<SLM_BH_Enemy>();

                    spawn.gameObject.SetActive(true);
                    spawn.gameObject.name = enemy.gameObject.name;
                    spawn.destroyondeath = true;
                    spawn.controll = controll;
                    enemyslist.Add(spawn);
                    spawn.eventOnDeathByCode.AddListener(() => MeDead(spawn));
                    spawn.eventOnDeathByPlayer.AddListener(() => MeDead(spawn));
                    if (forceEAPI)
                        spawn.easyAI.moveDir = EAPIdir;
                    if (ui != null && ui.Count == max)
                        if (ui[i] != null)
                            ui[i].enemy = spawn;

                    float timeO = 0;
                    while (timeO < timeOffset)
                    {
                        timeO += Time.deltaTime;
                        yield return null;
                    }
                }
                else
                    yield break;
            }
        }
        else if (pointT != null)
        {
            for (int i = 0; i < pointT.Count; i++)
            {
                if (saveid == curid)
                {
                    SLM_BH_Enemy spawn = Instantiate(enemy.gameObject, pointT[i].position, Quaternion.identity).GetComponent<SLM_BH_Enemy>();

                    spawn.gameObject.SetActive(true);
                    spawn.gameObject.name = enemy.gameObject.name;
                    spawn.destroyondeath = true;
                    spawn.controll = controll;
                    enemyslist.Add(spawn);
                    spawn.eventOnDeathByCode.AddListener(() => MeDead(spawn));
                    spawn.eventOnDeathByPlayer.AddListener(() => MeDead(spawn));
                    if (forceEAPI)
                        spawn.easyAI.moveDir = EAPIdir;
                    if (ui != null && ui.Count == pointT.Count)
                        if (ui[i] != null)
                            ui[i].enemy = spawn;

                    float timeO = 0;
                    while (timeO < timeOffset)
                    {
                        timeO += Time.deltaTime;
                        yield return null;
                    }
                }
                else
                    yield break;
            }
        }
        else if (pointV != null)
        {
            for (int i = 0; i < pointV.Count; i++)
            {
                if (saveid == curid)
                {
                    SLM_BH_Enemy spawn = Instantiate(enemy.gameObject, pointV[i], Quaternion.identity).GetComponent<SLM_BH_Enemy>();

                    spawn.gameObject.SetActive(true);
                    spawn.gameObject.name = enemy.gameObject.name;
                    spawn.destroyondeath = true;
                    spawn.controll = controll;
                    enemyslist.Add(spawn);
                    spawn.eventOnDeathByCode.AddListener(() => MeDead(spawn));
                    spawn.eventOnDeathByPlayer.AddListener(() => MeDead(spawn));
                    if (forceEAPI)
                        spawn.easyAI.moveDir = EAPIdir;
                    if (ui != null && ui.Count == pointV.Count)
                        if (ui[i] != null)
                            ui[i].enemy = spawn;

                    float timeO = 0;
                    while (timeO < timeOffset)
                    {
                        timeO += Time.deltaTime;
                        yield return null;
                    }
                }
                else
                    yield break;
            }
        }

        waveenrun++;
        if (waveenrun >= waveencount)
            waverun = true;

        yield break;
    }


    public void RunFirstWave()
    {
        if (curid != -1 && waveSettings != null)
        {
            curwave = 0;
            if (presets[curid].randomlist)
                waveSettings.Shuffle();
            RunWave(curwave);
        }

    }

    public void RunNextWave()
    {
        if (curwave != -1 && curid != -1)
        {
            for (int i = enemyslist.Count - 1; i >= 0; i--)
            {
                if (enemyslist[i] != null)
                {
                    enemyslist[i].Kill();
                }
            }

            waveSettings[curwave].onEnd.Invoke();
            enemyslist = new List<SLM_BH_Enemy>();

            bool blocknextwave = false;
            if (waveSettings[curwave].afterWaveEnds == SLM_BH_Wave_Wave.wavenext.next)
            {
                curwave++;
            }
            else if (waveSettings[curwave].afterWaveEnds == SLM_BH_Wave_Wave.wavenext.runwavenum)
            {
                curwave = waveSettings[curwave].runwavenum;
            }
            else if (waveSettings[curwave].afterWaveEnds == SLM_BH_Wave_Wave.wavenext.stop)
            {
                blocknextwave = true;
                waverun = false;
            }

            if (!blocknextwave)
            {
                if (curwave < waveSettings.Count)
                {
                    RunWave(curwave);
                }
                else
                {
                    if (presets[curid].onEnd == SLM_BH_Wave_WavePreset.onend.endBH)
                    {
                        waverun = false;
                        controll.EndBH(presets[curid].endBHAsWin);
                    }
                    else if (presets[curid].onEnd == SLM_BH_Wave_WavePreset.onend.loop)
                    {
                        RunFirstWave();
                    }
                    else if (presets[curid].onEnd == SLM_BH_Wave_WavePreset.onend.nothing)
                    {
                        curwave = -1;
                        waverun = false;
                    }
                }
            }
        }
    }

    public void StopAllWaves()
    {
        waverun = false;
        if (curid != -1)
        {
            if (curwave != -1)
            {

                for (int i = enemyslist.Count - 1; i >= 0; i--)
                {
                    if (enemyslist[i] != null)
                    {
                        enemyslist[i].Kill();
                    }
                }

                enemyslist = new List<SLM_BH_Enemy>();

                waveSettings = null;


                curwave = -1;
            }
            curid = -1;
        }

        if (GameObject.Find("UbhObjectPool") != null)
        {
            Transform pool = GameObject.Find("UbhObjectPool").transform;
            for (int i = pool.childCount - 1; i >= 0; i--)
                Destroy(pool.GetChild(i).gameObject);
        }
    }

    public void MeDead(SLM_BH_Enemy obj)
    {
        if (enemyslist.Contains(obj))
        {
            enemyslist.Remove(obj);
        }
    }
}


[System.Serializable]
public class SLM_BH_Wave_WavePreset
{
    public enum onend { endBH, nothing, loop};
    public onend onEnd;
    public float startDelay = 0;
    public bool randomlist;
    public bool endBHAsWin;
    public List<SLM_BH_Wave_Wave> waveSettings;
}

[System.Serializable]
public class SLM_BH_Wave_Wave
{
    public enum waveend {allDied, timer, any, never};
    public waveend whenWaveEnds;
    public enum wavenext { next, stop, runwavenum};
    public wavenext afterWaveEnds;
    public int runwavenum;
    public float time;
    public List<SLM_BH_Wave_EnemyLine> enemies;

    public UnityEvent onStart;
    public UnityEvent onEnd;
}

[System.Serializable]
public class SLM_BH_Wave_EnemyLine
{
    public SLM_BH_Enemy enemy;
    public float timeDelay=0;
    public enum spawntype {onePoint, lineX, lineY, lineObject, transform, vectors };
    public int count;
    public float timeOffset=0;
    public spawntype SpawnType;
    public Vector2 point;
    public float positionOffset = 0;
    public LineRenderer line;
    public List<Transform> positionT;
    public List<Vector2> positionV;

    public bool forceEasyAPI;
    public Vector2 EasyAPIDirection;

    [Tooltip ("Must be same as count")] public List<SLM_BH_UI> autosetUi;
}