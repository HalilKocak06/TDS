using UnityEngine;
using Unity.AI.Navigation;

public class DoorNavLinkController : MonoBehaviour
{
    [SerializeField] DoorController door;
    [SerializeField] NavMeshLink link;
    [SerializeField] bool linkEnabledWhenOpen = true;

    void Awake()
    {
        if (!door) door = GetComponentInChildren<DoorController>();
        if (!link) link = GetComponentInChildren<NavMeshLink>();
    }

    void Update()
    {
        if (!door || !link) return;

        bool shouldEnable = linkEnabledWhenOpen ? door.IsOpen : !door.IsOpen;
        if (link.enabled != shouldEnable)
            link.enabled = shouldEnable;
    }
}
