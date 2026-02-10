using UnityEngine;

public class PlayerCustomerClickInteractor : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Camera cam;

    [Header("Click Settings")]
    [SerializeField] float distance = 3f;
    [SerializeField] bool requireCenterAim = true;
    [SerializeField] float dotThreshold = 0.985f; // merkez nişan hassasiyeti
    [SerializeField] LayerMask NPC;

    [Header("Optional Debug")]
    [SerializeField] bool drawRay = false;

    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    void Update()
    {
        if (drawRay && cam)
            Debug.DrawRay(cam.transform.position, cam.transform.forward * distance, Color.red);

        if (!cam) return;

        if (Input.GetMouseButtonDown(0)) // ✅ Sol tık
        {
            TryClickCustomer();
        }
    }

    void TryClickCustomer()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, distance, NPC, QueryTriggerInteraction.Ignore))
        {
            Debug.Log("[CLICK] Ray hiçbir şeye çarpmadı");
            return;
        }

        Debug.Log($"[CLICK] Hit: {hit.collider.name} layer={LayerMask.LayerToName(hit.collider.gameObject.layer)} dist={hit.distance:F2}");

        var clickable = hit.collider.GetComponentInParent<CustomerClickable>();
        if (clickable == null) return;

        if (!clickable.CanClick()) return;
        clickable.OnClicked();
    }

    
}


