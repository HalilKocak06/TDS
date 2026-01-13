
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

    WheelCarryable carriedWheel;

    GameObject carriedGeneric;



    void Start()
    {
        if(!cam) cam = Camera.main;
    }

    
    void Update()
    {
         if(!Input.GetKeyDown(KeyCode.E)) return;

        //Lastik janttan ayrıldığında lastiği bırakabilmemizi sağlar . E ile 
         if (carriedGeneric != null)
        {
            DropGeneric();
            return;
        }

         //* 1)Elimde teker varsa bırak:
         if (carriedWheel != null)
         {
            TryPlaceOrDrop();
            return;
         }

        // 2)Elimde teker yoksa al
        TryPickUpWheel();
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


        //Şimdilik sadece Tire objesi için BASİT TAŞIMA!!!
        obj.transform.SetParent(carryPoint, false);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        Debug.Log(" Tire given to player hand");

    }

    void DropGeneric()
    {
        Vector3 dropPos = cam.transform.position + cam.transform.forward * 1.2f;

        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit groundHit, 2f))
            dropPos = groundHit.point + Vector3.up * 0.05f;

        carriedGeneric.transform.SetParent(null, true);
        carriedGeneric.transform.position = dropPos;
        carriedGeneric.transform.rotation = Quaternion.Euler(0f, cam.transform.eulerAngles.y, 0f);

        if(carriedGeneric.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        carriedGeneric = null;
        Debug.Log("SADE LASTIK BAŞARILI BİR ŞEKİLDE YERE BIRAKILDI :::");

    }
}
