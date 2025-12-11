using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("References")]
    public Transform player;          // FPS karakterinin YERİ - 3rd person olduğu için şart.
    public AudioSource audioSource; // ses çalacağımız componennt(Zaten hali hazırda Door'un içinde)

    [Header("Door Settings")]
    public float openAngle = 90f;     // +90 veya -90
    public float openSpeed = 5f; //Kapının açılma hızı .
    public float interactDistance = 3.5f; //Kapının açılması uzaklığı

    [Header("Sounds")]
    public AudioClip openDoorSound; //açılırken çalacak sesler
    public AudioClip closeDoorSound; //kapanırken çalacak sesler

    bool isOpen = false;
    float closedAngle;   // kapalıyken Açı
    float targetAngle; //gitmek istediğimiz açı

    void Start()
    {
        // Başlangıç açısını "kapalı" kabul et
        closedAngle = transform.localEulerAngles.y; 
        targetAngle = closedAngle;
    }

    void Update()
    {
        if (player == null) return;

        // Oyuncu yeterince yakın mı?
        float dist = Vector3.Distance(player.position, transform.position);
        if (dist <= interactDistance && Input.GetKeyDown(KeyCode.E))
        {
            ToggleDoor();
        }

        // Yumuşak döndürme
        float currentY = transform.localEulerAngles.y;
        float newY = Mathf.LerpAngle(currentY, targetAngle, Time.deltaTime * openSpeed); //LerpAngle = iki açı arasında smooth geçiş.

        Vector3 euler = transform.localEulerAngles;
        euler.y = newY;
        transform.localEulerAngles = euler;
    }

    void ToggleDoor()
    {
        isOpen = !isOpen;
        targetAngle = isOpen ? closedAngle + openAngle : closedAngle; //Eğer kapı açılacaksa +90 değilse 0

        //Sesi çalarız
        if (audioSource != null)
        {
            if (isOpen && openDoorSound != null)
            audioSource.PlayOneShot(openDoorSound);

            if(!isOpen && closeDoorSound != null)
            audioSource.PlayOneShot(closeDoorSound);
        }
    }
}
