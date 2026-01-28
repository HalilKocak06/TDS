using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitPhysicsToggle : MonoBehaviour
{
   [SerializeField] Collider col;
   [SerializeField] Rigidbody rb;

   void Reset()
    {
        col = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
    }

    void Awake()
    {
        DisableAll(); //wheel altındaki sistem bozulmasın.
    }

    public void DisableAll()
    {
        if (!col) col = GetComponent<Collider>();
        if (!rb) rb = GetComponent<Rigidbody>();

        if(col) col.enabled = false;

        if(rb)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    // Split sonrası LASTİK YERDE SERBEST kalsın (al bırak yapılacak)

    public void SetLoose(bool loose)
    {
         if (!col) col = GetComponent<Collider>();
        if (!rb)  rb  = GetComponent<Rigidbody>();

        if (col) col.enabled = loose;

        if (rb)
        {
            rb.isKinematic = !loose;
            rb.useGravity = loose;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    // Split sonrası JANT MAKİNEDE SABİT kalsın ama E ile alınabilir olsun
    public void SetOnMachine(bool onMachine)
    {
        if (!col) col = GetComponent<Collider>();
        if (!rb)  rb  = GetComponent<Rigidbody>();

        if (col) col.enabled = onMachine;          // E raycast vursun diye AÇIK
        if (rb)
        {
            rb.isKinematic = onMachine;            // makinede sabit
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
    
}
