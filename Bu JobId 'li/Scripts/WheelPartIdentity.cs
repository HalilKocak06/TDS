using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WheelPartKind
{
    WheelRoot = 0,
    Rim = 1,
    Tire = 2
}

public class WheelPartIdentity : MonoBehaviour
{
    [Header("0 = unassigned ( new tire like)")]
    public int jobId = 0; // Hangi aracın lastikleri olduğunu buradan tutacağız.

    public WheelPartKind kind = WheelPartKind.Tire; // bu kesinlikle lastik diyoruz burada
}
