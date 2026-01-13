using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Bu kodun asıl amacı tekeri elde taşınabilir yapmamız !!!
//WheelCarryable, bir tekerleği:
//elde taşınabilir yapar
//eldeyken fizik + collider kapatır
//yere bırakıldığında fiziği geri açar
//ama her zaman değil → sadece bijonlar söküldüyse

public class WheelCarryable : MonoBehaviour
{
    Rigidbody rb; //rigidboy referans
    Collider[] cols; //collider array list teker,jant, lastik collider olduğu için
    WheelController wheel;  //wheelcontroller sınıfı bu tekeer sökülebilir mi bilgisini WheelController'a bağımlı

    void Awake()
    {
        rb = GetComponentInChildren<Rigidbody>();
        cols = GetComponentsInChildren<Collider>(); // Teker modeli genelde çok parçalıdır. Tek collider değil → hepsini kapatman gerekir

        wheel = GetComponent<WheelController>();

    }

    public bool CanPickUp => wheel != null && wheel.IsUnlocked; //Bu teker SADECE bijonlar sökülmüşse alınabilir
    

    public void SetCarried(bool carried)
    {
        //Colliderları kapat aç elimizdeyken çarpışmasın
        foreach (var collider in cols) collider.enabled = !carried;
 
        if (rb) //📌 Null-safe yaklaşım 
        {
            rb.isKinematic = carried; //True olduğunda - Fizik motoru bu objeyi artık yönetmez
            rb.useGravity = !carried;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

        }
    }


    //Bu kod bloğu teker makineye yerleştirildiğinde fiziğini kapatır ve sabitler , makineden  alındığında ise tekrar fiziği açar.
    public void SetPlacedOnMachine(bool placed)
    {
        foreach (var collider in cols) collider.enabled = !placed;

        if(rb)
        {
            rb.isKinematic = placed;
            rb.useGravity = !placed;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
