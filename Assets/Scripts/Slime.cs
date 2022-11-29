using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Slime : MonoBehaviour
{

    public RunManager rm ;
    public Rigidbody rb ;
    public LineRenderer directionIndicator ;
    public Transform groundCheck;

    public TMP_Text healthUI;
    public TMP_Text damageUI;
    public TMP_Text livesUI;

    public LayerMask groundLayer;
    public LayerMask wallLayer;
    public LayerMask enemyLayer;

    protected int level;
    protected float HP;
    protected float maxHP;
    protected float damage;
    protected int lives;


    protected float horizontal;
    protected float speed = 240f;
    public Vector3 jumpingOrigin;
    protected float strikeRadiusMultiplier = 1f;
    protected float baseHP = 110f;
    protected float kickbackHorizontal = 1.3f;
    protected float kickbackVertical = 1.3f;
    protected float jumpingPower = 240f;
    protected float jumpingHeight = 50f;
    protected bool isFacingRight = true;
    protected bool knocked = false;
    protected bool running = false;
    protected bool melee = false;
    protected bool range = false;
    protected bool aoe = false;
    protected bool hopped = false;

    // Update is called once per frame
    protected void Update()
    {
      if(rb.position.y <= -100f){
        rb.position = new Vector3(0,20f,0);
      }

      // currently in knocked state, but grounded, reset knock
      if(knocked && isGrounded()){
        knocked = false;
      }

      // you were falling, but now are grounded
      if(rb.velocity.y < 0f && isGrounded()){
        rb.velocity = new Vector2(horizontal * speed, 0f);
        hopped = false;
      }
      // you're not jumping or falling, but you're no longer grounded
      else if(rb.velocity.y == 0f && !isGrounded()){
          rb.velocity = new Vector2(horizontal * speed, jumpingPower * -1f);
      }

      Collider wall = isWalled();


      // airborn with no wall and max jump height, start to fall
      if(wall == null && rb.velocity.y > 0f && !isGrounded() && rb.position.y - jumpingOrigin.y >= jumpingHeight){
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y * -1.0f);
        knocked = false;
      }
      // no wall and not knocked, move
      else if(!knocked && wall == null){
          rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
      }
      // wall to left and pressing right, or to the right and pressing left, move
      else if(wall != null && ((wall.transform.position.x < 0f  && isFacingRight) ||
                (wall.transform.position.x > 0f && !isFacingRight))){
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
      }
      //you're running into a wall, no move
      else{
          rb.velocity = new Vector2(0f, rb.velocity.y);
      }


      if(!isFacingRight && horizontal > 0f){
        Flip();
      }else if (isFacingRight && horizontal < 0f){
        Flip();
      }
    }

    protected void jump(bool context){
      bool grounded = isGrounded();
      Collider wall = isWalled();
      if(context && (wall != null || grounded || !hopped)){
        if((!grounded && wall == null) && !hopped){
          hopped = true;
          rm.Action("DoubleJump");
        }else{
          rm.Action("Jump");
        }
        jumpingOrigin = rb.position;
        rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
      }

      if(!context && rb.velocity.y > 0f){
        rb.velocity = new Vector2(rb.velocity.x, jumpingPower * -1f);
      }
    }

    public void Knock(Slime slime){
      Collide();
      Damage(slime);
      slime.KickBack(this);
      knocked = true;
      bool grounded = isGrounded();
      Collider wall = isWalled();
      float xVal = slime.transform.position.x <= rb.position.x ? 1f : -1f;
      float yVal = slime.transform.position.y <= rb.position.y ? 1f : -1f;
      if(wall != null){
        xVal *= -1f;
        yVal = -1f;
      }
      horizontal = xVal >= 1f || xVal <= -1f ? xVal : 1f;
      // if((wall.transform.position.x < 0f  && isFacingRight) ||
      //           (wall.transform.position.x > 0f && !isFacingRight)){
      //   rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
      // }
      jumpingOrigin = rb.position;
      rb.velocity = new Vector2(horizontal * speed, jumpingPower * yVal);
    }

    public void KickBack(Slime slime){
      knocked = true;
      bool grounded = isGrounded();
      Collider wall = isWalled();
      // float xVal = slime.transform.position.x <= rb.position.x ? 1f : -1f;
      float xVal = -1f;
      float yVal = slime.transform.position.y <= rb.position.y ? 1f : -1f;
      if(wall != null){
        xVal *= -1f;
        yVal = -1f;
      }
      horizontal = xVal;
      // if((wall.transform.position.x < 0f  && isFacingRight) ||
      //           (wall.transform.position.x > 0f && !isFacingRight)){
      //   rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
      // }
      jumpingOrigin = rb.position;
      rb.velocity = new Vector2(horizontal * (kickbackHorizontal*speed), (kickbackVertical*jumpingPower) * yVal);
    }

    public void LevelUp(){
      damage++;
      level++;
      lives = 3;
      HP = baseHP + (2f*level);
      maxHP = HP;
      healthUI.text = HP.ToString();
      damageUI.text = damage.ToString();
      livesUI.text = lives.ToString();
    }

    public void ResetLevel(){
      level = 0;
    }

    public int GetLevel(){
      return level;
    }

    public void NextLife(){
      lives--;
      livesUI.text = lives.ToString();
      // if(LastLife()){
        HP = maxHP;
      // }
    }


    public void ResetHP(){
      HP = maxHP;
    }

    public bool LastLife(){
      return lives == 0;
    }

    protected float GetHP(){
      return HP;
    }

    protected float GetDamage(){
      return damage;
    }

    protected void Collide(){
      StartCoroutine("CollideCooldown");
    }

    protected void Damage(Slime slime){
      rm.Hit();
      HP -= slime.GetDamage();
      healthUI.text = HP.ToString();
      if(HP <= 0f){
        rm.Resolve(slime, this);
      }
    }

    protected bool isGrounded(){
      Collider[] hitColliders = Physics.OverlapSphere(groundCheck.position, 2f, groundLayer);
      return hitColliders.Length > 0;
    }

    protected Collider isWalled(){
      Collider[] hitColliders = Physics.OverlapSphere(groundCheck.position, 10f, wallLayer);
      if(hitColliders.Length > 0){
        return hitColliders[0];
      }
      return null;
    }

    protected Collider isStrikeable(){
      float radius = 50f;//horizontal == 0f ? 25f : 50f;
      Collider[] hitColliders = Physics.OverlapSphere(groundCheck.position, radius * strikeRadiusMultiplier, enemyLayer);
      if(hitColliders.Length > 0){
        return hitColliders[0];
      }
      return null;
    }

    protected void Flip(){
        isFacingRight = !isFacingRight;
        transform.Rotate(0, 0, 180f);
    }

    IEnumerator CollideCooldown()
    {
        Collider enemy;
        Renderer damageRadius = transform.Find("DamageRadius").GetComponent<Renderer>();
        damageRadius.enabled = true;
        yield return new WaitForSeconds(0.15f);
        damageRadius.enabled = false;
    }

    IEnumerator MeleeCooldown()
    {
        Collider enemy;
        melee = true;

        //grab any enemy in range at start of dash
        enemy = isStrikeable();
        if(enemy != null){
          Debug.Log("HIT");
          enemy.gameObject.GetComponent<Slime>().Knock(this);
        }else{
          Debug.Log("NO HIT");
        }

        float prevSpeed = speed;
        speed *= 2.1f;
        yield return new WaitForSeconds(0.15f);

        //if no enemy was in range at start of dash, grab any enemy in range at end of dash
        if(enemy == null){
          enemy = isStrikeable();
          if(enemy != null){
            Debug.Log("HIT");
            enemy.gameObject.GetComponent<Slime>().Knock(this);
          }else{
            Debug.Log("NO HIT");
          }
        }

        melee = false;
        speed = prevSpeed;
    }

    IEnumerator DashCooldown()
    {
        rm.Action("Dash");
        running = true;
        float prevSpeed = speed;
        speed *= 2.5f;
        yield return new WaitForSeconds(0.15f);
        running = false;
        speed = prevSpeed;
    }

}
