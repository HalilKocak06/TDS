using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftController : MonoBehaviour
{

    [SerializeField] Animator animator;

    public bool isUp {get; private set; }= false;

    void Start()
    {
        //Oyun başlar başlamaz idle pozisyonuna geç
        animator.Play("Lift_IdleDown", 0, 0f);
        isUp = false;   
    }

    public void ToggleLift()
    {
        if(!isUp)
        {
            //Aşağıdan yukarıya çık
            animator.Play("Lift_Up");
            isUp =true;
        }
        else
        {
            //Yukarıdan aşağı in
            animator.Play("Lift_Down");
            isUp=false;
        }
    }
}
