using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerControlBoxInteractor : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] float interactDistance = 2.5f;
    [SerializeField] LayerMask controlBoxLayer;

    void Start()
    {
        if(!cam) cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if(!Input.GetKeyDown(KeyCode.E)) return;

        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, interactDistance, controlBoxLayer))
        {
            var box = hit.collider.GetComponentInParent<BoxController>();
            if( box != null)
            {
                box.Interact();
            }
        }
    }
}
