using System;

[Serializable]
public class TireOrder
{
    public TireSize size;
    public TireSeason season;
    public TireCondition condition;
    public int quantity;

    public string Display => $"{size} • {season} • {condition} x{quantity}";
}
