using System.Collections;
using System.Collections.Generic;
using DoorScript;
using UnityEngine;
using UnityEngine.AI; //NavmeshAgent tipine bakmak için.

public class DoorAutoOpenTrigger : MonoBehaviour
{
    [SerializeField] DoorController door; //bu script objesi.
    [SerializeField] float closeDelay = 0.3f; //kapanma gecikmesi.

    int insideCount = 0; //trigger içinde kaç npc var sayacı . 2 npc aynı anda girerse kapı kapanmasın.
    float closeAt = -1f; //plansız kapanış

    void Awake()
    {
        if(!door) door = GetComponentInParent<DoorController>(); //eğer door objesi bulamadıysa sccripti bulur.
    }

    bool IsNpc(Collider other)
    {
        // En sağlam ayırt etme: üst parent’ta NavMeshAgent var mı?
        // (Player’da genelde CharacterController var, NavMeshAgent yok)
        return other.GetComponentInParent<NavMeshAgent>() != null; 
    }

    void OnTriggerEnter(Collider other)
    {
        //Buradaki DIAMOND BURASI - yani isTrigger açık olduğu için triggerlandığnıda kapıyı açılıyor !.
        
        if (!IsNpc(other)) return ; //Yani NPC değilse dön.

        insideCount++; //trigger olduğu için arttırırz.
        closeAt = -1f;

        door?.Open(); //kapıyı aç.
    }

    void OnTriggerExit(Collider other)
    {
        if(!IsNpc(other)) return; //NPC yoksa dön

        insideCount = Mathf.Max(0, insideCount-1); // insideCount azaltılır çünkü NPC çıkıyor.

        if(insideCount == 0)
            closeAt = Time.time + closeDelay;
    }

    // Update is called once per frame
    void Update()
    {

        if(closeAt > 0f && Time.time >= closeAt)
        {
            closeAt = -1f; //Planlanmış kapanma zamanı geldiyse kapat .
            door?.Close();
        }
        
    }
}
