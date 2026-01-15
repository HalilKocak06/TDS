
using Unity.VisualScripting;
using UnityEngine;

public class PlayerWheelCarrier : MonoBehaviour
{
    [SerializeField] Camera cam; //ana kamerayı yerleştiriyoruz.
    [SerializeField] Transform carryPoint; //BU handpoint'i ekliyoruz ki buraya kamerayı ayarlasın diye.

    [Header("Raycast")]
    [SerializeField] float interactDistance = 5f; //Etkileşim mesafesi
    [SerializeField] LayerMask wheelLayer; //Teker layeri
    [SerializeField] LayerMask placeLayer; //
    [SerializeField] Vector3 carryEulerOffset = new Vector3(0f, 250f, 0f); //*

    [SerializeField] LayerMask genericLayer;

    WheelCarryable carriedWheel;

    GameObject carriedGeneric;



    void Start()
    {
        if(!cam) cam = Camera.main;
    }

    
    void Update()
    {
         if (Input.GetKeyDown(KeyCode.G))
        {
            if(carriedGeneric != null)
            {
                DropGeneric();
                return;
            }

        }

        //E ile al / yerleştir
        if(!Input.GetKeyDown(KeyCode.E)) return;

        if(carriedGeneric != null)
        {
            TryPlaceRimOnMachine();
            return;
        }

        //Elimde WHEEL VARSA DAVRANIŞŞ
        if(carriedWheel != null)
        {
            TryPlaceOrDrop();
            return;
        }

        TryPickUpWheel();
        if(carriedWheel == null)
        {
            //önce makinden rim almaya çalışacağız.
            TryPickUpRimFromMachine();

            // hala hiçbir şey almadıysak yerden generic dene
            if(carriedGeneric == null)
            {
                TryPickUpGeneric();
            }
            
        }
    }

    void TryPickUpGeneric()
    {
        //Generic layer'I burada raycast edeceğiz.
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, interactDistance, genericLayer))
        {
             // collider child’da olabilir → parent objeyi al
        var go = hit.collider.GetComponentInParent<Transform>()?.gameObject;
        if (go == null) return;

        carriedGeneric = go;

        // eldeyken fizik kapat (sürünme biter)
        if (carriedGeneric.TryGetComponent<GenericCarryable>(out var gc))
            gc.SetCarried(true);
        else if (carriedGeneric.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        carriedGeneric.transform.SetParent(carryPoint, false);
        carriedGeneric.transform.localPosition = Vector3.zero;
        carriedGeneric.transform.localRotation = Quaternion.identity;

        Debug.Log("Generic picked up");

        }
    }

    void TryPickUpWheel()
    {
         if(Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, interactDistance, wheelLayer))
        {
            var wheel = hit.collider.GetComponentInParent<WheelCarryable>();
            if (wheel == null) return;
            if (!wheel.CanPickUp) return; //CanPickUp eğer sökülmemişsize bijonlar dön.

            carriedWheel = wheel;
            carriedWheel.SetCarried(true);

            var t = carriedWheel.transform;
            t.SetParent(carryPoint);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.Euler(carryEulerOffset);

            Debug.Log("Wheel Picked Up!!!!!! ---log message---");
        }
    }

    void TryPlaceOrDrop()
    {
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit , interactDistance, placeLayer))
        {
            var place = hit.collider.GetComponentInParent<WheelPlacePoint>();
            if(place == null || place.slotPoint == null)
            {
                Debug.LogWarning("WheelPlacePoint veya SlotPoint yok!");
            }

            //SlotPoint'e  bağla koy.
            carriedWheel.transform.SetParent(place.slotPoint);
            carriedWheel.transform.localPosition = Vector3.zero;
            carriedWheel.transform.localRotation = Quaternion.identity;

            //Makinede sabitle
            carriedWheel.SetPlacedOnMachine(true); //Yani burada bu fonksiyona giderek(WheelCarrable.cs)'e tekerin fiziğini kapatırız.

            var machine = place.GetComponentInParent<TireChangerMachineController>();
            if(machine != null)
            {
                machine.RegisterWheelAlreadyPlaced(carriedWheel);
                Debug.Log("Machine notified: wheel registered.");
            }
            carriedWheel = null;


            Debug.Log("Wheel placed on machine");
            return;
        }

        //Yoksa yere bırak: kameranın önüne düşecek.
        Vector3 dropPos = cam.transform.position + cam.transform.forward *1.2f;
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit groundHit, 2f))
            dropPos = groundHit.point + Vector3.up * 0.05f;

        carriedWheel.transform.SetParent(null);
        carriedWheel.transform.position = dropPos;
        carriedWheel.transform.rotation = Quaternion.Euler(0f, cam.transform.eulerAngles.y, 0f);

        carriedWheel.SetCarried(false);
        carriedWheel= null;

        Debug.Log("Wheel Dropped");    
    }

    public void ForcePickUpExternalObject(GameObject obj)
    {
        

        carriedGeneric = obj; 

        // eldeyken fizik kapat (sürünme biter)
        if (carriedGeneric.TryGetComponent<GenericCarryable>(out var gc))
            gc.SetCarried(true);
        else if (carriedGeneric.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        carriedGeneric.transform.SetParent(carryPoint, false);
        carriedGeneric.transform.localPosition = Vector3.zero;
        carriedGeneric.transform.localRotation = Quaternion.identity;

        Debug.Log("Tire given to player hand");

    }

    void DropGeneric()
    {
        Vector3 dropPos = cam.transform.position + cam.transform.forward * 1.2f;

        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit groundHit, 2f))
            dropPos = groundHit.point + Vector3.up * 0.1f;

        carriedGeneric.transform.SetParent(null, true);
        carriedGeneric.transform.position = dropPos;
        bool isRim = carriedGeneric.name.ToLower().Contains("rim");
        if(isRim)
        {
            carriedGeneric.transform.rotation = Quaternion.Euler(-90f, cam.transform.eulerAngles.y, 0f);
        }
        else
        {
            carriedGeneric.transform.rotation = Quaternion.Euler(0f, cam.transform.eulerAngles.y, 0f);
        }




        if (carriedGeneric.TryGetComponent<GenericCarryable>(out var gc))
        {
            gc.SetCarried(false); // ✅ collider + rb geri açılır
        }
        else if (carriedGeneric.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        carriedGeneric = null;
        Debug.Log("SADE LASTIK BAŞARILI BİR ŞEKİLDE YERE BIRAKILDI :::");

    }

    void TryPlaceRimOnMachine()
    {
        if(carriedGeneric == null) return;

        bool isRim = carriedGeneric.name.ToLower().Contains("rim");
        if(!isRim) return; // sadece Rim

        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, interactDistance, placeLayer))
        {
            var machine = hit.collider.GetComponentInParent<TireChangerMachineController>();
            if (machine == null) return;

            Transform target = machine.GetRimStayPoint();
            if(target == null)
            {
                Debug.LogWarning("Makinede rimStayPoint yok !!!");
                return;
            }

            //Rim'i hedefe koy
            carriedGeneric.transform.SetParent(target,true);
            carriedGeneric.transform.SetPositionAndRotation(target.position, target.rotation);

            //makinede sabit kalsın (elde değil !)
            if(carriedGeneric.TryGetComponent<GenericCarryable>(out var gc))
                gc.SetCarried(true); //colider kapat + kinematic (makinede sabit)

            carriedGeneric = null;
            Debug.Log("Rim placed on machine (rimStayPoint).");    


        }
    }

    void TryPickUpRimFromMachine()
    {
        //El boş olmalı
        if(carriedGeneric != null || carriedWheel != null ) return;

        //Makineye bakıyor muyuz diye kontrol ediyoruz
        if(!Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, interactDistance, placeLayer))
        return;

        var machine = hit.collider.GetComponentInParent<TireChangerMachineController>();
        if(machine == null) return;

        Transform rimPoint = machine.GetRimStayPoint();
        if(rimPoint == null) return ; 

        //RimStayPoint altında rim var mı ?
        if (rimPoint.childCount == 0) return;

        // İlk çocuğu rim kabul ediyoruz (istersen isimle kontrol ekleriz)
        GameObject rimObj = rimPoint.GetChild(0).gameObject;

        // İstersen garanti: adı rim mi?
        if (!rimObj.name.ToLower().Contains("rim"))
            return;

        carriedGeneric = rimObj;

        // Makineden ayır
        carriedGeneric.transform.SetParent(null, true);

        // Ele alınca fizik kapat (senin sistem)
        if (carriedGeneric.TryGetComponent<GenericCarryable>(out var gc))
            gc.SetCarried(true);
        else if (carriedGeneric.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Eldeki noktaya al
        carriedGeneric.transform.SetParent(carryPoint, false);
        carriedGeneric.transform.localPosition = Vector3.zero;
        carriedGeneric.transform.localRotation = Quaternion.identity;

        Debug.Log("Rim picked up from machine.");

        }
}
