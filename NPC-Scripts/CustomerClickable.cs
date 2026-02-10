using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerClickable : MonoBehaviour
{
    CustomerController customer;

    void Awake()
    {
        customer = GetComponent<CustomerController>();
        if(!customer) customer = GetComponentInParent<CustomerController>();
    }
    
    public bool CanClick()
    {
        return customer != null && customer.state == CustomerController.State.WaitingPlayer;
    }

    public void OnClicked()
    {
        if(!CanClick()) return;

        
        // Şimdilik kabul logu
        Debug.Log("[NPC] Tamam usta, halledelim.");
        
        // Burada bir sonraki adımda Car/Lift tetikleyeceğiz.
        // customer.AcceptRequest(); gibi bir fonksiyon da çağırabiliriz (istersen ekleriz).

        
    }
}
