using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TireVisualFitTarget : MonoBehaviour
{
    [SerializeField] Transform visualChild;

    Vector3 defaultLocalPos;
    Vector3 defaultLocalEuler;
    Vector3 defaultLocalScale;

    public Transform VisualChild => visualChild;

    void Awake()
    {
        if(visualChild == null && transform.childCount > 0)
            visualChild = transform.GetChild(0);

        if(visualChild == null) return;

        defaultLocalPos = visualChild.localPosition;
        defaultLocalEuler = visualChild.localEulerAngles;
        defaultLocalScale = visualChild.localScale;    
    }

    public void ResetVisual()
    {
        if(visualChild == null) return;
        
        visualChild.localPosition = defaultLocalPos;
        visualChild.localRotation = Quaternion.Euler(defaultLocalEuler);
        visualChild.localScale = defaultLocalScale;
    }

    public void ApplyVisualFit(Vector3 posOffset, Vector3 rotOffset, Vector3 scaleMul)
    {
        if(visualChild == null )return;

        visualChild.localPosition = defaultLocalPos + posOffset;
        visualChild.localRotation = Quaternion.Euler(defaultLocalEuler+ rotOffset);
        visualChild.localScale = new Vector3(
            defaultLocalScale.x * scaleMul.x,
            defaultLocalScale.y * scaleMul.y,
            defaultLocalScale.z * scaleMul.z

        );
    }
}
