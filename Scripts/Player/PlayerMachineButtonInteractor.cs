using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMachineButtonInteractor : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] float interactDistance = 3f;
    [SerializeField] LayerMask buttonLayer;

    void Start()
    {
        if(!cam) cam = Camera.main;   
    }

    void Update()
    {
        bool pressed = Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0);
        if(!pressed) return;

                Debug.Log("Input pressed (E or LMB). Raycasting... dist=" + interactDistance);

        
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, interactDistance, buttonLayer))
        {
            Debug.Log("BUTTON HIT: " + hit.collider.name +
                      " layer=" + LayerMask.LayerToName(hit.collider.gameObject.layer));
                      
            var btn = hit.collider.GetComponentInParent<MachineStartButton>();
            if(btn != null) btn.Press();
        }
    }
}
