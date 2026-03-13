using System;

[Serializable]
public class TireOrder
{
    public TireSize size;
    public TireSeason season;
    public TireCondition condition;
    public int quantity;

    public TireBrand brand;

    public string Display => $"{brand}  {size} • {season} • {condition} x{quantity}";
}
