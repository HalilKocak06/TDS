using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BalanceMachineController : MonoBehaviour
{
    [Header("Points")]
    [SerializeField] Transform balanceWheelPoint;

    [Header("Work")]
    [SerializeField] float balanceTime = 5f;
    
    public bool IsWorking {get; private set;}

    WheelCarryable currentWheel;

    public bool TryAcceptWheel(WheelCarryable wheel)
    {
        if(IsWorking) return false;
        if(wheel == null) return false;
        if(currentWheel != null) return false; 
        
        if(balanceWheelPoint == null)
        {
            Debug.LogWarning("BalanceMachine : balanceWheelPoint atanmamış");
            return false;
        }

        currentWheel = wheel; //Teker objesi

        currentWheel.SetPlacedOnMachine(true);

        ParentAndSnapKeepWorld(currentWheel.transform, balanceWheelPoint);

        Debug.Log("BalanceMachine: wheel Accepted & snapped");

        return true;
    }

    public void TryStartBalancing()
    {
        if(IsWorking) return;
        if(currentWheel == null) return;

        StartCoroutine(BalanceRoutine());
    }

    IEnumerator BalanceRoutine()
    {
        IsWorking = true;

        //TODO animasyon veya SESS
        yield return new WaitForSeconds(balanceTime);

        //Wheel'in balancının olup olmadığını karar veriyoruz
        currentWheel.SetBalanced(true);
        IsWorking = false;

        Debug.Log("Balance finished!");
    }

    public WheelCarryable TryReleaseWheel()
    {
        if (IsWorking ) return null;

        var wheel = currentWheel;
        currentWheel = null;
        return wheel;
    }

    static void ParentAndSnapKeepWorld(Transform child, Transform parent)
    {
        Vector3 worldPos = child.position;
        Quaternion worldRot = child.rotation;
        Vector3 worldScale = child.lossyScale;
        

        child.SetParent(parent, true);
        child.SetPositionAndRotation(parent.position, parent.rotation);

        Vector3 parentLossy = parent.lossyScale;
        child.localScale = new Vector3(
            worldScale.x / parentLossy.x,
            worldScale.y / parentLossy.y,
            worldScale.z / parentLossy.z

        );
    }

}
