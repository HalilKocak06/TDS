using System.Collections;
using System.Collections.Generic;
using DoorScript;
using UnityEngine;
using UnityEngine.AI;

public class DoorAutoOpenTrigger : MonoBehaviour
{
    [SerializeField] DoorController door;
    [SerializeField] float closeDelay = 0.3f;

    int insideCount = 0;
    float closeAt = -1f;

    void Awake()
    {
        if(!door) door = GetComponentInParent<DoorController>();
    }

    bool IsNpc(Collider other)
    {
        // En sağlam ayırt etme: üst parent’ta NavMeshAgent var mı?
        // (Player’da genelde CharacterController var, NavMeshAgent yok)
        return other.GetComponentInParent<NavMeshAgent>() != null;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsNpc(other)) return ; //Yani NPC değilse dön.

        insideCount++;
        closeAt = -1f;

        door?.Open();
    }

    void OnTriggerExit(Collider other)
    {
        if(!IsNpc(other)) return;

        insideCount = Mathf.Max(0, insideCount-1);

        if(insideCount == 0)
            closeAt = Time.time + closeDelay;
    }

    // Update is called once per frame
    void Update()
    {

        if(closeAt > 0f && Time.time >= closeAt)
        {
            closeAt = -1f;
            door?.Close();
        }
        
    }
}
