using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CustomerType
{
    Premium, //%10 pahalıya alır
    Standard, //market price'a alır
    PriceOnly, //Sadece fiyat sorar almaz
    Cheap, // %5'e kadar indirim ister Müşteri fiyatından
    Referral, // piyasaya fiyatının %5 üstünde satılabilir.
}


[CreateAssetMenu(menuName = "TDS/Customer/Profile", fileName = "CustomerProfile")]
public class CustomerProfileSO : ScriptableObject
{
    public CustomerType type = CustomerType.Standard;

    [Header("Negotiation")]
    [Range(1,5)] public int maxOfferTurns = 2; //müşteri 2 kez fiyat sorar.

    [Header("Acceptance band vs Market Price")]
    [Tooltip("Market üstüne izin(0.10 => +%10)")]
    public float maxMarkup = 0.00f;

    [Tooltip("Market altına hedef (-0.05 => -%5)")]
    public float minDiscount = 0.00f;

    [Header("Behavior")]
    public bool willNeverBuy = false; // PriceOnly için hiç almayacak.

    
}
