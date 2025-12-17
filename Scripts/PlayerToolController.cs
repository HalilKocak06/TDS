using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

//Bu kodda asıl amacımız elde gösterme ...

public class PlayerToolController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Transform handPoint;
    [SerializeField] Camera playerCam;
    [Header("Drop Settings")]
    [SerializeField] float dropDistance = 1.2f;  //kameranın önüne bırakma mesafıesi.
    [SerializeField] float dropUpOffset = 0.05f; //zemine gömülmesin

    [Header("Current Tool")]
    GameObject heldToolInstance;
    PickUpItem heldToolPickUpItem; //Başka bir sınıfın objesi
    public bool HasImpactWrench => currentToolType == ToolType.ImpactWrench ;

    public enum ToolType { None, ImpactWrench} 
    
    ToolType currentToolType = ToolType.None;

    void Update()
    {
        // Drop / Return için Input girişi
        if(Input.GetKeyDown(KeyCode.G))
        {
            DropCurrentTool();
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            ReturnCurrentToolToOriginal();
        }
    }


    public void EquipImpactWrench(GameObject toolPrefab)
    {
        //elde var ise sil.
        if (heldToolInstance) return;

    // Sahnedeki objeyi ele alıyoruz (instantiate YOK)
        heldToolInstance = toolObject;
        heldToolInstance.transform.SetParent(handPoint);
        heldToolInstance.transform.localPosition = Vector3.zero;
        heldToolInstance.transform.localRotation = Quaternion.identity;

     // PickUpItem referansı
        heldToolPickUpItem = heldToolInstance.GetComponent<PickUpItem>();

    // Elde iken collider kapat
        foreach (var col in heldToolInstance.GetComponentsInChildren<Collider>())
        col.enabled = false;

    // Rigidbody sabitle
        var rb = heldToolInstance.GetComponentInChildren<Rigidbody>();
        if (rb)
    {
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    currentToolType = ToolType.ImpactWrench;   

    }

    public void DropCurrentTool()
    {
        if (!heldToolInstance) return;

        Transform toolTransform = heldToolInstance.transform;
        toolTransform.SetParent(null);

        //Bırakılacak pozisyon : kameranın önünde yere
        Vector3 dropPos = playerCam.transform.position + playerCam.transform.forward * dropDistance;

        if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out RaycastHit hit , dropDistance + 1.0f ))
        {
            dropPos = hit.point + Vector3.up * dropUpOffset;
        }

        toolTransform.position = dropPos;
        toolTransform.rotation = Quaternion.Euler(0f, playerCam.transform.eulerAngles.y, 0f);

        foreach (var col in heldToolInstance.GetComponentsInChildren<Collider>())
        col.enabled = true;

        var rb = heldToolInstance.GetComponentInChildren<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        //Tool state temizle
        heldToolInstance = null;
        heldToolPickUpItem = null;
        currentToolType = ToolType.None;


    }

    public void ReturnCurrentToolToOriginal()
    {
        if (!heldToolInstance || heldToolPickUpItem == null) return;

        Transform toolTransform = heldToolInstance.transform;
        toolTransform.SetParent(heldToolPickUpItem.originalParent);

        toolTransform.position = heldToolPickUpItem.originalPosition;
        toolTransform.rotation = heldToolPickUpItem.originalRotation;
        toolTransform.localScale = heldToolPickUpItem.originalScale;

        // Collider/rigidbody açık (standda fizik istemiyorsan kapatabilirsin)
        foreach (var col in heldToolInstance.GetComponentsInChildren<Collider>())
            col.enabled = true;

        var rb = heldToolInstance.GetComponentInChildren<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = true;   // standda sabit dursun
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Elden düşürmüş gibi state temizle
        heldToolInstance = null;
        heldToolPickUpItem = null;
        currentToolType = ToolType.None;

    }
}
