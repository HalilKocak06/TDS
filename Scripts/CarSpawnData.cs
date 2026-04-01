using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CarSpawnData
{
    [Header("Arabanın kimliği / debug için")]
    public string carId;

    [Header("Spawn edilecek prefab")]
    public GameObject prefab;

    [Header("Araba lift noktasına child olduktan sonra uygulanacak local offset")]
    public Vector3 localSpawnOffset;

    [Header("Araba lift noktasına child olduktan sonra uygulanacak local rotasyon")]
    public Vector3 localSpawnEulerOffset;

    [Header("Bu araç hangi jant inchlerini destekliyor ?")]
    public List<int> supportedRims = new List<int>();

    [Header("Random seçilme ağırlığı(şimdilik hepsi aynı)")]
    [Min(1)] public int weight = 1;

    /// <summary>
    /// Bu araba verilen rim inch'i destekliyor mu?
    /// Örn: rim=16 geldi, listede 16 varsa true döner.
    /// </summary>
    public bool SupportsRim(int rim)
    {
        if (supportedRims == null || supportedRims.Count == 0)
            return false;

        for (int i = 0; i < supportedRims.Count; i++)
        {
            if (supportedRims[i] == rim)
                return true;
        }

        return false;
    }


}