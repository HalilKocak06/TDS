using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitZone : MonoBehaviour
{
    //Hangi bay'i finalize edeceğiz.
    [SerializeField] ServiceBay bay;
    public ServiceBay Bay => bay;
}
