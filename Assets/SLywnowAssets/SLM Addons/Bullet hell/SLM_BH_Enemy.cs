using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SLywnow;

public class SLM_BH_Enemy : MonoBehaviour
{

   [Header("Main")]
   public SLM_BH_Controll controll;
   public SLM_BH_View view;
   [Tooltip("Works only if RotateToMoveDir is true")]

   public int hp = 1;
   public int scoreByDead = 100;
   public List<GameObject> spawnWhenDead;

   [Header("AI")]
   public float waitBeforeStartAI = 0;
   public aitpe AIType;
   [ShowFromEnum("AIType", 0)]
   public SLM_BH_Enemy_EasyAI easyAI;
   public int RoundCount = 2;
   public int WaitingBeforePosUpdate = 1;
   [ShowFromEnum("AIType", 1)]
   public SLM_BH_Enemy_MiddleAi middleAI;
   [ShowFromEnum("AIType", 2)]
   public SLM_BH_Enemy_HardAI hardAI;
   [ShowFromEnum("AIType", 3)]
   public SLM_BH_Enemy_TriggersAI triggerAI;
   public enum aitpe { easy, middle, hard, trigger };

   public bool useExternalMove;
   [ShowFromBool(nameof(useExternalMove))]
   public SLM_BH_Enemy_MoveEvents moveEvents;

   public bool cantBeFreezed;
   public bool useFreezeColor = true;
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
   int posupdtimer;
   bool shoottimerstop;
   float deathTimer;
   float damageTimer = -1;
   Vector3 topos;
   float charZ;
   [HideInInspector]
   public int curpos;
   [HideInInspector]
   public int maxpos;
   public enum triggerStates { Default, nothing, see, close, attack };
   [HideInInspector]
   public int maxpat;
   [HideInInspector]
   public int curpat;
   [HideInInspector]
   public triggerStates triggerState;
   bool blockcheckpatt;
   [HideInInspector]
   public UbhShotCtrl curshoot;

   [HideInInspector]
   public Transform player;
   [HideInInspector]
   public Transform target;

   public void Start()
   {
      charZ = transform.position.z;
      prevpos = transform.position;

      if (waitBeforeStartAI > 0)
         shoottimer = waitBeforeStartAI;

      if (autodeath > 0)
         deathTimer = autodeath;
      else
         deathTimer = -1;

      bool lookAtThePlayer = false;
      string playerTag = "";

      if (AIType == aitpe.easy)
      {
         lookAtThePlayer = easyAI.lookAtThePlayer;
         playerTag = easyAI.tag;
         moveEvents.onTargetUpdated?.Invoke(transform.position + (Vector3)(easyAI.moveDir * 10000), easyAI.speed);
      }
      else if (AIType == aitpe.middle)
      { lookAtThePlayer = middleAI.lookAtThePlayer; playerTag = middleAI.tag; }
      else if (AIType == aitpe.hard && curpat != -1)
      { lookAtThePlayer = hardAI.patterns[curpat].lookAtThePlayer; playerTag = hardAI.patterns[curpat].tag; }

      if (lookAtThePlayer)
      {
         if (GameObject.FindGameObjectWithTag(playerTag) != null)
            player = GameObject.FindGameObjectWithTag(playerTag).transform;
      }

      if (useFreezeColor)
      {
         msr = GetComponent<SpriteRenderer>();
         defColor = msr.color;
      }

      maxtime = -1;
      freezetime = -1;
      maxhp = hp;
   }

   public void ChangeTriggerState(triggerStates state, bool exit)
   {
      switch (state)
      {
         case triggerStates.Default:
            {
               if (!exit)
               {
                  triggerState = triggerStates.Default;
                  DisableAllTriggerShoots();
                  GetHardAI(triggerAI.Default);
               }
               break;
            }
         case triggerStates.nothing:
            {
               if (!exit)
               {
                  triggerState = triggerStates.nothing;
                  DisableAllTriggerShoots();
                  GetHardAI(triggerAI.nothing);
               }
               break;
            }
         case triggerStates.see:
            {
               if (!exit)
               {
                  triggerState = triggerStates.see;
                  DisableAllTriggerShoots();
                  GetHardAI(triggerAI.seeTarget);
               }
               else
               {
                  if (triggerAI.useNothing)
                     ChangeTriggerState(triggerStates.nothing, false);
                  else
                     ChangeTriggerState(triggerStates.Default, false);
               }
               break;
            }
         case triggerStates.close:
            {
               if (!exit)
               {
                  triggerState = triggerStates.close;
                  DisableAllTriggerShoots();
                  GetHardAI(triggerAI.targetIsClose);
               }
               else
               {
                  if (triggerAI.useSeeTarget)
                     ChangeTriggerState(triggerStates.see, false);
                  else
                     ChangeTriggerState(triggerStates.see, true);
               }
               break;
            }
         case triggerStates.attack:
            {
               if (!exit)
               {
                  if (triggerState != triggerStates.attack)
                  {
                     triggerState = triggerStates.attack;
                     DisableAllTriggerShoots();
                     GetHardAI(triggerAI.isGetDamage);
                  }
                  damageTimer = triggerAI.damageTimer;
               }
               else
               {
                  damageTimer = -1;
                  ChangeTriggerState(triggerStates.see, true);
               }
               break;
            }
      }

      moveEvents.onTargetUpdated?.Invoke(topos, GetCurrentTrigger().patterns[curpat].speed);
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
            bool lookAtThePlayer = false;

            if (AIType == aitpe.easy)
               lookAtThePlayer = easyAI.lookAtThePlayer;
            else if (AIType == aitpe.middle)
               lookAtThePlayer = middleAI.lookAtThePlayer;
            else if (AIType == aitpe.hard && curpat != -1)
               lookAtThePlayer = hardAI.patterns[curpat].lookAtThePlayer;
            else if (AIType == aitpe.trigger && curpat != -1)
               lookAtThePlayer = GetCurrentTrigger().patterns[curpat].lookAtThePlayer;

            if (view.TypeOfMove == SLM_BH_View.tpe.sprites && view.sprites.RotateToMoveDir && lookAtThePlayer && player != null)
            {
               transform.LookAt2D(player);
               transform.Rotate(new Vector3(0, 0, view.sprites.rotationOffset));
            }

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
               if (!useExternalMove)
                  transform.position += topos * easyAI.speed * Time.deltaTime;
               else
               {
                  moveEvents.onMove.Invoke();
                  CheckExternalMove();
               }
            }
            else if (AIType == aitpe.middle)
            {
               if (System.Math.Round(Vector2.Distance(transform.position, topos), RoundCount) > 0)
               {
                  if (posupdtimer < WaitingBeforePosUpdate)
                     posupdtimer++;
                  else
                  {
                     if (middleAI.updateTargetEachFrame && target != null)
                     {
                        topos = target.position;
                        fixTopos();
                        moveEvents.onTargetUpdated?.Invoke(topos, middleAI.speed);
                     }
                     posupdtimer = 0;

                  }

                  if (middleAI.movePattern != SLM_BH_Enemy_MiddleAi.usetpe.DontMove)
                  {
                     if (!useExternalMove)
                        transform.position = Vector2.MoveTowards(transform.position, topos, middleAI.speed * Time.deltaTime);
                     else
                     {
                        moveEvents.onMove.Invoke();
                        CheckExternalMove();
                     }
                  }
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
                     target = null;
                     if (curpos < maxpos)
                     {
                        if (middleAI.movePattern == SLM_BH_Enemy_MiddleAi.usetpe.line)
                           topos = middleAI.line.GetPosition(curpos);
                        else if (middleAI.movePattern == SLM_BH_Enemy_MiddleAi.usetpe.transform)
                        {
                           topos = middleAI.pointsT[curpos].position;
                           target = middleAI.pointsT[curpos];
                        }
                        else if (middleAI.movePattern == SLM_BH_Enemy_MiddleAi.usetpe.vector)
                           topos = middleAI.pointsV[curpos];
                        else if (middleAI.movePattern == SLM_BH_Enemy_MiddleAi.usetpe.TagTarget)
                        {
                           GameObject o = GameObject.FindGameObjectWithTag(middleAI.tag);
                           if (o != null)
                           {
                              topos = o.transform.position;
                              target = o.transform;
                           }
                           else
                              topos = transform.position;
                        }
                        else if (middleAI.movePattern == SLM_BH_Enemy_MiddleAi.usetpe.DontMove)
                        {
                           topos = transform.position;
                        }

                        CheckDir();
                     }
                     else
                     {
                        if (middleAI.onEndPath == SLM_BH_Enemy_MiddleAi.endtpe.death)
                        {
                           Kill();
                        }
                        else if (middleAI.onEndPath == SLM_BH_Enemy_MiddleAi.endtpe.loop || middleAI.onEndPath == SLM_BH_Enemy_MiddleAi.endtpe.tpToStart)
                        {
                           curpos = 0;
                           if (middleAI.movePattern == SLM_BH_Enemy_MiddleAi.usetpe.line)
                              topos = middleAI.line.GetPosition(curpos);
                           else if (middleAI.movePattern == SLM_BH_Enemy_MiddleAi.usetpe.transform)
                           {
                              if (middleAI.pointsT[curpos] != null)
                              {
                                 topos = middleAI.pointsT[curpos].position;
                                 target = middleAI.pointsT[curpos];
                              }
                              else
                                 topos = transform.position;
                           }
                           else if (middleAI.movePattern == SLM_BH_Enemy_MiddleAi.usetpe.vector)
                              topos = middleAI.pointsV[curpos];
                           else if (middleAI.movePattern == SLM_BH_Enemy_MiddleAi.usetpe.TagTarget)
                           {
                              GameObject o = GameObject.FindGameObjectWithTag(middleAI.tag);
                              if (o != null)
                              {
                                 topos = o.transform.position;
                                 target = o.transform;
                              }
                              else
                                 topos = transform.position;
                           }
                           else if (middleAI.movePattern == SLM_BH_Enemy_MiddleAi.usetpe.DontMove)
                           {
                              topos = transform.position;
                           }


                           if (middleAI.onEndPath == SLM_BH_Enemy_MiddleAi.endtpe.tpToStart)
                              transform.position = topos;

                           CheckDir();
                        }
                        else if (middleAI.onEndPath == SLM_BH_Enemy_MiddleAi.endtpe.nothing)
                        {
                           topos = transform.position;
                           CheckDir();
                        }
                     }

                     fixTopos();
                     moveEvents.onTargetUpdated?.Invoke(topos, middleAI.speed);

                     if (middleAI.stayOnPoint > 0)
                        staytimer = middleAI.stayOnPoint;
                  }
               }
            }
            else if (AIType == aitpe.hard)
            {
               UpdateHardAI(hardAI);
            }
            else if (AIType == aitpe.trigger)
            {
               UpdateHardAI(GetCurrentTrigger());
               if (triggerState == triggerStates.attack && damageTimer != -1)
               {
                  if (damageTimer > 0) damageTimer -= Time.deltaTime;
                  else
                     ChangeTriggerState(triggerStates.attack, true);
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

   void UpdateHardAI(SLM_BH_Enemy_HardAI hAI)
   {
      if (System.Math.Round(Vector2.Distance(transform.position, topos), RoundCount) > 0)
      {
         if (posupdtimer < WaitingBeforePosUpdate)
            posupdtimer++;
         else
         {
            if (hAI.patterns[curpat].updateTargetEachFrame && target != null)
            {
               topos = target.position;
               fixTopos();
               moveEvents.onTargetUpdated?.Invoke(topos, hAI.patterns[curpat].speed);
            }
            posupdtimer = 0;

         }

         if (hAI.patterns[curpat].movePattern != SLM_BH_Enemy_MiddleAiAdvanced.usetpe.DontMove)
         {
            if (!useExternalMove)
               transform.position = Vector2.MoveTowards(transform.position, topos, hAI.patterns[curpat].speed * Time.deltaTime);
            else
            {
               moveEvents.onMove.Invoke();
               CheckExternalMove();
            }
         }
      }
      else
      {
         if (hAI.patterns[curpat].stayOnPoint > 0 && staytimer != -1)
         {
            if (staytimer > 0)
               staytimer -= Time.deltaTime;
            else
               staytimer = -1;
         }
         else
         {
            curpos++;
            target = null;
            if (curpos < maxpos)
            {
               if (hAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.line)
                  topos = hAI.patterns[curpat].line.GetPosition(curpos);
               else if (hAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.transform)
               {
                  if (hAI.patterns[curpat].pointsT[curpos] != null)
                  {
                     topos = hAI.patterns[curpat].pointsT[curpos].position;
                     target = hAI.patterns[curpat].pointsT[curpos];
                  }
                  else
                     topos = transform.position;
               }
               else if (hAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.vector)
                  topos = hAI.patterns[curpat].pointsV[curpos];
               else if (hAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.TagTarget)
               {
                  GameObject o = GameObject.FindGameObjectWithTag(hAI.patterns[curpat].tag);
                  if (o != null)
                  {
                     topos = o.transform.position;
                     target = o.transform;
                  }
                  else
                     topos = transform.position;
               }
               else if (hAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.DontMove)
               {
                  topos = transform.position;
               }


               CheckDir();
            }
            else
            {
               if (hAI.patterns[curpat].onEndPath == SLM_BH_Enemy_MiddleAiAdvanced.endtpe.death)
               {
                  Kill();
               }
               else if (hAI.patterns[curpat].onEndPath == SLM_BH_Enemy_MiddleAiAdvanced.endtpe.loop || hAI.patterns[curpat].onEndPath == SLM_BH_Enemy_MiddleAiAdvanced.endtpe.tpToStart)
               {
                  target = null;
                  curpos = 0;
                  if (hAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.line)
                     topos = hAI.patterns[curpat].line.GetPosition(curpos);
                  else if (hAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.transform)
                  {
                     topos = hAI.patterns[curpat].pointsT[curpos].position;
                     target = hAI.patterns[curpat].pointsT[curpos];
                  }
                  else if (hAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.vector)
                     topos = hAI.patterns[curpat].pointsV[curpos];
                  else if (hAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.TagTarget)
                  {
                     GameObject o = GameObject.FindGameObjectWithTag(hAI.patterns[curpat].tag);
                     if (o != null)
                     {
                        topos = o.transform.position;
                        target = o.transform;
                     }
                     else
                        topos = transform.position;
                  }
                  else if (hAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.DontMove)
                  {
                     topos = transform.position;
                  }

                  if (hAI.patterns[curpat].onEndPath == SLM_BH_Enemy_MiddleAiAdvanced.endtpe.tpToStart)
                     transform.position = topos;

                  CheckDir();
               }
               else if (hAI.patterns[curpat].onEndPath == SLM_BH_Enemy_MiddleAiAdvanced.endtpe.nextPattern)
               {
                  if (!blockcheckpatt)
                     ChangePat(hAI);
               }
               else if (hAI.patterns[curpat].onEndPath == SLM_BH_Enemy_MiddleAiAdvanced.endtpe.nothing)
               {
                  topos = transform.position;
                  CheckDir();
               }
            }

            fixTopos();

            if (hAI.patterns[curpat].stayOnPoint > 0)
               staytimer = hAI.patterns[curpat].stayOnPoint;
         }

         moveEvents.onTargetUpdated?.Invoke(topos, hAI.patterns[curpat].speed);
      }

      if (hAI.patterns[curpat].timerOfPattern > 0 && pattimer != -1 && !blockcheckpatt)
      {
         if (pattimer > 0)
            pattimer -= Time.deltaTime;
         else
            ChangePat(hAI);
      }
   }

   void DisableAllTriggerShoots()
   {
      {
         if (triggerAI.nothing != null && triggerAI.nothing.patterns != null)
            foreach (SLM_BH_Enemy_MiddleAiAdvanced p in triggerAI.nothing.patterns)
            {
               if (p.shootObject != null)
               {
                  p.shootObject.StopShotRoutine();
                  p.shootObject.gameObject.SetActive(false);
                  curshoot = null;
               }
            }

         if (triggerAI.seeTarget != null && triggerAI.seeTarget.patterns != null)
            foreach (SLM_BH_Enemy_MiddleAiAdvanced p in triggerAI.seeTarget.patterns)
            {
               if (p.shootObject != null)
               {
                  p.shootObject.StopShotRoutine();
                  p.shootObject.gameObject.SetActive(false);
                  curshoot = null;
               }
            }

         if (triggerAI.targetIsClose != null && triggerAI.targetIsClose.patterns != null)
            foreach (SLM_BH_Enemy_MiddleAiAdvanced p in triggerAI.targetIsClose.patterns)
            {
               if (p.shootObject != null)
               {
                  p.shootObject.StopShotRoutine();
                  p.shootObject.gameObject.SetActive(false);
                  curshoot = null;
               }
            }

         if (triggerAI.isGetDamage != null && triggerAI.isGetDamage.patterns != null)
            foreach (SLM_BH_Enemy_MiddleAiAdvanced p in triggerAI.isGetDamage.patterns)
            {
               if (p.shootObject != null)
               {
                  p.shootObject.StopShotRoutine();
                  p.shootObject.gameObject.SetActive(false);
                  curshoot = null;
               }
            }
      }
   }

   public void AIGet()
   {
      blockcheckpatt = false;
      if (easyAI.shootObject != null)
      {
         easyAI.shootObject.gameObject.SetActive(false);
         easyAI.shootObject.StopShotRoutine();
      }
      if (middleAI.shootObject != null)
      {
         middleAI.shootObject.gameObject.SetActive(false);
         middleAI.shootObject.StopShotRoutine();
      }
      foreach (SLM_BH_Enemy_MiddleAiAdvanced p in hardAI.patterns)
      {
         if (p.shootObject != null)
         {
            p.shootObject.StopShotRoutine();
            p.shootObject.gameObject.SetActive(false);
            curshoot = null;
         }
      }
      DisableAllTriggerShoots();

      if (AIType == aitpe.easy)
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

         target = null;

         if (middleAI.movePattern == SLM_BH_Enemy_MiddleAi.usetpe.line)
         {
            topos = middleAI.line.GetPosition(0);
            maxpos = middleAI.line.positionCount;
         }
         else if (middleAI.movePattern == SLM_BH_Enemy_MiddleAi.usetpe.transform)
         {
            if (middleAI.pointsT[0] != null)
            {
               topos = middleAI.pointsT[0].position;
            }
            else
               topos = transform.position;

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
            {
               topos = o.transform.position;
               target = o.transform;
            }
            else
            {
               topos = transform.position;
            }
            maxpos = 1;
         }

         if (middleAI.onEndPath == SLM_BH_Enemy_MiddleAi.endtpe.tpToStart)
            transform.position = topos;

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

         fixTopos();
         moveEvents.onTargetUpdated?.Invoke(topos, middleAI.speed);
      }
      if (AIType == aitpe.hard)
      {
         GetHardAI(hardAI);

         fixTopos();
         moveEvents.onTargetUpdated?.Invoke(topos, hardAI.patterns[curpat].speed);
      }
      else if (AIType == aitpe.trigger)
      {
         if (triggerAI.useNothing)
         {
            triggerState = triggerStates.nothing;
            GetHardAI(triggerAI.nothing);
         }
         else
         {
            triggerState = triggerStates.Default;
            GetHardAI(triggerAI.Default);
         }

         if (triggerAI.useSeeTarget)
         {
            triggerAI.seeTargetTrigger.targetTags = new List<string>();
            triggerAI.seeTargetTrigger.targetTags.Add(triggerAI.targetTag);

            triggerAI.seeTargetTrigger.onTriggerEnter.RemoveAllListeners();
            triggerAI.seeTargetTrigger.onTriggerExit.RemoveAllListeners();

            triggerAI.seeTargetTrigger.onTriggerEnter.AddListener((Collider2D c) => ChangeTriggerState(triggerStates.see, false));
            triggerAI.seeTargetTrigger.onTriggerExit.AddListener((Collider2D c) => ChangeTriggerState(triggerStates.see, true));
         }
         if (triggerAI.useTargetIsClose)
         {
            triggerAI.targetIsCloseTrigger.targetTags = new List<string>();
            triggerAI.targetIsCloseTrigger.targetTags.Add(triggerAI.targetTag);

            triggerAI.targetIsCloseTrigger.onTriggerEnter.RemoveAllListeners();
            triggerAI.targetIsCloseTrigger.onTriggerExit.RemoveAllListeners();

            triggerAI.targetIsCloseTrigger.onTriggerEnter.AddListener((Collider2D c) => ChangeTriggerState(triggerStates.close, false));
            triggerAI.targetIsCloseTrigger.onTriggerExit.AddListener((Collider2D c) => ChangeTriggerState(triggerStates.close, true));
         }
      }
   }

   void GetHardAI(SLM_BH_Enemy_HardAI hAI)
   {
      curpos = 0;
      curpat = 0;
      maxpat = hAI.patterns.Count;
      blockcheckpatt = false;
      target = null;

      foreach (SLM_BH_Enemy_MiddleAiAdvanced p in hAI.patterns)
      {
         if (p.shootObject != null)
         {
            p.shootObject.StopAllCoroutines();
            p.shootObject.gameObject.SetActive(false);
            curshoot = null;
         }
      }

      if (hAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.line)
      {
         topos = hAI.patterns[curpat].line.GetPosition(0);
         maxpos = hAI.patterns[curpat].line.positionCount;
      }
      else if (hAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.transform)
      {
         if (hAI.patterns[curpat].pointsT[0] != null)
         {
            topos = hAI.patterns[curpat].pointsT[0].position;
            target = hAI.patterns[curpat].pointsT[0];
         }
         else
            topos = transform.position;

         maxpos = hAI.patterns[curpat].pointsT.Count;
      }
      else if (hAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.vector)
      {
         topos = hAI.patterns[curpat].pointsV[0];
         maxpos = hAI.patterns[curpat].pointsV.Count;
      }
      else if (hAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.TagTarget)
      {
         GameObject o = GameObject.FindGameObjectWithTag(hAI.patterns[curpat].tag);
         if (o != null)
         {
            topos = o.transform.position;
            target = o.transform;
         }
         else
            topos = transform.position;
         maxpos = 1;
      }
      else if (hAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.DontMove)
      {
         topos = transform.position;
      }

      if (hAI.patterns[curpat].onEndPath == SLM_BH_Enemy_MiddleAiAdvanced.endtpe.tpToStart)
         transform.position = topos;

      if (hAI.patterns[curpat].stayOnPoint > 0)
         staytimer = hAI.patterns[curpat].stayOnPoint;
      else
         staytimer = -1;

      if (hAI.patterns[curpat].shootObject != null)
      {
         hAI.patterns[curpat].shootObject.gameObject.SetActive(true);
         hAI.patterns[curpat].shootObject.StartShotRoutine();
         curshoot = hAI.patterns[curpat].shootObject;
      }
      else
         curshoot = null;

      if (hAI.patterns[curpat].timerOfPattern > 0)
      {
         pattimer = hAI.patterns[curpat].timerOfPattern;
         maxtime = pattimer;
      }
      else
      {
         pattimer = -1;
         maxtime = -1;
      }

      moveEvents.onTargetUpdated?.Invoke(topos, hAI.patterns[curpat].speed);
      CheckDir();
   }

   void fixTopos()
   {
      topos.z = charZ;
   }

   public void ChangePat(SLM_BH_Enemy_HardAI hAI)
   {
      if (!blockcheckpatt)
      {
         UbhShotCtrl lastshoot = curshoot;
         if (curpat != -1)
         {
            if (hAI.patterns[curpat].shootObject != null)
            {
               hAI.patterns[curpat].shootObject.gameObject.SetActive(false);
               hAI.patterns[curpat].shootObject.StopAllCoroutines();
               curshoot = null;
            }
            hAI.patterns[curpat].onPatternEnd.Invoke();
         }
         curpos = 0;
         curpat++;
         target = null;


         if (curpat < maxpat)
         {
            if (hAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.line)
            {
               topos = hAI.patterns[curpat].line.GetPosition(0);
               maxpos = hAI.patterns[curpat].line.positionCount;
            }
            else if (hAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.transform)
            {
               if (hAI.patterns[curpat].pointsT[0] != null)
               {
                  topos = hAI.patterns[curpat].pointsT[0].position;
                  target = hAI.patterns[curpat].pointsT[0];
               }
               else
                  topos = transform.position;

               maxpos = hAI.patterns[curpat].pointsT.Count;
            }
            else if (hAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.vector)
            {
               topos = hAI.patterns[curpat].pointsV[0];
               maxpos = hAI.patterns[curpat].pointsV.Count;
            }
            else if (hAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.TagTarget)
            {
               GameObject o = GameObject.FindGameObjectWithTag(hAI.patterns[curpat].tag);
               if (o != null)
               {
                  topos = o.transform.position;
                  target = o.transform;
               }
               else
                  topos = transform.position;
               maxpos = 1;
            }
            else if (hAI.patterns[curpat].movePattern == SLM_BH_Enemy_MiddleAiAdvanced.usetpe.DontMove)
            {
               topos = transform.position;
            }

            if (hAI.patterns[curpat].stayOnPoint > 0)
               staytimer = hAI.patterns[curpat].stayOnPoint;
            else
               staytimer = -1;

            if (hAI.patterns[curpat].shootObject != null)
            {
               hAI.patterns[curpat].shootObject.gameObject.SetActive(true);
               hAI.patterns[curpat].shootObject.StartShotRoutine();
               curshoot = hAI.patterns[curpat].shootObject;
            }
            else
               curshoot = null;

            if (hAI.patterns[curpat].timerOfPattern > 0)
            {
               pattimer = hAI.patterns[curpat].timerOfPattern;
               maxtime = hAI.patterns[curpat].timerOfPattern;
            }
            else
            {
               pattimer = -1;
               maxtime = -1;
            }

            hAI.patterns[curpat].onPatternStart.Invoke();
         }
         else
         {
            if (hAI.onEndPatternts == SLM_BH_Enemy_HardAI.endtpe.continueLast)
            {
               blockcheckpatt = true;
               if (lastshoot != null)
               {
                  lastshoot.gameObject.SetActive(true);
                  lastshoot.StartShotRoutine();
                  curshoot = lastshoot;
               }
               curpat--;
            }
            else if (hAI.onEndPatternts == SLM_BH_Enemy_HardAI.endtpe.loop)
            {
               curpat = -1;
               ChangePat(hAI);
            }
            else if (hAI.onEndPatternts == SLM_BH_Enemy_HardAI.endtpe.death)
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

   SLM_BH_Enemy_HardAI GetCurrentTrigger()
   {
      switch (triggerState)
      {
         case triggerStates.Default:
            return triggerAI.Default;
         case triggerStates.nothing:
            return triggerAI.nothing;
         case triggerStates.see:
            return triggerAI.seeTarget;
         case triggerStates.close:
            return triggerAI.targetIsClose;
         case triggerStates.attack:
            return triggerAI.isGetDamage;
         default:
            return triggerAI.Default;
      }
   }

   public void CheckDir()
   {
      bool lookAtThePlayer = false;

      if (AIType == aitpe.easy)
      {
         lookAtThePlayer = easyAI.lookAtThePlayer;
         if (lookAtThePlayer)
            player = GameObject.FindGameObjectWithTag(easyAI.tag).transform;
      }
      else if (AIType == aitpe.middle)
      {
         lookAtThePlayer = middleAI.lookAtThePlayer;
         if (lookAtThePlayer)
            player = GameObject.FindGameObjectWithTag(middleAI.tag).transform;
      }
      else if (AIType == aitpe.hard)
      {
         lookAtThePlayer = hardAI.patterns[curpat].lookAtThePlayer;
         if (lookAtThePlayer)
            player = GameObject.FindGameObjectWithTag(hardAI.patterns[curpat].tag).transform;
      }
      else if (AIType == aitpe.trigger)
      {
         lookAtThePlayer = GetCurrentTrigger().patterns[curpat].lookAtThePlayer;
         if (lookAtThePlayer)
            player = GameObject.FindGameObjectWithTag(GetCurrentTrigger().patterns[curpat].tag).transform;
      }



      if (view.TypeOfMove == SLM_BH_View.tpe.sprites && !lookAtThePlayer)
      {
         if (view.sprites.RotateToMoveDir)
         {
            if (topos == transform.position)
            {
               view.StopMove();
               transform.rotation = Quaternion.identity;
            }
            else
            {
               transform.LookAt2D(topos);
               transform.Rotate(new Vector3(0, 0, view.sprites.rotationOffset));
            }
         }
         else
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
            else if (AIType == aitpe.hard || AIType == aitpe.middle || AIType == aitpe.trigger)
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
      }
   }

   Vector2 prevpos;
   public void CheckExternalMove()
   {
      bool lookAtThePlayer = false;

      if (AIType == aitpe.easy)
      {
         lookAtThePlayer = easyAI.lookAtThePlayer;
      }
      else if (AIType == aitpe.middle)
      {
         lookAtThePlayer = middleAI.lookAtThePlayer;
      }
      else if (AIType == aitpe.hard)
      {
         lookAtThePlayer = hardAI.patterns[curpat].lookAtThePlayer;
      }
      else if (AIType == aitpe.trigger)
      {
         lookAtThePlayer = GetCurrentTrigger().patterns[curpat].lookAtThePlayer;
      }

      if (view.TypeOfMove == SLM_BH_View.tpe.sprites && !lookAtThePlayer)
      {
         if (view.sprites.RotateToMoveDir)
         {
            transform.LookAt2D(prevpos);
            transform.Rotate(new Vector3(0, 0, view.sprites.rotationOffset + 180));
         }
         else
         {
            if (transform.position.x < prevpos.x)
               view.Move("l");
            else if (transform.position.x > prevpos.x)
               view.Move("r");
            else if (transform.position.y < prevpos.y)
               view.Move("d");
            else if (transform.position.y > prevpos.y)
               view.Move("u");
            else if (prevpos == (Vector2)transform.position)
               view.StopMove();
         }
      }


      prevpos = transform.position;


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
            if (!string.IsNullOrEmpty(Enemyname))
               controll.lastenemydamage = Enemyname;

            hp -= 1;
         }

         if (hp > 0)
         {
            OnDamage();
			}
         else
         {
            Dead();
			}
      }
   }

   void OnDamage()
   {
		eventOnDamage.Invoke();
		imutineTimer = immortalityAfterDamage;
		view.Damage();
		if (AIType == aitpe.trigger && triggerAI.useIsGetDamage)
		{
			ChangeTriggerState(triggerStates.attack, false);
		}
	}

   void Dead()
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

   public void ForceDamage(int damage)
   {
      hp -= damage;

		if (hp > 0)
		{
			OnDamage();
		}
		else
		{
			Dead();
		}
	}
}

[System.Serializable]
public class SLM_BH_Enemy_EasyAI
{
    public float speed = 4f;
    public Vector2 moveDir=Vector2.down;
    public UbhShotCtrl shootObject;
    public bool lookAtThePlayer;
    [ShowFromBool("lookAtThePlayer")]
    public string tag;
}

[System.Serializable]
public class SLM_BH_Enemy_MiddleAi
{
    public UbhShotCtrl shootObject;
    public enum endtpe { nothing, loop, death, tpToStart };
    public endtpe onEndPath;
    public float speed = 4f;
    public float stayOnPoint = 0f;
    public usetpe movePattern;
    public enum usetpe { line, transform, vector, TagTarget, DontMove };
    [ShowFromEnum("movePattern", 0)]
    public LineRenderer line;
    [ShowFromEnum("movePattern", 1)]
    public List<Transform> pointsT;
    [ShowFromEnum("movePattern", 2)]
    public List<Vector2> pointsV;
	[ShowFromMultiple(nameof(movePattern), new string[] { "1", "3" }, "enum", ShowFromMultipleAttribute.mode.or)]
	public bool updateTargetEachFrame;
	public bool lookAtThePlayer;
    [ShowFromMultiple(new string[2] { "movePattern", "lookAtThePlayer" }, new string[2] { "3", "true" }, new string[2] { "enum", "bool" }, ShowFromMultipleAttribute.mode.or)]
    public string tag;
}

[System.Serializable]
public class SLM_BH_Enemy_MiddleAiAdvanced
{
    public UbhShotCtrl shootObject;
    public float timerOfPattern = 0f;
    public endtpe onEndPath;
    public enum endtpe { nothing, loop, nextPattern, death, tpToStart };
    public float speed = 4f;
    public float stayOnPoint = 0f;
    public usetpe movePattern;
    public enum usetpe { line, transform, vector, TagTarget, DontMove };
    [ShowFromEnum("movePattern", 0)]
    public LineRenderer line;
    [ShowFromEnum("movePattern", 1)]
    public List<Transform> pointsT;
    [ShowFromEnum("movePattern", 2)]
    public List<Vector2> pointsV;
    [ShowFromMultiple(new string[2] { nameof(movePattern), nameof(lookAtThePlayer) }, new string[2] { "3", "true" }, new string[2] { "enum", "bool" }, ShowFromMultipleAttribute.mode.or)]
    public string tag;
    [ShowFromMultiple(nameof(movePattern), new string[] { "1", "3" }, "enum", ShowFromMultipleAttribute.mode.or)]
	public bool updateTargetEachFrame;
    public bool lookAtThePlayer;
    public UnityEvent onPatternStart;
    public UnityEvent onPatternEnd;
}

[System.Serializable]
public class SLM_BH_Enemy_HardAI
{ 
    public enum endtpe { continueLast, loop, death };
    public endtpe onEndPatternts;
    public List<SLM_BH_Enemy_MiddleAiAdvanced> patterns;
}

[System.Serializable]
public class SLM_BH_Enemy_TriggersAI
{
    public string targetTag;
    public bool useNothing;
    public bool useSeeTarget;
    public bool useTargetIsClose;
    public bool useIsGetDamage;

    public SLM_BH_Enemy_HardAI Default;
    [ShowFromBool("useNothing")]
    public SLM_BH_Enemy_HardAI nothing;
    [ShowFromBool("useSeeTarget")]
    public SLM_BH_EntityTrigger seeTargetTrigger;
    [ShowFromBool("useSeeTarget")]
    public SLM_BH_Enemy_HardAI seeTarget;
    [ShowFromBool("useTargetIsClose")]
    public SLM_BH_EntityTrigger targetIsCloseTrigger;
    [ShowFromBool("useTargetIsClose")]
    public SLM_BH_Enemy_HardAI targetIsClose;
    [ShowFromBool("useIsGetDamage")]
    public float damageTimer;
    [ShowFromBool("useIsGetDamage")]
    public SLM_BH_Enemy_HardAI isGetDamage;
}

[System.Serializable]
public class SLM_BH_Enemy_MoveEvents
{
    [Tooltip("Calling in Update when enemy must Move")]
    public UnityEvent onMove;
    public UnityEvent<Vector3, float> onTargetUpdated;
}