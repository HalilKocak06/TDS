using UnityEngine;
using Unity.AI.Navigation;

[ExecuteAlways]
public class DoorNavMeshLinkBinder : MonoBehaviour
{
    [SerializeField] NavMeshLink link;
    [SerializeField] Transform startWorld; // DoorLink_Outside
    [SerializeField] Transform endWorld;   // DoorLink_Inside

    void OnEnable()
    {
        if (!link) link = GetComponent<NavMeshLink>();
        if (!startWorld) startWorld = transform;
    }

    void Update()
    {
        if (!link || !startWorld || !endWorld) return;

        // NavMeshLink start/end point local space istiyor
        link.startPoint = transform.InverseTransformPoint(startWorld.position);
        link.endPoint   = transform.InverseTransformPoint(endWorld.position);
    }
}
