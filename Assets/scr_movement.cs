using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class scr_movement : MonoBehaviour
{

    
    public float speed = 8f;
    
    public float jumpForce = 10f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Rigidbody2D rb;

    float horizontalMovement;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    private bool isGrounded;

    public float Gravity = 1f;
    public float FallingGravity = 3f;

    public float Fallingspeed = 0.005f;
    public float CoyoteTime = 0.2f;
    public float CoyoteTimetimer;

    public int extraJumpsVal = 2;
    private int extraJumps = 0;

    public float jumpBufferTime = 0.15f;
    private float jumpBufferCounter;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        extraJumps = extraJumpsVal;
        CoyoteTimetimer = CoyoteTime;

    }

    // Update is called once per frame
    void Update()
    { // gracz po osiągnięciu maksymalnej wysokości spada szybciej
       if (rb.linearVelocity.y < 0)
    { //implementacja szybkości
        rb.gravityScale += Time.deltaTime*2; 
        if (rb.gravityScale >= FallingGravity)
         {// upewnienie się czy prędkość nie przekracza limitu
            rb.gravityScale = FallingGravity;
         }
    }
    else
    { //reset grawitacji
        rb.gravityScale = Gravity;
    }

        // movement prawo lewo z użyciem 1 lub -1 
         float moveInput = Input.GetAxis("Horizontal");
         rb.linearVelocity = new Vector2(moveInput * speed,rb.linearVelocity.y);
         
          
        if (isGrounded)
        { //sprawdzenie czy gracz jest na ziemi 
            Gravity = 1;
            extraJumps = extraJumpsVal;
            CoyoteTimetimer = CoyoteTime;
        }else
        {
            CoyoteTimetimer -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;

        }
            //ibiza 
        if (jumpBufferCounter > 0f)
        {
         if (CoyoteTimetimer > 0f)
            {
                
                rb.linearVelocity = new Vector2(rb.linearVelocity.x,jumpForce);
                print(extraJumps);
                CoyoteTimetimer = 0f;
            }
            else if(extraJumps > 0)   
            {
                
                rb.linearVelocity = new Vector2(rb.linearVelocity.x,jumpForce);
                extraJumps --;
                
            }    
            
            
        }
    }

    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius,groundLayer);

    }

}





