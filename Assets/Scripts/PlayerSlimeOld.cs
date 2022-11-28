using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSlimeOld : MonoBehaviour
{
    
    public Rigidbody rb ;
    public LineRenderer directionIndicator ;
    public Transform groundCheck;

    public LayerMask groundLayer;
    public LayerMask wallLayer;

    protected float horizontal;
    protected float speed = 240f;
    public Vector3 jumpingOrigin;
    protected float jumpingPower = 240f;
    protected float jumpingHeight = 50f;
    protected bool isFacingRight = true;
    protected bool running = false;
    protected bool melee = false;
    protected bool range = false;
    protected bool aoe = false;
    protected bool hopped = false;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
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
      }
      // no wall, move
      else if(wall == null){
          rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
      }
      // wall to left and pressing right, or to the right and pressing left, move
      else if((wall.transform.position.x < 0f  && isFacingRight) ||
                (wall.transform.position.x > 0f && !isFacingRight)){
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


    private bool isGrounded(){
      Collider[] hitColliders = Physics.OverlapSphere(groundCheck.position, 2f, groundLayer);
      return hitColliders.Length > 0;
    }

    private Collider isWalled(){
      Collider[] hitColliders = Physics.OverlapSphere(groundCheck.position, 10f, wallLayer);
      if(hitColliders.Length > 0){
        return hitColliders[0];
      }
      return null;
    }

    private void Flip(){
        isFacingRight = !isFacingRight;
        transform.Rotate(0, 0, 180f);
    }

    public void Move(InputAction.CallbackContext context){
        // if(running){
        //   horizontal = context.ReadValue<Vector2>().x;
        // }else {
          Vector3 cardinal = new Vector3();
          float vertical = context.ReadValue<Vector2>().y;
          horizontal = context.ReadValue<Vector2>().x;
          cardinal.y = horizontal != 0f ? 160f : 0f;
          cardinal.z = vertical == 0f ? 60f : (vertical < 0f ? -60f : 180f);
          directionIndicator.SetPosition(1,cardinal);

        // }
    }

    public void Melee(InputAction.CallbackContext context){
      if(context.performed && !running && !melee){
        StartCoroutine("MeleeCooldown");
      }
    }

    IEnumerator MeleeCooldown()
    {
        Renderer damageRadius = GameObject.Find("PlayerSlime/DamageRadius").GetComponent<Renderer>();
        damageRadius.enabled = true;
        melee = true;
        float prevSpeed = speed;
        speed *= 2.1f;

        yield return new WaitForSeconds(0.15f);
        damageRadius.enabled = false;
        melee = false;
        speed = prevSpeed;
    }

    public void Dash(InputAction.CallbackContext context){
      if(context.performed && !running && !melee){
        StartCoroutine("DashCooldown");
      }
    }

    IEnumerator DashCooldown()
    {
        running = true;
        float prevSpeed = speed;
        speed *= 2.5f;
        yield return new WaitForSeconds(0.15f);
        running = false;
        speed = prevSpeed;
    }


    public void Jump(InputAction.CallbackContext context){
      bool grounded = isGrounded();
      Collider wall = isWalled();
      if(context.performed && (wall != null || grounded || !hopped)){
        if((!grounded && wall == null) && !hopped){
          hopped = true;
        }
        jumpingOrigin = rb.position;
        rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
        // rb.velocity = new Vector2(rb.velocity.x, Mathf.Sqrt(-2.0f * jumpingHeight));
      }
      //rb.velocity.y
      // if(context.canceled && rb.velocity.y > 0f){
      if(context.canceled && rb.velocity.y > 0f){
        // rb.velocity = new Vector2(rb.velocity.x, jumpingPower * -1f);
        rb.velocity = new Vector2(rb.velocity.x, jumpingPower * -1f);
      }
    }
}
