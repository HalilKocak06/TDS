using UnityEngine;

public class CustomerTypePickerTester : MonoBehaviour
{
    [SerializeField] private bool testOnStart = true;
    [SerializeField] private int sampleCount = 10;

    private void Start()
    {
        if (!testOnStart)
            return;

        for (int i = 0; i < sampleCount; i++)
        {
            CustomerType picked = CustomerTypePicker.PickRandom();
            Debug.Log($"[CustomerTypePickerTester] Pick {i + 1}: {picked}");
        }
    }
}