using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AISlime : Slime
{

    private Slime targetSlime;
    private bool aggrod;
    private float aggroableMultiplier;
    private float aggroDelay;
    private float reactionDelay;
    private float doubleJumpTiming;
    private float doubleJumpAdjust;

    void Start(){
      // initialization/setup
      rm = GameObject.Find("RunManager").GetComponent<RunManager>();
      healthUI = GameObject.Find("HUDCanvas/OpponentPanel/HP").GetComponent<TMP_Text>();
      damageUI = GameObject.Find("HUDCanvas/OpponentPanel/Damage").GetComponent<TMP_Text>();
      livesUI = GameObject.Find("HUDCanvas/OpponentPanel/Lives").GetComponent<TMP_Text>();
      // targetSlime = GameObject.Find("PlayerSlime").GetComponent<Slime>();
      targetSlime = rm.Player().GetComponent<Slime>();

      // layer definition, pls ignore
      enemyLayer = new LayerMask();
      enemyLayer &= ~(1 << 1);
      enemyLayer |= (1 << 10);
      doubleJumpAdjust = 1f;

      // leveling/stas logic
      level = rm.Level();
      HP = 100f + (5f*level);
      maxHP = HP;
      lives = 3;
      damage = 3f + (1.5f*(float)level);
      aggroableMultiplier = 0.15f;

      healthUI.text = HP.ToString();
      damageUI.text = damage.ToString();
      livesUI.text = lives.ToString();

      // reaction/delay specific params
      doubleJumpTiming = 2/3;
      aggroDelay = 48f * doubleJumpTiming;
      reactionDelay = 24f * doubleJumpTiming;


      // functions to cue AI related coroutines
      Stalk();
      Aggro();
    }

    void Update(){
      base.Update();
    }

    public void Stalk(){
      StartCoroutine("StalkCooldown");
    }

    public void Aggro(){
      StartCoroutine("AggroCooldown");
    }

    private Collider isPlatformReachable(){
      Collider[] hitColliders = Physics.OverlapSphere(groundCheck.position, jumpingHeight, groundLayer);
      if(hitColliders.Length > 0){
        return hitColliders[0];
      }
      return null;
    }


    protected Collider isAggroable(){
      float radius = 50f;//horizontal == 0f ? 25f : 50f;
      Collider[] hitColliders = Physics.OverlapSphere(groundCheck.position, radius * aggroableMultiplier, enemyLayer);
      if(hitColliders.Length > 0){
        return hitColliders[0];
      }
      return null;
    }

    IEnumerator AggroCooldown() {
      Collider aggro = isAggroable();
      if(aggro != null && !aggrod && !running && !melee && !knocked){
        aggrod = true;
        StartCoroutine("MeleeCooldown");
      }
      yield return new WaitForSeconds(aggroDelay);
      aggrod = false;
      StartCoroutine("AggroCooldown");
    }

    IEnumerator StalkCooldown() {
      if(knocked || aggrod){
        Debug.Log("Stalking But Knocked");
      }else if(rb.position.y - targetSlime.transform.position.y >= 30f){
        horizontal = 1f;
      }else if(targetSlime.transform.position.x < rb.position.x){
        horizontal = -1f;
      }else if(targetSlime.transform.position.x > rb.position.x){
        horizontal = 1f;
      }
      if(targetSlime.transform.position.y - rb.position.y >= 30f && isPlatformReachable()){
        jump(true);
      }
      yield return new WaitForSeconds(reactionDelay);
      StartCoroutine("StalkCooldown");
    }


}
