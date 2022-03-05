using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SLM_BH_Enemy : MonoBehaviour
{

    [Header("Main")]
    public SLM_BH_Controll controll;
    public SLM_BH_View view;
    public int hp=1;
    public int scoreByDead=100;
    public List<GameObject> spawnWhenDead;

    [Header("AI")]
    public float waitBeforeStartAI=0;
    public aitpe AIType;
    public SLM_BH_Enemy_EasyAI easyAI;
    public int RoundCount = 2;
    public SLM_BH_Enemy_MiddleAi middleAI;
    public SLM_BH_Enemy_HardAI hardAI;
    public enum aitpe { easy, middle, hard};
    public bool cantBeFreezed;
    public bool useFreezeColor=true;
    public Color freezeColor = Color.blue;

    [Header("Damage")]
    public string bulletTag;
    public float immortalityAfterDamage = 0f;
    public bool destroyondeath;
    public float autodeath = -1;
    public UnityEvent eventOnDamage;
    public UnityEvent eventOnDeathByPlayer;
    public UnityEvent eventOnDeathByCode;

    bool freeze;
    Color defColor;
    SpriteRenderer msr;

    [HideInInspector] public int maxhp;
    [HideInInspector] public float maxtime;
    [HideInInspector] public float freezetime = 0;

    [HideInInspector] public float pattimer;
    float staytimer;
    float shoottimer;
    float imutineTimer;
    bool shoottimerstop;
    float deathTimer;
    Vector3 topos;
    public int curpos;
    int maxpos;
    [HideInInspector] public int maxpat;
    [HideInInspector] public int curpat;
    bool blockcheckpatt;
    public UbhShotCtrl curshoot;

    public void Start()
	{
        if (waitBeforeStartAI > 0)
            shoottimer = waitBeforeStartAI;

        if (autodeath > 0)
            deathTimer = autodeath;
        else
            deathTimer = -1;

        if (useFreezeColor)
        {
            msr = GetComponent<SpriteRenderer>();
            defColor = msr.color;
        }

        maxtime = -1;
        freezetime = -1;
        maxhp = hp;
    }

	public void Kill()
	{
        view.Die();
        eventOnDeathByCode.Invoke();
        if (destroyondeath)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }

    public void Update()
    {
            if (imutineTimer > 0)
                imutineTimer -= Time.deltaTime;
            else
                imutineTimer = 0;

        if (freezetime == -1)
        {
            if (!shoottimerstop)
            {
                if (shoottimer > 0)
                    shoottimer -= Time.deltaTime;
                else
                {
                    shoottimer = 0;
                    AIGet();
                    shoottimerstop = true;
                }
            }
            else
            {
                if (deathTimer != -1)
                {
                    if (deathTimer > 0)
                        deathTimer -= Time.deltaTime;
                    else
                    {
                        Kill();
                    }
                }
                if (AIType == aitpe.easy)
                {
                    transform.position += topos * easyAI.speed * Time.deltaTime;
                }
                else if (AIType == aitpe.middle)
                {
                    if (System.Math.Round(Vector2.Distance(transform.position, topos), RoundCount) > 0)
                    {
                        transform.position = Vector2.MoveTowards(transform.position, topos, middleAI.speed * Time.deltaTime);
                    }
                    else
                    {
                        if (middleAI.stayOnPoint > 0 && staytimer != -1)
                        {
                            if (staytimer > 0)
                                staytimer -= Time.deltaTime;
                            else
                                staytimer = -1;
                        }
                        else
                        {
                            curpos++;
                            if (curpos < maxpos)
                            {
                                if (middleAI.movePattern == SLM_BH_Enemy_MiddleAi.usetpe.line)
                                    topos = middleAI.line.GetPosition(curpos);
                                else if (middleAI.movePattern == SLM_BH_Enemy_MiddleAi.usetpe.transform)
                                    topos = middleAI.pointsT[curpos].position;
                                else if (middleAI.movePattern == SLM_BH_Enemy_MiddleAi.usetpe.vector)
                                    topos = middleAI.pointsV[curpos];
                                else if (middleAI.movePattern == SLM_BH_Enemy_MiddleAi.usetpe.TagTarget)
                                {
                                    GameObject o = GameObject.FindGameObjectWithTag(middleAI.tag);
                                    if (o != null)
                                        topos = o.transform.position;
                                    else
                                        topos = new Vector3(0, 0);
                                }

                                CheckDir();
                            }
                            else
                            {
                                if (middleAI.onEndPath == SLM_BH_Enemy_MiddleAi.endtpe.death)
                                {
                                    Kill();
                                }
                                else if (middleAI.onEndPath == SLM_BH_Enemy_MiddleAi.endtpe.loop)
                                {
                                    curpos = 0;
                                    if (middleAI.movePattern == SLM_BH_Enemy_MiddleAi.usetpe.line)
                                        topos = middleAI.line.GetPosition(curpos);
                                    else if (middleAI.movePattern == SLM_BH_Enemy_MiddleAi.usetpe.transform)
                                        topos = middleAI.pointsT[curpos].position;
                                    else if (middleAI.movePattern == SLM_BH_Enemy_MiddleAi.usetpe.vector)
                                        topos = middleAI.pointsV[curpos];
                                    else if (middleAI.movePattern == SLM_BH_Enemy_MiddleAi.usetpe.TagTarget)
                                    {
                                        GameObject o = GameObject.FindGameObjectWithTag(middleAI.tag);
                                        if (o != null)
                                            topos = o.transform.position;
                                        else
                                            topos = new Vector3(0, 0);
                                    }

                                    CheckDir();
                                }
                                else if (middleAI.onEndPath == SLM_BH_Enemy_MiddleAi.endtpe.nothing)
                                {
                                    topos = transform.position;
                                    CheckDir();
                                }
                            }

                            if (middleAI.stayOnPoint > 0)
                                staytimer = middleAI.stayOnPoint;
                        }
                    }
                }
                else if (AIType == aitpe.hard)
                {
                    if (System.Math.Round(Vector2.Distance(transform.position, topos), RoundCount) > 0)
                    {
                        transform.position = Vector2.MoveTowards(transform.position, topos, hardAI.patterns[curpat].speed * Time.deltaTime);
                    }
                    else
                    {
                        if (hardAI.patterns[curpat].stayOnPoint > 0 && staytimer != -1)
                        {
                            if (staytimer > 0)
                                staytimer -= Time.deltaTime;
                            else
                                staytimer = -1;
                        }
                        else if (!blockcheckpatt)
                        {

                            curpos++;
                            if (curpos < maxpos)
                            {
                                if (hardAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.line)
                                    topos = hardAI.patterns[curpat].line.GetPosition(curpos);
                                else if (hardAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.transform)
                                    topos = hardAI.patterns[curpat].pointsT[curpos].position;
                                else if (hardAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.vector)
                                    topos = hardAI.patterns[curpat].pointsV[curpos];
                                else if (hardAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.TagTarget)
                                {
                                    GameObject o = GameObject.FindGameObjectWithTag(hardAI.patterns[curpat].tag);
                                    if (o != null)
                                        topos = o.transform.position;
                                    else
                                        topos = new Vector3(0, 0);
                                }
                                else if (hardAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.DontMove)
                                {
                                    topos = transform.position;
                                }


                                    CheckDir();
                            }
                            else
                            {
                                if (hardAI.patterns[curpat].onEndPath == SLM_BH_Enemy_MiddleAiAdvanced.endtpe.death)
                                {
                                    Kill();
                                }
                                else if (hardAI.patterns[curpat].onEndPath == SLM_BH_Enemy_MiddleAiAdvanced.endtpe.loop)
                                {
                                    curpos = 0;
                                    if (hardAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.line)
                                        topos = hardAI.patterns[curpat].line.GetPosition(curpos);
                                    else if (hardAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.transform)
                                        topos = hardAI.patterns[curpat].pointsT[curpos].position;
                                    else if (hardAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.vector)
                                        topos = hardAI.patterns[curpat].pointsV[curpos];
                                    else if (hardAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.TagTarget)
                                    {
                                        GameObject o = GameObject.FindGameObjectWithTag(hardAI.patterns[curpat].tag);
                                        if (o != null)
                                            topos = o.transform.position;
                                        else
                                            topos = new Vector3(0, 0);
                                    }
                                    else if (hardAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.DontMove)
                                    {
                                        topos = transform.position;
                                    }

                                    CheckDir();
                                }
                                else if (hardAI.patterns[curpat].onEndPath == SLM_BH_Enemy_MiddleAiAdvanced.endtpe.nextPattern)
                                {
                                    ChangePat();
                                }
                                else if (hardAI.patterns[curpat].onEndPath == SLM_BH_Enemy_MiddleAiAdvanced.endtpe.nothing)
                                {
                                    topos = transform.position;
                                    CheckDir();
                                }
                            }


                            if (hardAI.patterns[curpat].stayOnPoint > 0)
                                staytimer = hardAI.patterns[curpat].stayOnPoint;
                        }
                    }

                    if (hardAI.patterns[curpat].timerOfPattern > 0 && pattimer != -1 && !blockcheckpatt)
                    {
                        if (pattimer > 0)
                            pattimer -= Time.deltaTime;
                        else
                            ChangePat();
                    }
                }
            }
        }
        else
        {
            if (freezetime >= 0)
            {
                if (!cantBeFreezed)
                {
                    if (!freeze)
                    {
                        if (curshoot != null)
                        {
                            curshoot.StopAllCoroutines();
                            curshoot.gameObject.SetActive(false);
                        }
                        freeze = true;
                    }
                    if (useFreezeColor)
                        msr.color = freezeColor;
                    freezetime -= Time.deltaTime;
                }
                else
                    freezetime = -1;
            }
            else
            {
                if (curshoot != null)
                {
                    curshoot.StartShotRoutine();
                    curshoot.gameObject.SetActive(true);
                }
                if (useFreezeColor)
                    msr.color = defColor;
                freeze = false;
                freezetime = -1;
            }
        }
    }

    public void AIGet()
	{
        if (AIType==aitpe.easy)
		{
            topos = easyAI.moveDir;
            if (easyAI.shootObject != null)
            {
                easyAI.shootObject.gameObject.SetActive(true);
                easyAI.shootObject.StartShotRoutine();
                curshoot = easyAI.shootObject;
            }
            else
                curshoot = null;
            CheckDir();
        }
        if (AIType == aitpe.middle)
        {
            curpos = 0;
            if (middleAI.movePattern == SLM_BH_Enemy_MiddleAi.usetpe.line)
            {
                topos = middleAI.line.GetPosition(0);
                maxpos = middleAI.line.positionCount;
            }
            else if (middleAI.movePattern == SLM_BH_Enemy_MiddleAi.usetpe.transform)
            {
                topos = middleAI.pointsT[0].position;
                maxpos = middleAI.pointsT.Count;
            }
            else if (middleAI.movePattern == SLM_BH_Enemy_MiddleAi.usetpe.vector)
            {
                topos = middleAI.pointsV[0];
                maxpos = middleAI.pointsV.Count;
            }
            else if (middleAI.movePattern == SLM_BH_Enemy_MiddleAi.usetpe.TagTarget)
			{
                GameObject o = GameObject.FindGameObjectWithTag(middleAI.tag);
                if (o != null)
                    topos = o.transform.position;
                else
                    topos = new Vector3(0, 0);
                maxpos = 1;
            }

            if (middleAI.stayOnPoint > 0)
                staytimer = middleAI.stayOnPoint;
            else
                staytimer = -1;

            if (middleAI.shootObject != null)
            {
                middleAI.shootObject.gameObject.SetActive(true);
                middleAI.shootObject.StartShotRoutine();
                curshoot = middleAI.shootObject;
            }
            else
                curshoot = null;

            CheckDir();
        }
        if (AIType == aitpe.hard)
        {
            curpos = 0;
            curpat = 0;
            maxpat = hardAI.patterns.Count;

            foreach (SLM_BH_Enemy_MiddleAiAdvanced p in hardAI.patterns)
            {
                if (p.shootObject != null)
                {
                    p.shootObject.StopAllCoroutines();
                    p.shootObject.gameObject.SetActive(false);
                    curshoot = null;
                }
            }

            if (hardAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.line)
            {
                topos = hardAI.patterns[curpat].line.GetPosition(0);
                maxpos = hardAI.patterns[curpat].line.positionCount;
            }
            else if (hardAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.transform)
            {
                topos = hardAI.patterns[curpat].pointsT[0].position;
                maxpos = hardAI.patterns[curpat].pointsT.Count;
            }
            else if (hardAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.vector)
            {
                topos = hardAI.patterns[curpat].pointsV[0];
                maxpos = hardAI.patterns[curpat].pointsV.Count;
            }
            else if (hardAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.TagTarget)
            {
                GameObject o = GameObject.FindGameObjectWithTag(hardAI.patterns[curpat].tag);
                if (o != null)
                    topos = o.transform.position;
                else
                    topos = new Vector3(0, 0);
                maxpos = 1;
            }
            else if (hardAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.DontMove)
            {
                topos = transform.position;
            }

            if (hardAI.patterns[curpat].stayOnPoint > 0)
                staytimer = hardAI.patterns[curpat].stayOnPoint;
            else
                staytimer = -1;

            if (hardAI.patterns[curpat].shootObject != null)
            {
                hardAI.patterns[curpat].shootObject.gameObject.SetActive(true);
                hardAI.patterns[curpat].shootObject.StartShotRoutine();
                curshoot = hardAI.patterns[curpat].shootObject;
            }
            else
                curshoot = null;

            if (hardAI.patterns[curpat].timerOfPattern > 0)
            {
                pattimer = hardAI.patterns[curpat].timerOfPattern;
                maxtime = pattimer;
            }
            else
            {
                pattimer = -1;
                maxtime = -1;
            }

            CheckDir();
        }
    }

    public void ChangePat()
	{
        if (!blockcheckpatt)
        {
            if (curpat != -1)
            {
                if (hardAI.patterns[curpat].shootObject != null)
                {
                    hardAI.patterns[curpat].shootObject.gameObject.SetActive(false);
                    hardAI.patterns[curpat].shootObject.StopAllCoroutines();
                    curshoot = null;
                }
            }
            curpos = 0;
            curpat++;

            if (curpat < maxpat)
            {
                if (hardAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.line)
                {
                    topos = hardAI.patterns[curpat].line.GetPosition(0);
                    maxpos = hardAI.patterns[curpat].line.positionCount;
                }
                else if (hardAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.transform)
                {
                    topos = hardAI.patterns[curpat].pointsT[0].position;
                    maxpos = hardAI.patterns[curpat].pointsT.Count;
                }
                else if (hardAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.vector)
                {
                    topos = hardAI.patterns[curpat].pointsV[0];
                    maxpos = hardAI.patterns[curpat].pointsV.Count;
                }
                else if (hardAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.TagTarget)
                {
                    GameObject o = GameObject.FindGameObjectWithTag(hardAI.patterns[curpat].tag);
                    if (o != null)
                        topos = o.transform.position;
                    else
                        topos = new Vector3(0, 0);
                    maxpos = 1;
                }
                else if (hardAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.DontMove)
                {
                    topos = transform.position;
                }

                if (hardAI.patterns[curpat].stayOnPoint > 0)
                    staytimer = hardAI.patterns[curpat].stayOnPoint;
                else
                    staytimer = -1;

                if (hardAI.patterns[curpat].shootObject != null)
                {
                    hardAI.patterns[curpat].shootObject.gameObject.SetActive(true);
                    hardAI.patterns[curpat].shootObject.StartShotRoutine();
                    curshoot = hardAI.patterns[curpat].shootObject;
                }
                else
                    curshoot = null;

                if (hardAI.patterns[curpat].timerOfPattern > 0)
                {
                    pattimer = hardAI.patterns[curpat].timerOfPattern;
                    maxtime = hardAI.patterns[curpat].timerOfPattern;
                }
                else
                {
                    pattimer = -1;
                    maxtime = -1;
                }

            }
            else
            {
                if (hardAI.onEndPatternts == SLM_BH_Enemy_HardAI.endtpe.continueLast)
                {
                    blockcheckpatt = true;
                    CheckDir();
                    curpat--;
                    if (hardAI.patterns[curpat].shootObject != null)
                    {
                        hardAI.patterns[curpat].shootObject.gameObject.SetActive(true);
                        hardAI.patterns[curpat].shootObject.StartShotRoutine();
                        curshoot = hardAI.patterns[curpat].shootObject;
                    }
                    else
                        curshoot = null;
                }
                else if (hardAI.onEndPatternts == SLM_BH_Enemy_HardAI.endtpe.loop)
                {
                    curpat = -1;
                    ChangePat();
                }
                else if (hardAI.onEndPatternts == SLM_BH_Enemy_HardAI.endtpe.death)
                {
                    eventOnDeathByCode.Invoke();
                    if (destroyondeath)
                        Destroy(gameObject);
                    else
                        gameObject.SetActive(false);
                }
            }

            CheckDir();
        }
    }

	public void CheckDir()
	{
        if (AIType == aitpe.easy)
        {
            if (topos.x < 0)
                view.Move("l");
            else if (topos.x > 0)
                view.Move("r");
            else if (topos.y < 0)
                view.Move("d");
            else if (topos.y > 0)
                view.Move("u");
            else
                view.StopMove();
        }
        else if (AIType == aitpe.hard || AIType == aitpe.middle)
        {
            if (transform.position.x > topos.x)
                view.Move("l");
            else if (transform.position.x < topos.x)
                view.Move("r");
            else if (transform.position.y > topos.y)
                view.Move("d");
            else if (transform.position.y < topos.y)
                view.Move("u");
            else if (topos == transform.position)
                view.StopMove();
        }
    }

    private void OnTriggerEnter2D(Collider2D c)
    {
        if (c.tag == bulletTag)
        {
            UbhBullet bullet = c.gameObject.GetComponentInParent<UbhBullet>();
            if (bullet.isActive)
            {
                UbhObjectPool.instance.ReleaseBullet(bullet);
                Damage(c.name);
            }
        }
    }

    private void Damage(string Enemyname)
    {
        if (imutineTimer <= 0)
        {
            if (controll.damageName.Contains(Enemyname))
            {
                controll.damagesCount[controll.damageName.IndexOf(Enemyname)]++;
                controll.lastenemydamage = Enemyname;
                if (controll.damages[controll.damageName.IndexOf(Enemyname)] > 0)
                {
                    hp -= controll.damages[controll.damageName.IndexOf(Enemyname)];
                }
                else
                    return;
            }
            else
            {
                hp -= 1;
            }

            if (hp > 0)
            {
                eventOnDamage.Invoke();
                imutineTimer = immortalityAfterDamage;
                view.Damage();
            }
            else
            {
                view.Die();
                eventOnDeathByPlayer.Invoke();
                controll.score += scoreByDead;

                foreach (GameObject i in spawnWhenDead)
                    Instantiate(i, transform.position, Quaternion.identity);

                if (destroyondeath)
                    Destroy(gameObject);
                else
                    gameObject.SetActive(false);
            }
        }
    }
}

[System.Serializable]
public class SLM_BH_Enemy_EasyAI
{
    public float speed = 4f;
    public Vector2 moveDir=Vector2.down;
    public UbhShotCtrl shootObject;
}

[System.Serializable]
public class SLM_BH_Enemy_MiddleAi
{
    public UbhShotCtrl shootObject;
    public enum endtpe { nothing, loop, death};
    public endtpe onEndPath;
    public float speed = 4f;
    public float stayOnPoint = 0f;
    public usetpe movePattern;
    public enum usetpe { line, transform, vector, TagTarget };
    public LineRenderer line;
    public List<Transform> pointsT;
    public List<Vector2> pointsV;
    public string tag;
}

[System.Serializable]
public class SLM_BH_Enemy_MiddleAiAdvanced
{
    public UbhShotCtrl shootObject;
    public float timerOfPattern = 0f;
    public endtpe onEndPath;
    public enum endtpe { nothing, loop, nextPattern, death };
    public float speed = 4f;
    public float stayOnPoint = 0f;
    public usetpe movePattern;
    public enum usetpe { line, transform, vector, TagTarget, DontMove };
    public LineRenderer line;
    public List<Transform> pointsT;
    public List<Vector2> pointsV;
    public string tag;
}

[System.Serializable]
public class SLM_BH_Enemy_HardAI
{ 
    public enum endtpe { continueLast, loop, death };
    public endtpe onEndPatternts;
    public List<SLM_BH_Enemy_MiddleAiAdvanced> patterns;
}