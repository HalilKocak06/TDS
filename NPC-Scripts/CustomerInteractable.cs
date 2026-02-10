using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerInteractable : MonoBehaviour
{
    CustomerController customer;

    void Awake()
    {
        customer = GetComponent<CustomerController>();
        if(!customer) customer = GetComponentInParent<CustomerController>();
    }

    public bool CanInteract()
    {
        return customer != null && customer.CanAccept();
    }

    public void Interact()
    {
        if(customer == null) return;
        customer.AcceptRequest();
    }
}
