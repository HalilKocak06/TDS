using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LugNut : MonoBehaviour
{
    public bool IsRemoved { get; private set;}

    [UnitHeaderInspectable("Visuals")]
    [SerializeField] Renderer[] renderers; //boş 

    void Awake()
    {
        if(renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>(true);

        bool anyVisible = false;
        foreach(var r in renderers)
            if (r != null && r.enabled) { anyVisible = true; break;}

        IsRemoved = !anyVisible;    
    }

    public void Remove()
    {
        if (IsRemoved) return;

        IsRemoved = true;
        SetVisual(false);
    }

    public void Install()
    {
        if(!IsRemoved) return;
        IsRemoved =false;
        SetVisual(true);
    }

    void SetVisual(bool visible)
    {
        if(renderers == null) return;
        foreach (var r in renderers)
            if( r != null) r.enabled = visible;
    }
}
