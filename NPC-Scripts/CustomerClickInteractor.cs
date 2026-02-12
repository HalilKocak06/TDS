using UnityEngine;

public class CustomerClickInteractor : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] float maxDistance = 6f;

    [Header("Layer filter (optional)")]
    [SerializeField] bool useLayerMask = false; // ✅ şimdilik kapalı (debug için)
    [SerializeField] LayerMask customerLayer;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        Debug.Log("[Click] Awake OK. Cam=" + (cam ? cam.name : "NULL"));
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryClickCustomer();
        }
    }

    void TryClickCustomer()
    {
        if (!cam) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.yellow, 1f);

        RaycastHit hit;
        bool didHit = useLayerMask
            ? Physics.Raycast(ray, out hit, maxDistance, customerLayer)
            : Physics.Raycast(ray, out hit, maxDistance);

        if (!didHit)
        {
            Debug.Log("[Click] Raycast: NO HIT (nothing under crosshair/mouse)");
            return;
        }

        Debug.Log($"[Click] Raycast HIT: {hit.collider.name}  layer={LayerMask.LayerToName(hit.collider.gameObject.layer)}  dist={hit.distance:0.00}");

        var customer = hit.collider.GetComponentInParent<CustomerController>();
        if (!customer)
        {
            Debug.Log("[Click] HIT object has NO CustomerController in parents.");
            return;
        }

        Debug.Log("[Click] CustomerController FOUND -> calling OnPlayerGreetClicked()");
        customer.OnPlayerGreetClicked();
    }
}
