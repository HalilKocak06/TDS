using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoltRemover : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] float boltDistance = 2.5f;
    [SerializeField] LayerMask boltLayer;
    [SerializeField] PlayerToolController toolController;

    [SerializeField] float removeCooldown = 0.25f;
    float t;


    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime;

        if (!Input.GetMouseButtonDown(0)) return; //SOl click
        if (t < removeCooldown) return;
        if(!toolController.HasImpactWrench) return; //elinde makine yoksa yok

        if (Physics.Raycast(cam.transform.position, cam.transform.forward,
        out RaycastHit hit, boltDistance, boltLayer))
        {
            var nut = hit.collider.GetComponentInParent<LugNut>();
            if(nut != null && !nut.IsRemoved)
            {
                nut.Remove();
                t = 0f;
            }
        }


    }
}
