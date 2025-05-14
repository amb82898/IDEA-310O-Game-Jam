using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animationStateController : MonoBehaviour
{
    Animator animator;
    int isWalkingHash;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        bool isRunning = animator.GetBool("isRunning");
        bool isWalking = animator.GetBool("isWalking");
        bool forwardPressed = Input.GetKey("w");
        bool runPressed = Input.GetKey("left shift");

        if (!isWalking && forwardPressed)
        {
            animator.SetBool("isWalking", true);
        }

        if (isWalking && !forwardPressed)
        {
            animator.SetBool("isWalking", false);
        }

        if (!isRunning && (forwardPressed && runPressed))
        {
            animator.SetBool("isRunning", true);
        }

        if (isRunning && (!forwardPressed || !runPressed))
        {
            animator.SetBool("isRunning", false);
        }

        bool isWalkingBackward = animator.GetBool("isWalkingBackward");
        bool backwardPressed = Input.GetKey("s");
        if (!isWalkingBackward && backwardPressed)
        {
            animator.SetBool("isWalkingBackward", true);
        }

        if (isWalkingBackward && !backwardPressed)
        {
            animator.SetBool("isWalkingBackward", false);
        }

        bool isWalkingLeft = animator.GetBool("isWalkingLeft");
        bool leftPressed = Input.GetKey("a");
        if (!isWalkingLeft && leftPressed)
        {
            animator.SetBool("isWalkingLeft", true);
        }

        if (isWalkingLeft && !leftPressed)
        {
            animator.SetBool("isWalkingLeft", false);
        }

        bool isWalkingRight = animator.GetBool("isWalkingRight");
        bool rightPressed = Input.GetKey("d");
        if (!isWalkingRight && rightPressed)
        {
            animator.SetBool("isWalkingRight", true);
        }

        if (isWalkingRight && !rightPressed)
        {
            animator.SetBool("isWalkingRight", false);
        }
      
        bool mousePressed = Input.GetKey(KeyCode.Mouse0);
        if (mousePressed)
        {
            animator.SetTrigger("Attack");
        }

       


    }
}
