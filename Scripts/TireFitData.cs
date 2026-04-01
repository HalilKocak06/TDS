using UnityEngine;

[System.Serializable]
public class TireFitData
{
    public string carId;
    public string prefabId;

    public Vector3 positionOffset;
    public Vector3 rotationOffset;
    public Vector3 scaleMultiplier = Vector3.one;
    
}