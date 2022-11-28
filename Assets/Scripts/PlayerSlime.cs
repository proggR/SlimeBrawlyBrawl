using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerSlime : Slime
{
    void Start(){
      rm = GameObject.Find("RunManager").GetComponent<RunManager>();
      healthUI = GameObject.Find("HUDCanvas/PlayerPanel/HP").GetComponent<TMP_Text>();
      damageUI = GameObject.Find("HUDCanvas/PlayerPanel/Damage").GetComponent<TMP_Text>();
      livesUI = GameObject.Find("HUDCanvas/PlayerPanel/Lives").GetComponent<TMP_Text>();
      HP = 125f;
      lives = 3;
      maxHP = HP;
      damage = 5f;
      level = 0;      
      //strikeRadiusMultiplier = 1.2f;
    }

    public void Move(InputAction.CallbackContext context){
      Vector3 cardinal = new Vector3();
      float vertical = context.ReadValue<Vector2>().y;
      horizontal = context.ReadValue<Vector2>().x;
      cardinal.y = horizontal != 0f ? 160f : 0f;
      cardinal.z = vertical == 0f ? 60f : (vertical < 0f ? -60f : 180f);
      directionIndicator.SetPosition(1,cardinal);
    }

    public void Melee(InputAction.CallbackContext context){
      if(context.performed && !knocked && !running && !melee){
        StartCoroutine("MeleeCooldown");
      }
    }

    public void Dash(InputAction.CallbackContext context){
      if(context.performed && !knocked && !running && !melee){
        StartCoroutine("DashCooldown");
      }
    }

    public void Jump(InputAction.CallbackContext context){
      jump(context.performed);
    }
}
