using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AISlimeNuclear : Slime
{

    private Slime targetSlime;
    private float aggroableMultiplier;
    private float aggroDelay;
    private bool aggrod;
    private float reactionDelay;
    private float doubleJumpTiming;
    private float doubleJumpAdjust;

    void Start(){
      rm = GameObject.Find("RunManager").GetComponent<RunManager>();
      healthUI = GameObject.Find("HUDCanvas/OpponentPanel/HP").GetComponent<TMP_Text>();
      level = rm.Level();
      HP = 100f + (10f*level);
      damage = 2f + (3f*level);
      aggroableMultiplier = 0.5f;
      aggroDelay = 1.5f;
      healthUI.text = HP.ToString();
      targetSlime = GameObject.Find("PlayerSlime").GetComponent<Slime>();
      enemyLayer = new LayerMask();
      enemyLayer &= ~(1 << 1);
      enemyLayer |= (1 << 10);
      doubleJumpTiming = 2/3;
      doubleJumpAdjust = 1f;
      reactionDelay = doubleJumpTiming;

      Stalk();
      // Aggro();
    }

    void Update(){
      base.Update();
      Collider aggro = isAggroable();
      if(aggro != null){
        StartCoroutine("AggroCooldown");
      }
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
      if(!running && !melee && !knocked){
        aggrod = true;
        StartCoroutine("MeleeCooldown");
      }
      yield return new WaitForSeconds(aggroDelay);
      aggrod = false;
      // StartCoroutine("AggroCooldown");
    }

    IEnumerator StalkCooldown() {
      if(rb.position.y - targetSlime.transform.position.y >= 30f){
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
