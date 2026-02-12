using UnityEngine;

// TOOL ELDE TUTMA / BIRAKMA
public class PlayerToolController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Transform handPoint; //Burada handPoint objesiini direk atıyoruz oraya .
    [SerializeField] Camera playerCam; // player kamerasın ıatıyoruız

    [Header("Drop Settings")]
    [SerializeField] float dropDistance = 1.2f; //bırakma uzaklığı 
    [SerializeField] float dropUpOffset = 0.05f; //offseti 

    GameObject heldTool; //Şu an elimde tuttuğum tool objesi
    PickUpItem heldPickUpItem; //Eldeki tool'un PicUpItem component'i

    public bool HasImpactWrench => heldTool != null; //Başka scriptler “elde tool var mı?” kontrolünü heldTool’a direkt bakmadan yapsın diye. Şu an tek tool olduğundan heldTool != null yeterli.


    void Awake() // Unityde component aktif olur olmaz çalışır.
    {
        if (!playerCam) playerCam = Camera.main; //Eğer kamera yoksa hemen kamerayı atar.
    }

    void Update() //Her frame çalışır.
    {
        if (Input.GetKeyDown(KeyCode.G)) //G'ye basıldığında yere bırakır.
        {
            DropTool();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReturnToolToOriginal();
        }
    }

    // 🔥 ASIL KISIM — SAHNEDEKİ OBJENİN KENDİSİNİ ELİNE ALIYOR
    public void EquipImpactWrench(GameObject toolObject) //Fonksiyonun gereksinimleri game obje
    {
        if (heldTool != null) return; //Eğer elde hiçbir şey yok ise returnluyoruz.

        heldTool = toolObject; //toolObject heldTool 'a aktarıyoruz
        heldPickUpItem = heldTool.GetComponent<PickUpItem>(); //heldTool'un componentini aktarıyoruz...

        heldTool.transform.SetParent(handPoint); //Burada heldTool'u handPoint'in childi yapıyoruz ki beraber gezebilsin.
        heldTool.transform.localPosition = Vector3.zero; //parente göre konum alıyoru.
        heldTool.transform.localRotation = Quaternion.identity; //HandPoint’in rotation’ı neyse onu “default” kabul et. Tool’u onunla hizala.

        foreach (Collider col in heldTool.GetComponentsInChildren<Collider>())
            col.enabled = false; //heldTool'un kendisi ve tüm childrenların collider'larını kapatıyoruz ki Player ile çarpışmasın.

        Rigidbody rb = heldTool.GetComponentInChildren<Rigidbody>(); //Rigidbody'i düzenlememizi sağlar ve rb ile.
        if (rb)
        {
            rb.isKinematic = true; //Rigidbody fizik simülasyonundan çıkar.
            rb.useGravity = false; //yerçekimini kapatıyoruz.
            rb.velocity = Vector3.zero; // anlık hız sıfır
            rb.angularVelocity = Vector3.zero; // dönme hızı sıfır.
        }
    }

    // 🔽 YERE BIRAK
    void DropTool()
    {
        if (heldTool == null) return; //yoksa direk dön

        heldTool.transform.SetParent(null); //Parenti kaldırıyoruz artık world root'ta 

        Vector3 dropPos = playerCam.transform.position + playerCam.transform.forward * dropDistance; //kameranın dünya konumu + kameranın baktığı yön 

        if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward,
            out RaycastHit hit, dropDistance + 1f))//Raycast: bir ışın at bir şeye çarpar mı bak !
            //RaycastHit hit eğer çarparsa bilgileri hit'e yaz.
            //Kamera önünde duvar varsa tool’u duvarın içine spawn etmek istemiyorsun. “Çarptığın yerde bırak”.
        {
            dropPos = hit.point + Vector3.up * dropUpOffset;
            //hit.point : ışınının çarptığı dünya noktası
            //zemin objeye yapışık değil biraz üstte dursun.
        }

        heldTool.transform.position = dropPos; //Tool’u world’de o pozisyona koy.
        heldTool.transform.rotation = Quaternion.Euler(0f, playerCam.transform.eulerAngles.y, 0f); //Tool yere bırakılınca kabaca kameranın baktığı yöne dönük dursun (daha doğal).

        foreach (Collider col in heldTool.GetComponentsInChildren<Collider>())
            col.enabled = true; //yerdeyken collider'ı aç

        Rigidbody rb = heldTool.GetComponentInChildren<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = false;
            rb.useGravity = true; //grafvity aç
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        heldTool = null; //ele alınan objeyi sıfırlıyoruzz
        heldPickUpItem = null;
    }

    void ReturnToolToOriginal()
    {
        if (heldTool == null || heldPickUpItem == null) return;

        heldTool.transform.SetParent(heldPickUpItem.originalParent);
        heldTool.transform.position = heldPickUpItem.originalPosition;
        heldTool.transform.rotation = heldPickUpItem.originalRotation;

        EnablePhysics(standMode: true);

        heldTool = null;
        heldPickUpItem = null;
    }

    void EnablePhysics(bool standMode = false)
    {
        foreach (Collider col in heldTool.GetComponentsInChildren<Collider>())
            col.enabled = true;

        Rigidbody rb = heldTool.GetComponentInChildren<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = standMode;   // standda sabit
            rb.useGravity = !standMode;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
    
    }
