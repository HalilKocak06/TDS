using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LugNut : MonoBehaviour
{
    public bool IsRemoved { get; private set;}

    public void Remove()
    {
        if (IsRemoved) return;

        IsRemoved = true;
        //Görseli kaldır...
        gameObject.SetActive(false);
    }
}
