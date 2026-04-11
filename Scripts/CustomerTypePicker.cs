using UnityEngine;

public class CustomerTypePicker
{

   private static readonly CustomerType[] allTypes =
    {
        CustomerType.Standard,
        CustomerType.Cheap,
        CustomerType.Referral,
        CustomerType.Premium,
        CustomerType.PriceOnly
    };

    public static CustomerType PickRandom()
    {
        int index = Random.Range(0, allTypes.Length);
        return allTypes[index];
    }
    
}
