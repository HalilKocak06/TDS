using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoltRemover : MonoBehaviour
{
    public enum BoltMode {Remove, Install}
    [Header("Ray(Wrench )")]
    [SerializeField] Transform rayOrigin;
    [SerializeField] float boltDistance = 0.5f; // bijon farkı
    [SerializeField] LayerMask boltLayer; //bijon Layer'i

    [SerializeField] Camera cam; //Hangi kamerayı atarız

    [Header("Tools")]  
    [SerializeField] PlayerToolController toolController; //tool control sınıfı.

    [Header("Lift Gate")]
    [SerializeField] LiftController lift; //Lift sınıfının referansını kullanıyoruz. Ayrıca Player'dan lift yukarıda mı diye sorabilmemizi sağlar ...
    [SerializeField] bool requireLiftUp = true; //Burada lift kalkık mı değil mi sorgulayacağoız. // Bu bir oyun kuralı anahtarı.true → Lift yukarı değilse bijon sökülemez

    [SerializeField] float removeCooldown = 0.25f; // süre

    [Header("Timing")]
    [SerializeField] BoltMode mode = BoltMode.Remove;
    
    
    float t;


    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime; 

        //Mode Switch
        if(Input.GetKeyDown(KeyCode.U))
        {
            mode = BoltMode.Remove;
            Debug.Log("BOLT MODE : REMOVE");
        }

        if(Input.GetKeyDown(KeyCode.I))
        {
            mode = BoltMode.Install;
            Debug.Log("BOLT MODE : INSTALL");
        }

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

        if(rayOrigin == null)
        {
            Debug.LogWarning("BoltRemover: rayOrigin (uç nokta) atanmadı!");
        }

        Vector3 origin = rayOrigin.position;
        Vector3 dir = rayOrigin.forward;

        // Debug.DrawRay(origin, dir * boltDistance, Color.red, 0.2f);

        if (!Physics.Raycast(origin, dir, out RaycastHit hit, boltDistance, boltLayer))
            return;

        var nut = hit.collider.GetComponentInParent<LugNut>();
        if(nut == null) return;
           
        if (mode == BoltMode.Remove)
        {
            nut.Remove();
        }
        else //INSTALL MODE !!
            nut.Install();

        t=0f;    


    }
}
