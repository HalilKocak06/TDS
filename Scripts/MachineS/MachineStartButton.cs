using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Buradaki tek yaptığımız şey Düğmeye basıldığında Press sınıfı çağrılıyor ve oradan da makineyi çalıştırıyoruz.


public class MachineStartButton : MonoBehaviour
{
    [SerializeField] TireChangerMachineController machine;  //zaten bu obje ve sınıf

    void Awake()
    {
        if (!machine) machine = GetComponentInParent<TireChangerMachineController>();
    }
    
    public void Press()
    {
        if (!machine)
        {
            Debug.LogWarning("MachineStartButton: TireChangerMachineController yokk");
            return;
        }

        machine.TryStart();
        Debug.Log("Start button pressed (machine.TryStart)");
    }
}
