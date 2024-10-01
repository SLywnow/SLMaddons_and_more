using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SLywnow;
using UnityEngine.Events;

public class SLM_BH_Player : UbhMonoBehaviour
{
   [Header("Main")]
   public SLM_BH_Controll controll;
   public bool mobileInput;
   [ShowFromBool(nameof(mobileInput))]
   public bool disableMobileInputOnPC;
   [ShowFromBool(nameof(mobileInput))]
   public bool mobileCustom;
   public string Axis_x = "Horizontal";
   public string Axis_y = "Vertical";
   public bool useshift = true;
   public string shift = "Fire3";
   public string shoot = "Fire1";
   public GameObject shiftObject;
   public bool useUltimate = true;
   public string Ultimate = "Fire2";
   public SLM_BH_View view;
   public UnityEvent onDamage;

   [Header("Move")]
   public float speed = 10f;
   public float speedShift = 5f;
   public enum MoveMode { transform, rigitbody };
   public MoveMode moveMode;
   [ShowFromEnum("moveMode", 1)]
   public Rigidbody2D rigidbody2d;
   public Transform PlayArea;
   [ShowFromObjectNotNull("PlayArea", true)]
   public Vector2 Max;
   [ShowFromObjectNotNull("PlayArea", true)]
   public Vector2 Min;
   public bool useFreezeColor = true;
   public Color freezeColor = Color.blue;

   [Header("Shoot")]
   public bool autocontrolShoots = true;
   public bool autoShoot;
   public updtpe ShootUpdate;
   public List<SLM_BH_Player_Shoot> shoots;
   public UnityEvent<int> onChangeShoot;
   public string NextWeapon;
   public string PrevWeapon;
   public bool loopweapons;
   public float waitbeforestart = 0;
   public UbhShotCtrl ultimateShoot;
   public UnityEvent ultimateEvent;
   public bool useUltimateForEatchShoot;
   public List<UbhShotCtrl> ultimatesShoot;

   public float UltimateDelay = 0f;

   [Header("Enemy")]
   public string bulletTag;
   public string enemyTag;
   public float immortalityAfterDamage = 1f;
   public SLM_BH_EntityTrigger damageTrigger;
   Vector2 moveV;
   Vector2 rotV;


   bool freeze;
   Color defColor;
   SpriteRenderer msr;

   public enum updtpe { score, byVoid, time, byButton };

   [HideInInspector]
   public int curshoot;
	[HideInInspector]
	public bool shooting; //for external control
	[HideInInspector]
   public float freezetime = -1;
   [HideInInspector]
   public float imutineTimer;
   [HideInInspector]
   public bool nowShift;
   int nextupd;
   int prevupd;
   bool blocknextupd;
   bool blockprevupd;
   float timer;
   float shoottimer;
   bool shoottimerstop;
   float ulttimer;
   UbhShotCtrl cursh;
   Color dcolor;
   Vector2 curMoveV;

   private void Awake()
   {
      msr = GetComponent<SpriteRenderer>();
      if (msr != null)
         dcolor = msr.color;
      else
         useFreezeColor = false;
   }

   void Start()
   {
      if (msr != null)
         msr.color = dcolor;
      freezetime = -1;
      if (useFreezeColor)
      {
         defColor = msr.color;
      }

      if (controll == null)
         controll = FindObjectOfType<SLM_BH_Controll>();
      if (shiftObject != null)
         shiftObject.SetActive(false);
      if (PlayArea != null)
      {
         Vector2 curMin;
         Vector2 curMax;

         curMin.x = (-(PlayArea.localScale.x / 2)) + PlayArea.position.x;
         curMin.y = (-(PlayArea.localScale.y / 2)) + PlayArea.position.y;
         curMax.x = (PlayArea.localScale.x / 2) + PlayArea.position.x;
         curMax.y = (PlayArea.localScale.y / 2) + PlayArea.position.y;

         Min.x = curMin.x <= curMax.x ? curMin.x : curMax.x;
         Min.y = curMin.y <= curMax.y ? curMin.y : curMax.y;
         Max.x = curMin.x > curMax.x ? curMin.x : curMax.x;
         Max.y = curMin.y > curMax.y ? curMin.y : curMax.y;
      }
      if (Min.x == 0 && Max.x == 0 && Min.y == 0 && Max.y == 0)
      {
         Min.x = float.NegativeInfinity;
         Min.y = float.NegativeInfinity;
         Max.x = float.PositiveInfinity;
         Max.y = float.PositiveInfinity;
      }

      if (autocontrolShoots && waitbeforestart > 0)
      {
         shoottimer = waitbeforestart;
         shoottimerstop = false;
      }
      else if (autocontrolShoots)
         GetShoot();

      if (ultimateShoot != null)
         ultimateShoot.gameObject.SetActive(true);

      if (useUltimateForEatchShoot)
         foreach (UbhShotCtrl s in ultimatesShoot)
            s.gameObject.SetActive(true);

      if (damageTrigger != null)
      {
         damageTrigger.onTriggerEnter.RemoveAllListeners();
         damageTrigger.onTriggerEnter.AddListener((Collider2D c) => OnTriggerEnterCheck(c));
         damageTrigger.targetTags = new List<string>();
         if (!string.IsNullOrEmpty(bulletTag))
            damageTrigger.targetTags.Add(bulletTag);
         if (!string.IsNullOrEmpty(enemyTag))
            damageTrigger.targetTags.Add(enemyTag);
      }

      if (mobileInput && disableMobileInputOnPC)
      {
         bool ok = false;

#if UNITY_STANDALONE
         ok = true;
#endif

         if (ok)
         {
            mobileInput = false;
				mobileCustom = false;
			}
         
      }
   }

   public void Restart()
   {
      Start();
   }

   public Vector3 startMpos;
   public Vector2 statIpos;

   void Update()
   {
      if (freezetime == -1)
      {
         if (!mobileInput)
         {
            if (Input.GetButton(Axis_x) || Input.GetButton(Axis_y))
            {
               moveV.x = Input.GetAxisRaw(Axis_x);
               moveV.y = Input.GetAxisRaw(Axis_y);
               moveV = moveV.normalized;

               curMoveV = moveV;

               Move(moveV);
            }
         }
         else
         {
            if (!mobileCustom && moveMode == MoveMode.transform)
            {
               if (Input.GetMouseButtonDown(0))
               {
                  startMpos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                  statIpos = transform.position;
               }
               if (Input.GetMouseButton(0))
               {
                  Vector2 pos = statIpos - ((Vector2)(startMpos - Camera.main.ScreenToWorldPoint(Input.mousePosition)));

                  pos.x = Mathf.Clamp(pos.x, Min.x, Max.x);
                  pos.y = Mathf.Clamp(pos.y, Min.y, Max.y);

                  transform.position = pos;
               }
            }
         }

         if (Input.GetButtonDown(shift))
            ShiftOn();
         if (Input.GetButtonUp(shift))
            ShiftOff();


         if (!mobileInput && view.TypeOfMove == SLM_BH_View.tpe.sprites && view.sprites.RotateToMoveDir)
         {
            Vector3 mouse = Input.mousePosition;
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(mouse.x, mouse.y, transform.position.z));
            transform.LookAt2D(mouseWorld);
            transform.Rotate(new Vector3(0, 0, view.sprites.rotationOffset));
         }
         else if (!view.sprites.RotateToMoveDir)
         {
            if (((view.TypeOfMove != SLM_BH_View.tpe.sprites) && (Input.GetButtonDown(Axis_x) && Input.GetAxisRaw(Axis_x) > 0)) || ((view.TypeOfMove == SLM_BH_View.tpe.sprites) && curMoveV.x > 0))
               view.Move("r");
            else if (((view.TypeOfMove != SLM_BH_View.tpe.sprites) && (Input.GetButtonDown(Axis_x) && Input.GetAxisRaw(Axis_x) < 0)) || ((view.TypeOfMove == SLM_BH_View.tpe.sprites) && curMoveV.x < 0))
               view.Move("l");
            else if (((view.TypeOfMove != SLM_BH_View.tpe.sprites) && (Input.GetButtonDown(Axis_y) && Input.GetAxisRaw(Axis_y) > 0)) || ((view.TypeOfMove == SLM_BH_View.tpe.sprites) && curMoveV.y > 0))
               view.Move("u");
            else if (((view.TypeOfMove != SLM_BH_View.tpe.sprites) && (Input.GetButtonDown(Axis_y) && Input.GetAxisRaw(Axis_y) < 0)) || ((view.TypeOfMove == SLM_BH_View.tpe.sprites) && curMoveV.y < 0))
               view.Move("d");
            else if (Input.GetAxisRaw(Axis_x) == 0 && Input.GetAxisRaw(Axis_y) == 0)
            {
               view.StopMove();
            }
         }

         if (useUltimate)
         {
            if (Input.GetButtonDown(Ultimate))
            {
               RunUltimate();
            }
         }

         if (!autoShoot && cursh != null)
         {
            if (((Input.GetButton(shoot) && !mobileCustom) || shooting) && shoottimer == 0)
            {
               if (!cursh.gameObject.activeSelf)
               {
                  cursh.StartShotRoutine();
                  cursh.gameObject.SetActive(true);
               }
            }
            else
            {
               if (cursh.gameObject.activeSelf)
               {
                  cursh.StopAllCoroutines();
                  cursh.gameObject.SetActive(false);
               }
            }
         }

         if (autocontrolShoots)
         {
            if (!blocknextupd)
            {
               if (ShootUpdate == updtpe.score)
               {
                  if (nextupd < controll.score)
                     UpdateShoot(false);
               }
               else if (ShootUpdate == updtpe.time)
               {
                  timer += Time.deltaTime;
                  if (nextupd < timer)
                     UpdateShoot(false);
               }
               else if (ShootUpdate == updtpe.byButton)
               {
                  if (Input.GetButtonDown(NextWeapon))
                  {
                     UpdateShoot(false);
                  }
               }
            }
            if (!blockprevupd)
            {
               if (ShootUpdate == updtpe.score)
               {
                  if (prevupd > controll.score)
                     UpdateShoot(true);
               }
               else if (ShootUpdate == updtpe.byButton)
               {
                  if (Input.GetButtonDown(PrevWeapon))
                  {
                     UpdateShoot(true);
                  }
               }
            }


         }
      }
      else
      {
         if (freezetime >= 0)
         {
            if (!freeze)
            {
               if (cursh != null)
               {
                  cursh.StopAllCoroutines();
                  cursh.gameObject.SetActive(false);
               }
               freeze = true;
            }
            if (useFreezeColor)
               msr.color = freezeColor;
            freezetime -= Time.deltaTime;
         }
         else
         {
            if (cursh != null)
            {
               cursh.StartShotRoutine();
               cursh.gameObject.SetActive(true);
            }
            if (useFreezeColor)
               msr.color = defColor;
            freeze = false;
            freezetime = -1;
         }
      }



      if (imutineTimer > 0)
         imutineTimer -= Time.deltaTime;
      else
         imutineTimer = 0;

      if (!shoottimerstop && autocontrolShoots)
      {
         if (shoottimer > 0)
            shoottimer -= Time.deltaTime;
         else
         {
            shoottimer = 0;
            GetShoot();
            shoottimerstop = true;
         }
      }

      if (ulttimer > 0)
         ulttimer -= Time.deltaTime;
      else
         ulttimer = 0;
   }

   public void ShiftOn()
   {
      if (useshift)
      {
         nowShift = true;
         if (shiftObject != null)
         {
            shiftObject.SetActive(true);
         }
      }
   }

   public void ShiftOff()
   {

      if (useshift)
      {
         nowShift = false;
         if (shiftObject != null)
         {
            shiftObject.SetActive(false);
         }
      }
   }

   public void StopMove()
   {
      view.StopMove();
   }

   public void Move(Vector2 moveV)
   {
      if (freezetime == -1)
      {
         if (moveMode == MoveMode.transform)
         {
            Vector2 pos = transform.position;

            if (useshift)
               pos += moveV * (nowShift ? speedShift : speed) * Time.deltaTime;
            else
               pos += moveV * speed * Time.deltaTime;

            pos.x = Mathf.Clamp(pos.x, Min.x, Max.x);
            pos.y = Mathf.Clamp(pos.y, Min.y, Max.y);

            transform.position = pos;
         }
         else if (moveMode == MoveMode.rigitbody)
         {
            rigidbody2d.AddForce(moveV * ((useshift && nowShift) ? speedShift : speed) * Time.deltaTime);
            transform.position = new Vector2(Mathf.Clamp(transform.position.x, Min.x, Max.x), Mathf.Clamp(transform.position.y, Min.y, Max.y));
         }

         if (view.sprites.RotateToMoveDir)
            view.Move("move");
      }
   }

   public void Rotate(Vector2 dir)
   {
      if (freezetime == -1)
      {
         if (dir != Vector2.zero)
         {
            transform.ResetRotation();
            transform.Rotate(new Vector3(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg));
            transform.Rotate(new Vector3(0, 0, view.sprites.rotationOffset));
         }
      }
   }

   public void RunUltimate()
   {
      if (freezetime == -1)
      {
         if (useUltimate)
         {
            if (shoottimer == 0 && ulttimer == 0 && controll.ult > 0)
            {
               if (!useUltimateForEatchShoot)
               {
                  if (ultimateShoot != null)
                  {
                     ultimateShoot.StopAllCoroutines();
                     ultimateShoot.StartShotRoutine();
                  }
               }
               else
               {
                  if (ultimatesShoot.Count > curshoot && ultimatesShoot[curshoot] != null)
                  {
                     ultimatesShoot[curshoot].StopAllCoroutines();
                     ultimatesShoot[curshoot].StartShotRoutine();
                  }
               }
               ultimateEvent.Invoke();
               ulttimer = UltimateDelay;
               controll.ult--;
            }
         }
      }
   }

   public void SetShoot(int id)
   {
      if (id >= 0 && id < shoots.Count)
      {
         shoots[curshoot].shootObject.gameObject.SetActive(false);
         shoots[curshoot].shootObject.StopAllCoroutines();
         cursh = null;

         curshoot = id;
         shoots[curshoot].shootObject.gameObject.SetActive(true);
         shoots[curshoot].shootObject.StartShotRoutine();
         cursh = shoots[curshoot].shootObject;

         onChangeShoot.Invoke(curshoot);

         if (curshoot != shoots.Count - 1)
            blocknextupd = false;

         if (curshoot != 0)
            blockprevupd = false;

         if (ShootUpdate == updtpe.score)
         {
            prevupd = shoots[curshoot - 1].scoreToUpdate;
            if (curshoot == shoots.Count - 1)
            {
               nextupd = -1;
               blocknextupd = true;
            }
            else
               nextupd = shoots[curshoot + 1].scoreToUpdate;
         }
         else if (ShootUpdate == updtpe.time)
         {
            if (curshoot == shoots.Count - 1)
            {
               nextupd = -1;
               blocknextupd = true;
            }
            else
               nextupd = shoots[curshoot + 1].secToUpdate;
         }


      }
   }

   public void UpdateShoot(bool prev)
   {
      if (prev && ((prevupd != -1 && curshoot > 0) || ShootUpdate == updtpe.byButton || ShootUpdate == updtpe.byVoid))
      {
         shoots[curshoot].shootObject.gameObject.SetActive(false);
         shoots[curshoot].shootObject.StopAllCoroutines();
         cursh = null;

         if (curshoot > 0 || !loopweapons)
         {
            curshoot--;
         }
         else
         {
            curshoot = shoots.Count - 1;
         }
         onChangeShoot.Invoke(curshoot);

         shoots[curshoot].shootObject.gameObject.SetActive(true);
         shoots[curshoot].shootObject.StartShotRoutine();
         cursh = shoots[curshoot].shootObject;
         blocknextupd = false;

         if (ShootUpdate == updtpe.score)
         {
            nextupd = shoots[curshoot + 1].scoreToUpdate;
            if (curshoot == 0)
            {
               prevupd = -1;

               blockprevupd = true;
            }
            else
               prevupd = shoots[curshoot - 1].scoreToUpdate;
         }
      }
      else if (!prev && (nextupd != -1 || ShootUpdate == updtpe.byButton || ShootUpdate == updtpe.byVoid))
      {
         if (curshoot < shoots.Count - 1 || loopweapons)
         {
            shoots[curshoot].shootObject.gameObject.SetActive(false);
            shoots[curshoot].shootObject.StopAllCoroutines();
            cursh = null;
            if (curshoot < shoots.Count - 1 || !loopweapons)
               curshoot++;
            else
               curshoot = 0;

            onChangeShoot.Invoke(curshoot);

            shoots[curshoot].shootObject.gameObject.SetActive(true);
            shoots[curshoot].shootObject.StartShotRoutine();
            cursh = shoots[curshoot].shootObject;
            blockprevupd = false;

            if (ShootUpdate == updtpe.score)
            {
               prevupd = shoots[curshoot - 1].scoreToUpdate;
               if (curshoot == shoots.Count - 1)
               {
                  nextupd = -1;
                  blocknextupd = true;
               }
               else
                  nextupd = shoots[curshoot + 1].scoreToUpdate;
            }
            else if (ShootUpdate == updtpe.time)
            {
               if (curshoot == shoots.Count - 1)
               {
                  nextupd = -1;
                  blocknextupd = true;
               }
               else
                  nextupd = shoots[curshoot + 1].secToUpdate;
            }
         }
      }
   }


   void GetShoot()
   {
      List<UbhShotCtrl> shs = new List<UbhShotCtrl>();
      List<int> scu = new List<int>();

      foreach (SLM_BH_Player_Shoot s in shoots)
      {
         s.shootObject.gameObject.SetActive(false);
         s.shootObject.StopAllCoroutines();
         shs.Add(s.shootObject);
         if (ShootUpdate == updtpe.score)
            scu.Add(s.scoreToUpdate);
         else if (ShootUpdate == updtpe.time)
            scu.Add(s.secToUpdate);
      }



      if (shoottimer == 0)
      {
         //Debug.Log("here");
         if (ShootUpdate == updtpe.score && shs.Count > 0)
         {
            for (int i = shs.Count - 1; i >= 0; i--)
            {
               if (i != 0)
               {
                  if (controll.score > scu[i])
                  {
                     if (i != shs.Count - 1)
                     {
                        shoots[i].shootObject.gameObject.SetActive(true);
                        shoots[i].shootObject.StartShotRoutine();
                        cursh = shoots[i].shootObject;
                        curshoot = i;
                        nextupd = shoots[i + 1].scoreToUpdate;
                        if (i > 0)
                           prevupd = shoots[i - 1].scoreToUpdate;
                        else
                        {
                           prevupd = -1;
                           blockprevupd = true;
                        }
                     }
                     else
                     {
                        nextupd = -1;
                        shoots[i].shootObject.gameObject.SetActive(true);
                        shoots[i].shootObject.StartShotRoutine();
                        cursh = shoots[i].shootObject;
                        curshoot = i;
                        if (i > 0)
                           prevupd = shoots[i - 1].scoreToUpdate;
                        else
                        {
                           prevupd = -1;
                           blockprevupd = true;
                        }
                     }
                     break;
                  }
               }
               else
               {
                  prevupd = -1;
                  blockprevupd = true;
                  shoots[0].shootObject.gameObject.SetActive(true);
                  shoots[0].shootObject.StartShotRoutine();
                  cursh = shoots[0].shootObject;
                  curshoot = 0;
                  nextupd = shoots[1].scoreToUpdate;
               }
            }
         }
         else if (ShootUpdate == updtpe.time && shs.Count > 0)
         {
            shoots[0].shootObject.gameObject.SetActive(true);
            shoots[0].shootObject.StartShotRoutine();
            cursh = shoots[0].shootObject;
            curshoot = 0;
            blockprevupd = true;
            if (shs.Count > 1)
               nextupd = shoots[1].secToUpdate;
         }
         else if (ShootUpdate == updtpe.byVoid || ShootUpdate == updtpe.byButton && shs.Count > 0)
         {
            shoots[0].shootObject.gameObject.SetActive(true);
            shoots[0].shootObject.StartShotRoutine();
            cursh = shoots[0].shootObject;
            curshoot = 0;
         }
         else if (shs.Count == 0)
         {
            blockprevupd = true;
            blocknextupd = true;
         }
      }
   }

   private void OnTriggerEnter2D(Collider2D c)
   {
      if (damageTrigger == null)
      {
         if (c.tag == bulletTag)
         {
            UbhBullet bullet = c.gameObject.GetComponentInParent<UbhBullet>();
            if (bullet.isActive)
            {
               UbhObjectPool.instance.ReleaseBullet(bullet);
               Damage(c.name, false);
            }
         }
         else if (c.tag == enemyTag)
         {
            Damage(c.name, true);
         }
      }
   }

   public void OnTriggerEnterCheck(Collider2D c)
   {
      if (c.tag == bulletTag)
      {
         UbhBullet bullet = c.gameObject.GetComponentInParent<UbhBullet>();
         if (bullet.isActive)
         {
            UbhObjectPool.instance.ReleaseBullet(bullet);
            Damage(c.name, false);
         }
      }
      else if (c.tag == enemyTag)
      {
         Damage(c.name, true);
      }
   }

   public void Damage(string Enemyname, bool enemy, int damagecount = -1)
   {
      if (imutineTimer <= 0)
      {
         if (enemy)
         {
            if (controll.damageName.Contains(Enemyname))
            {
               controll.damagesCount[controll.damageName.IndexOf(Enemyname)]++;
               controll.lastdamage = Enemyname;
               if (controll.damages[controll.damageName.IndexOf(Enemyname)] > 0)
               {
                  if (damagecount == -1)
                     controll.hp -= controll.damages[controll.damageName.IndexOf(Enemyname)];
                  else
                     controll.hp -= damagecount;
               }
               else
                  return;
            }
            else
            {
               if (!string.IsNullOrEmpty(Enemyname))
                  controll.lastdamage = Enemyname;

               if (controll.damagebyEnemy > 0)
               {
                  if (damagecount == -1)
                     controll.hp -= controll.damagebyEnemy;
                  else
                     controll.hp -= damagecount;

               }
               else
                  return;
            }
         }
         else
         {
            if (controll.damageName.Contains(Enemyname))
            {
               controll.damagesCount[controll.damageName.IndexOf(Enemyname)]++;
               controll.lastdamage = Enemyname;
               if (controll.damages[controll.damageName.IndexOf(Enemyname)] > 0)
               {
                  if (damagecount == -1)
                     controll.hp -= controll.damages[controll.damageName.IndexOf(Enemyname)];
                  else
                     controll.hp -= damagecount;
               }
               else
                  return;
            }
            else
            {
               if (!string.IsNullOrEmpty(Enemyname))
                  controll.lastdamage = Enemyname;

               if (damagecount == -1)
                  controll.hp -= 1;
               else
                  controll.hp -= damagecount;
            }
         }

         onDamage.Invoke();

         if (controll.hp > 0)
         {
            imutineTimer = immortalityAfterDamage;
            view.Damage();
         }
         else
         {
            controll.EndBH(false);
            view.Die();
         }
      }
   }
}

[System.Serializable]
public class SLM_BH_Player_Shoot
{
    public UbhShotCtrl shootObject;
    public int scoreToUpdate;
    public int secToUpdate;
}