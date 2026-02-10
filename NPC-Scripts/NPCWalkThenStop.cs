using System.Collections;
using UnityEngine;

public class NPCWalkThenStop : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] float walkDuration = 20.0f;

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        StartCoroutine(WalkThenStop());
    }

    IEnumerator WalkThenStop()
    {
        // yürü
        animator.SetFloat("Speed", 1f);

        yield return new WaitForSeconds(walkDuration);

        // dur (idle)
        animator.SetFloat("Speed", 0f);
    }
}
