using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerProfileLibrary : MonoBehaviour
{
    [Header("Profiles")]
    [SerializeField] private CustomerProfileSO premium;
    [SerializeField] private CustomerProfileSO standard;
    [SerializeField] private CustomerProfileSO priceOnly;
    [SerializeField] private CustomerProfileSO cheap;
    [SerializeField] private CustomerProfileSO referral;

    public CustomerProfileSO GetProfile(CustomerType type)
    {
        switch(type)
        {
            case CustomerType.Premium:  return premium;
            case CustomerType.Standard: return standard;
            case CustomerType.PriceOnly:return priceOnly;
            case CustomerType.Cheap:    return cheap;
            case CustomerType.Referral: return referral;
            default:                    return standard;
        }
    }
}
