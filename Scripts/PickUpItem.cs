using UnityEngine;

public class PickUpItem : MonoBehaviour
{
    // Bu obje hangi tur item olarak sayilacak?
    public enum ItemType { ImpactWrench } //Bu pickup hangi tip ve şu an sadece 1 tip var oda ImpactWrench(Bİjon tabancası yani)
    //Neden enum? Çünkü string ile ("ImpactWrench") uğraşınca typo riski var. Enum daha güvenli, daha hızlı, refactor’a dayanıklı
    // Inspector'dan secilen item tipi
    public ItemType itemType = ItemType.ImpactWrench; //itemType 'ı tanımlıyoruz (Bijon makinesi diye.)

    // Item'i birakirken geri donmek icin ilk halini saklariz
    [HideInInspector] public Transform originalParent; //Transform : Unity’de konum/dönüş/scale taşıyan component türü.
    [HideInInspector] public Vector3 originalPosition; //Vector3 (x,y,z)
    [HideInInspector] public Quaternion originalRotation; //Unity’de dönüş (rotation) temsilidir.

    void Awake()
    {
        // Sahnedeki ilk halini kaydet (bırakma/geri koyma icin)
        originalParent = transform.parent;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }
}
