using System.Collections.Generic;
using UnityEngine;

public class CarJobController : MonoBehaviour
{
    [Header("Bütün araç havuzu")]
    [SerializeField] List<CarSpawnData> cars = new List<CarSpawnData>();

    [Header("Arabanın lifte spawn olacağı nokta")]
    [SerializeField] Transform liftParkingSpot;

    // Şu anda sahnede aktif olan araba
    GameObject currentCar;
    public GameObject CurrentCar => currentCar;

    // Şu anda spawn olan arabanın id bilgisi
    string currentCarId;
    public string CurrentCarId => currentCarId;

    [Header("Debug / manuel araç seçimi")]
    [SerializeField] int selectedCarIndex = 0;
    [SerializeField] bool debugKeyboardSelect = true;

    [Header("Debug rim testi")]
    [SerializeField] int debugRim = 16;

    void Update()
    {
        // F -> debug olarak inspector'daki debugRim değerine göre random uygun araba spawn eder
        if (Input.GetKeyDown(KeyCode.F))
        {
            SpawnRandomCarForRim(debugRim);
        }

        // K -> mevcut arabayı kaldır
        if (Input.GetKeyDown(KeyCode.K))
        {
            RemoveCar();
        }

        // İstersen 1-9 ile inspector listesinde manuel seçili aracı değiştir
        // Bu kısım AŞAMA 1 için zorunlu değil ama debug için faydalı
        if (debugKeyboardSelect)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SetSelectedCarIndex(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SetSelectedCarIndex(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SetSelectedCarIndex(2);
            if (Input.GetKeyDown(KeyCode.Alpha4)) SetSelectedCarIndex(3);
            if (Input.GetKeyDown(KeyCode.Alpha5)) SetSelectedCarIndex(4);
            if (Input.GetKeyDown(KeyCode.Alpha6)) SetSelectedCarIndex(5);
            if (Input.GetKeyDown(KeyCode.Alpha7)) SetSelectedCarIndex(6);
            if (Input.GetKeyDown(KeyCode.Alpha8)) SetSelectedCarIndex(7);
            if (Input.GetKeyDown(KeyCode.Alpha9)) SetSelectedCarIndex(8);
        }
    }

    /// <summary>
    /// Debug amaçlı inspector listesinden manuel araç seçer.
    /// Şu an AŞAMA 1'de esas kullanılan sistem bu değil.
    /// Esas sistem rim'e göre random seçim yapan SpawnCarAtLift(int rim).
    /// </summary>
    public void SetSelectedCarIndex(int index)
    {
        if (cars == null || cars.Count == 0)
        {
            Debug.LogWarning("[CarJob] Car list is empty.");
            return;
        }

        if (index < 0 || index >= cars.Count)
        {
            Debug.LogWarning("[CarJob] Invalid car index: " + index);
            return;
        }

        selectedCarIndex = index;
        Debug.Log("[CarJob] Selected car index -> " + selectedCarIndex + " | " + cars[selectedCarIndex].carId);
    }

    /// <summary>
    /// Eski manuel spawn sistemi.
    /// Seçili araç index'ine göre spawn eder.
    /// İstersen test için kullanılabilir.
    /// </summary>
    public GameObject SpawnCarAtLift()
    {
        // Zaten araba varsa tekrar spawn etme
        if (currentCar != null)
            return currentCar;

        if (liftParkingSpot == null)
        {
            Debug.LogError("[CarJob] liftParkingSpot is missing!");
            return null;
        }

        CarSpawnData data = GetSelectedCarPrefab();
        if (data == null || data.prefab == null)
        {
            Debug.LogError("[CarJob] No valid car prefab selected.");
            return null;
        }

        // Arabayı spawn et
        currentCar = Instantiate(
            data.prefab,
            liftParkingSpot.position,
            liftParkingSpot.rotation
        );

        // Lift noktasına child yap
        currentCar.transform.SetParent(liftParkingSpot);

        // Araç için özel local offset/rotation uygula
        currentCar.transform.localPosition = data.localSpawnOffset;
        currentCar.transform.localRotation = Quaternion.Euler(data.localSpawnEulerOffset);

        // Şu anki araç id'sini sakla
        currentCarId = data.carId;

        Debug.Log($"[CarJob] Spawned manually -> {data.carId} | localOffset={data.localSpawnOffset}");

        return currentCar;
    }

    /// <summary>
    /// AŞAMA 1'in ana fonksiyonu:
    /// Verilen rim inch'e göre uygun araçlar bulunur,
    /// içlerinden rastgele biri seçilir ve lifte spawn edilir.
    /// </summary>
    public GameObject SpawnCarAtLift(int rim)
    {
        // Zaten araba varsa aynı arabayı döndür
        if (currentCar != null)
            return currentCar;

        if (liftParkingSpot == null)
        {
            Debug.LogError("[CarJob] liftParkingSpot is missing!");
            return null;
        }

        // Verilen rim'e göre uygun araba seç
        CarSpawnData data = PickRandomCarForRim(rim);
        if (data == null || data.prefab == null)
        {
            Debug.LogWarning($"[CarJob] Rim {rim} için uygun araç bulunamadı!");
            return null;
        }

        // Arabayı spawn et
        currentCar = Instantiate(
            data.prefab,
            liftParkingSpot.position,
            liftParkingSpot.rotation
        );

        // Lift noktasına child yap
        currentCar.transform.SetParent(liftParkingSpot);

        // O araca özel local offset/rotasyonu uygula
        currentCar.transform.localPosition = data.localSpawnOffset;
        currentCar.transform.localRotation = Quaternion.Euler(data.localSpawnEulerOffset);

        // Hangi araba spawn oldu bilgisini sakla
        currentCarId = data.carId;

        Debug.Log($"[CarJob] Random Spawn -> {data.carId} | rim={rim}");

        return currentCar;
    }

    /// <summary>
    /// Debug için okunaklı isimli wrapper.
    /// İçeride yine SpawnCarAtLift(int rim) çağrılır.
    /// </summary>
    public GameObject SpawnRandomCarForRim(int rim)
    {
        return SpawnCarAtLift(rim);
    }

    /// <summary>
    /// Sahnedeki mevcut arabayı yok eder.
    /// </summary>
    public void RemoveCar()
    {
        if (currentCar == null)
            return;

        Destroy(currentCar);
        currentCar = null;
        currentCarId = null;
    }

    /// <summary>
    /// Job bittiğinde çağrılabilir.
    /// RemoveCar ile aynı işi yapar ama isim olarak job akışında daha okunaklıdır.
    /// </summary>
    public void DespawnCar()
    {
        if (currentCar == null)
            return;

        Destroy(currentCar);
        currentCar = null;
        currentCarId = null;

        Debug.Log("[CarJob] Car despawned");
    }

    /// <summary>
    /// AŞAMA 1'in seçim algoritması:
    /// 1) cars listesini dolaşır
    /// 2) rim'i destekleyen araçları filtreler
    /// 3) ağırlığa göre random birini seçer
    /// 
    /// Şimdilik hepsinin weight'i 1 olursa eşit ihtimalli seçim olur.
    /// </summary>
    CarSpawnData PickRandomCarForRim(int rim)
    {
        if (cars == null || cars.Count == 0)
            return null;

        List<CarSpawnData> eligible = new List<CarSpawnData>();
        int totalWeight = 0;

        // Uygun araçları topla
        for (int i = 0; i < cars.Count; i++)
        {
            CarSpawnData car = cars[i];

            if (car == null || car.prefab == null)
                continue;

            // Bu araç bu rim'i desteklemiyorsa geç
            if (!car.SupportsRim(rim))
                continue;

            eligible.Add(car);
            totalWeight += Mathf.Max(1, car.weight);
        }

        // Uygun araç yoksa null
        if (eligible.Count == 0)
            return null;

        // 0 ile totalWeight-1 arasında random sayı
        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;

        // Weight'e göre seçim
        for (int i = 0; i < eligible.Count; i++)
        {
            cumulative += Mathf.Max(1, eligible[i].weight);

            if (roll < cumulative)
                return eligible[i];
        }

        // Güvenlik fallback
        return eligible[eligible.Count - 1];
    }

    /// <summary>
    /// Manuel seçili araç index'inden prefab bilgisi döndürür.
    /// </summary>
    CarSpawnData GetSelectedCarPrefab()
    {
        if (cars == null || cars.Count == 0)
            return null;

        if (selectedCarIndex < 0 || selectedCarIndex >= cars.Count)
            return null;

        return cars[selectedCarIndex];
    }
}