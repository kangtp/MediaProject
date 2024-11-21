using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationAndMovementController : MonoBehaviour
{
    PlayerInput playerInput;
    CharacterController characterController;
    Animator animator;
    public Transform realCamera;
    public Transform characterBody;

    Vector2 currentMovementInput;
    Vector3 currentMovement;
    Vector3 currentRunMovement;
    Vector3 currentGravity;

    bool isMovementPressed;
    bool isRunPressed;
    bool isJumpPressed = false;
    float initialJumpVelocity;
    float maxJumpHeight = 4.0f;
    float maxJumpTime = 0.75f;
    bool isJumping = false;

    float gravity = -9.8f;
    float groundedGravity = -.05f;

    float rotationFactorPerFrame = 15.0f;
    public float walkMultiplier = 2.0f;
    public float runMultiplier = 4.0f;

    int isWalkingHash;
    int isRunningHash;
    int isJumpingHash;
    bool isJumpAnimating = false;
    private void Awake()
    {
        playerInput = new PlayerInput();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
        isJumpingHash = Animator.StringToHash("isJumping");

        playerInput.CharacterControls.Move.started += onMovementInput;
        playerInput.CharacterControls.Move.canceled += onMovementInput;
        playerInput.CharacterControls.Move.performed += onMovementInput;
        playerInput.CharacterControls.Run.started += onRun;
        playerInput.CharacterControls.Run.canceled += onRun;
        playerInput.CharacterControls.Jump.started += onJump;
        playerInput.CharacterControls.Jump.canceled += onJump;

        setupJumpVariables();
    }

    void setupJumpVariables()
    {
        float timeToApex = maxJumpTime / 2;
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
    }

    void handleJump()
    {
        if(!isJumping && characterController.isGrounded && isJumpPressed)
        {
            //video version
            isJumping = true;
            animator.SetBool(isJumpingHash, true);
            isJumpAnimating = true;

            //currentMovement.y = initialJumpVelocity;
            //currentRunMovement.y = initialJumpVelocity;

            //my version
            currentGravity.y = initialJumpVelocity * 0.5f;

        }
        else if(!isJumpPressed && isJumping &&  characterController.isGrounded)
        {
            isJumping = false;
        }
    }
    void handleRotation()
    {
        //Vector3 positionToLookAt;
        //positionToLookAt.x = currentMovement.x;
        //positionToLookAt.y = 0.0f;
        //positionToLookAt.z = currentMovement.z;
        //Quaternion currentRotation = transform.rotation;

        //if (isMovementPressed)
        //{
        //    Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
        //    transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame * Time.deltaTime);
        //}


        //수정한 코드

        if (isMovementPressed)
        {
            Vector3 lookForward = new Vector3(realCamera.forward.x, 0f, realCamera.forward.z).normalized;
            Vector3 lookRight = new Vector3(realCamera.right.x, 0f, realCamera.right.z).normalized;
            currentMovement = lookForward * currentMovementInput.y * walkMultiplier + lookRight * currentMovementInput.x * walkMultiplier;
            currentRunMovement = lookForward * currentMovementInput.y * runMultiplier + lookRight * currentMovementInput.x * runMultiplier;
            //characterBody.forward = moveDir;
            //transform.position += moveDir * Time.deltaTime * 5f; //원래

            //characterController.Move(moveDir * walkSpeed * Time.deltaTime);
            
            if(currentMovement.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(currentMovement);
                characterBody.rotation = Quaternion.Slerp(characterBody.rotation, targetRotation, rotationFactorPerFrame * Time.deltaTime);
            }
        }
        else
        {
            //currentMovement.x = 0f;
            //currentMovement.z = 0f;
            //currentRunMovement.x = 0f;
            //currentRunMovement.z = 0f;
            currentMovement = Vector3.zero;
            currentRunMovement = Vector3.zero;
        }


    }

    void onMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        //currentMovement.x = currentMovementInput.x;
        //currentMovement.z = currentMovementInput.y;
        //currentRunMovement.x = currentMovementInput.x * runMultiplier;
        //currentRunMovement.z = currentMovementInput.y * runMultiplier;
        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }

    void onRun(InputAction.CallbackContext context)
    {
        isRunPressed = context.ReadValueAsButton();
    }

    void onJump(InputAction.CallbackContext context)
    {
        isJumpPressed = context.ReadValueAsButton();
        Debug.Log(isJumpPressed);
    }

    void handleAnimation()
    {
        bool isWalking = animator.GetBool(isWalkingHash);
        bool isRunning = animator.GetBool(isRunningHash);

        if(isMovementPressed && !isWalking) {
            animator.SetBool(isWalkingHash, true);
        }
        else if(!isMovementPressed && isWalking) {
            animator.SetBool(isWalkingHash, false);
        }

        if((isMovementPressed && isRunPressed) && !isRunning)
        {
            animator.SetBool(isRunningHash, true);
        }
        else if((!isMovementPressed || !isRunPressed) && isRunning)
        {
            animator.SetBool(isRunningHash, false);
        }


    }


    void handleGravity()
    {
        bool isFalling = currentGravity.y <= 0.0f; //속도가 0이면 이제 떨어질 상태
        float fallMultiplier = 1.5f;    //낙하 속도 강화를 위한 변수

        if (characterController.isGrounded)
        {
            //currentMovement.y = groundedGravity;
            //currentRunMovement.y = groundedGravity;
            if(isJumpAnimating) //성능 향상의 효과
            {
                animator.SetBool(isJumpingHash, false);
                isJumpAnimating = false;
            }
            
            //my version
            currentGravity.y = groundedGravity;
        }
        //점프 시, 떨어지면서 받는 중력(가속을 더 받음)
        else if(isFalling) {
            float previousYVelocity = currentGravity.y;
            float newYVelocity = currentGravity.y + (gravity * fallMultiplier * Time.deltaTime);
            float nextYVelocity = (previousYVelocity + newYVelocity) * 0.5f;
            currentGravity.y = nextYVelocity;
        }
        //점프 시, 올라가면서 받는 중력
        else
        {
            //currentMovement.y += gravity * Time.deltaTime;
            //currentRunMovement.y += gravity * Time.deltaTime;

            //my version
            float previousYVelocity = currentGravity.y;
            float newYVelocity = currentGravity.y + (gravity * Time.deltaTime);
            float nextYVelocity = (previousYVelocity + newYVelocity) * 0.5f;
            currentGravity.y = nextYVelocity;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        handleRotation();
        handleAnimation();

        //if (isRunPressed)
        //{
        //    characterController.Move(currentRunMovement * runSpeed * Time.deltaTime);
        //}
        //else
        //{
        //    characterController.Move(currentMovement * walkSpeed * Time.deltaTime);
        //}

        Vector3 moveDirection = isRunPressed ? currentRunMovement : currentMovement;
        characterController.Move(new Vector3(moveDirection.x, currentGravity.y, moveDirection.z) * Time.deltaTime);
        
        //characterController.Move(moveDirection * Time.deltaTime);

        handleGravity();
        handleJump();
    }

    void OnEnable()
    {
        playerInput.CharacterControls.Enable();
    }

    void OnDisable()
    {
        playerInput.CharacterControls.Disable();
    }
}
