using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoltRemover : MonoBehaviour
{
    [SerializeField] Camera cam; //Hangi kamerayı atarız
    [SerializeField] float boltDistance = 5.5f; // bijon farkı
    [SerializeField] LayerMask boltLayer; //bijon Layer'i
    [SerializeField] PlayerToolController toolController; //tool control sınıfı.

    [Header("Lift Gate")]
    [SerializeField] LiftController lift; //Lift sınıfının referansını kullanıyoruz. Ayrıca Player'dan lift yukarıda mı diye sorabilmemizi sağlar ...
    [SerializeField] bool requireLiftUp = true; //Burada lift kalkık mı değil mi sorgulayacağoız. // Bu bir oyun kuralı anahtarı.true → Lift yukarı değilse bijon sökülemez

    [SerializeField] float removeCooldown = 0.25f; // süre
    float t;


    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime; 

        if (!Input.GetMouseButtonDown(0)) return;  //SOl click
        
        // Debug.Log("Click. HasImpactWrench=" + toolController.HasImpactWrench);

        if (t < removeCooldown) return;

        if(!toolController.HasImpactWrench) return; //elinde makine yoksa yok

        if(requireLiftUp)
        {
            if (lift == null)
            {
                Debug.LogWarning("BoltRemover: LiftController referansı yok !");
                return;
            }

            if(!lift.isUp) return; //Eğer lift aşağıdaysa returnluyoruz.
        }

        Debug.DrawRay(cam.transform.position, cam.transform.forward * boltDistance, Color.red, 0.2f);

        if (Physics.Raycast(cam.transform.position, cam.transform.forward,
        out RaycastHit hit, boltDistance, boltLayer))
        {
            // Debug.Log("HIT (maskesiz): " + hit.collider.name + " layer=" + LayerMask.LayerToName(hit.collider.gameObject.layer));
            

            var nut = hit.collider.GetComponentInParent<LugNut>();
            if(nut != null && !nut.IsRemoved)
            {
                nut.Remove();
                t = 0f;
            }
        }
        else
            {
            // Debug.Log("HIT NOTHING (maskesiz)");
            }


    }
}
