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
    Rigidbody[] rbs; //rigidboy referans
    Collider[] cols; //collider array list teker,jant, lastik collider olduğu için
    WheelController wheel;  //wheelcontroller sınıfı bu tekeer sökülebilir mi bilgisini WheelController'a bağımlı

    public bool IsBalanced {get; private set;}

    void Awake()
    {
        rbs = GetComponentsInChildren<Rigidbody>();
        cols = GetComponentsInChildren<Collider>(); // Teker modeli genelde çok parçalıdır. Tek collider değil → hepsini kapatman gerekir

        wheel = GetComponent<WheelController>();

    }

    public bool CanPickUp => wheel != null && wheel.IsUnlocked; //Bu teker SADECE bijonlar sökülmüşse alınabilir
    

    public void SetCarried(bool carried)
    {
        // eldeyken herşeyi kapatmak lazım
        if(carried)
        {
            foreach (var c in cols) c.enabled = false;
            foreach (var r in rbs)
            {
                r.isKinematic = true;
                r.useGravity = false;
                r.velocity = Vector3.zero;
                r.angularVelocity = Vector3.zero;
            }
            return;
        }
        // elde değilken:
    // SADECE root fizik açılacak, child fizik kapalı kalacak (assembled varsayımı)
    foreach (var r in rbs)
    {
        bool isRoot = (r.gameObject == gameObject);

        r.isKinematic = !isRoot;
        r.useGravity  = isRoot;

        r.velocity = Vector3.zero;
        r.angularVelocity = Vector3.zero;
    }

    foreach (var c in cols)
    {
        bool isRoot = (c.gameObject == gameObject);
        c.enabled = isRoot;
    }

    // ayrıca rim/tyre SplitPhysicsToggle varsa komple disablela (ek güvenlik)
    foreach (var s in GetComponentsInChildren<SplitPhysicsToggle>(true))
        s.DisableAll();
    }


    //Bu kod bloğu teker makineye yerleştirildiğinde fiziğini kapatır ve sabitler , makineden  alındığında ise tekrar fiziği açar.
    public void SetPlacedOnMachine(bool placed)
    {
        foreach (var collider in cols) collider.enabled = !placed;

        foreach (var rb in rbs)
        {
            rb.isKinematic = placed;
            rb.useGravity = !placed;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void SetBalanced(bool value)
    {
        IsBalanced = value;
    }
}