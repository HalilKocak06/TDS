using System.Collections;
using System.Collections.Generic;
// using System.Numerics;
using JetBrains.Annotations;
using UnityEngine;

public class PickUpItem : MonoBehaviour
{
    // public enum ItemType { ImpactWrench}
    // BUNLAR ÇALIŞIYOR.
    // public ItemType itemType = ItemType.ImpactWrench;

    public enum ItemType { ImpactWrench}
    public ItemType itemType;

    [HideInInspector] public Transform originalParent;
    [HideInInspector] public Vector3 originalPosition;
    [HideInInspector] public Quaternion originalRotation;
    [HideInInspector] public Vector3 originalScale;

    void Awake()
    {
        //ilk sahneye geldiği anki konumunu kaydet.
        originalParent = transform.parent;
        originalRotation= transform.rotation;
        originalPosition = transform.position;
        originalScale = transform.localScale;
    }
    
}
