using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxController : MonoBehaviour
{
  
  [SerializeField] LiftController lift; //obje lift kontrolü
  
    public void Interact()
    {
        if(lift== null)
        {
            Debug.LogWarning("boxkontrtoller: lift refarans yok");
            return;
        }

        lift.ToggleLift();
    }
}
