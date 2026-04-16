using UnityEngine;
using System.Collections;


public class CustomerClickInteractor : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] float maxDistance = 6f;

    [Header("Layer filter (optional)")]
    [SerializeField] bool useLayerMask = false; // ✅ şimdilik kapalı (debug için)
    [SerializeField] LayerMask customerLayer;
    [SerializeField] private DialogSystemController dialogUI;
    [SerializeField] DialogueUIController dialogUIController;
    [SerializeField] private DialogueRuntimeService dialogueRuntimeService;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        Debug.Log("[Click] Awake OK. Cam=" + (cam ? cam.name : "NULL"));
    }

    void Update()
    {
        if(dialogUI != null && dialogUI.IsOpen)
             return;


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
        // customer.OnPlayerGreetClicked();
        // dialogUI.Show();
        if(!customer.CanStartDialogue())
        {
            Debug.Log($"[Click] Dialogue blocked for {customer.name}");
            return;
        }
        
        // customer.StartDialogue();
        StartRuntimeDialogueForCustomer(customer);
        
        
    }

    private void StartRuntimeDialogueForCustomer(CustomerController customer)
{
    Debug.Log("[Click] A");

    if (customer == null)
    {
        Debug.LogWarning("[Click] B customer null");
        return;
    }

    Debug.Log("[Click] C");

    if (dialogueRuntimeService == null)
    {
        Debug.LogWarning("[Click] D dialogueRuntimeService null");
        return;
    }

    Debug.Log("[Click] E before HasDialogueContext");

    bool hasContext = customer.HasDialogueContext();
    Debug.Log("[Click] F hasContext = " + hasContext);

    if (!hasContext)
    {
        Debug.Log("[Click] G before GetPendingOrder");

        TireOrder order = customer.GetPendingOrder();

        Debug.Log("[Click] H after GetPendingOrder");

        if (order == null)
        {
            Debug.LogWarning("[Click] I Customer has no pending order");
            return;
        }

        Debug.Log($"[Click] J Pending order found -> {order.brand} {order.size.width}/{order.size.aspect}R{order.size.rim} {order.season} x{order.quantity}");

        CustomerDialogueContext context = dialogueRuntimeService.BuildContext(order);

        Debug.Log("[Click] K after BuildContext");

        if (context == null)
        {
            Debug.LogWarning("[Click] L Failed to build dialogue context");
            return;
        }

        customer.SetDialogueContext(context);
        Debug.Log($"[Click] M Dialogue context assigned -> type={context.customerType}, flow={context.flow.flowId}");
    }

    Debug.Log("[Click] N before StartDialogue");
    dialogueRuntimeService.StartDialogue(customer.GetDialogueContext());
    Debug.Log("[Click] O after StartDialogue");
}
}
