using UnityEngine;

[System.Serializable]
public class TireVisualAdjustData
{
    public string Id;
    public GameObject prefab;

    [Header("Visual Child Settings")]
    public Vector3 localVisualPosition;
    public Vector3 localVisualEulerRotation;
    public Vector3 localVisualScale = Vector3.one;
    
}
