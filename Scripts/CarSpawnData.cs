using UnityEngine;

[System.Serializable]
public class CarSpawnData
{
    public string carId;
    public GameObject prefab;
    public Vector3 localSpawnOffset;
    public Vector3 localSpawnEulerOffset;
}