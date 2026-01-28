using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxController : MonoBehaviour
{
  
  [SerializeField] LiftController lift; //obje lift kontrolü
  [SerializeField] Transform player;
  [SerializeField] float interactDistance = 2.5f;

    void Update()
    {
        if(Vector3.Distance(player.position, transform.position) <= interactDistance)
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                lift.ToggleLift();
            }
        }
    }
}
