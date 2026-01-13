using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericCarryable : MonoBehaviour
{
    Collider col;
    Rigidbody rb;

    void Awake()
    {
        col = GetComponent<Collider>(); //Collider'i col'a eşitledipk o objedeki.
        rb = GetComponent<Rigidbody>(); // Rigidbody 'yi rbye eşitledik o objedeki
    }

    public void SetCarried(bool carried)
    {
        //carried true-false olan ve başka bir yerden gelen //
         if (col) col.enabled = !carried;

         if (rb)
        {
            rb.isKinematic = carried;
            rb.useGravity = !carried;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
