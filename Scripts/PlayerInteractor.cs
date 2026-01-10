using UnityEngine;

// YERDEN ALMA RAYCAST "E" ile 
public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] float interactDistance = 2.5f;
    [SerializeField] LayerMask interactLayer; // Pickup layer

    [SerializeField] PlayerToolController toolController;

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward,
            out RaycastHit hit, interactDistance, interactLayer))
        {
            PickUpItem pickup = hit.collider.GetComponentInParent<PickUpItem>();

            if (pickup != null && pickup.itemType == PickUpItem.ItemType.ImpactWrench)
            {
                toolController.EquipImpactWrench(pickup.gameObject);
            }
        }
    }
}
