using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//YERDEN ALMA RAYCAST "E" ile 

public class PlayerInteractor : MonoBehaviour
{

    [SerializeField] Camera cam;
    [SerializeField] float interactDistance = 2.5f;
    [SerializeField] LayerMask interactLayer; //Sadece pickup layer

    [SerializeField] PlayerToolController toolController; //Bu başka scriptin objesi.
    [SerializeField] GameObject impactWrenchPrefab; // Elinde gözükücek Bijon Prefabı.




    // Update is called once per frame
    void Update()
    {
        if(!Input.GetKeyDown(KeyCode.E)) return;

        if(Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E basıldı");
        }

        if(Physics.Raycast(cam.transform.position, cam.transform.forward,
        out RaycastHit hit, interactDistance, interactLayer))
        {
            var pickup = hit.collider.GetComponentInParent<PickUpItem>();
            if(pickup && pickup.itemType == PickUpItem.ItemType.ImpactWrench)
            {
                toolController.EquipImpactWrench(impactWrenchPrefab);
                // Destroy(pickup.gameObject); // yerden sil.
            }
        }
    }
}
