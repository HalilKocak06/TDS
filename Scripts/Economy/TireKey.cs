using System;

[Serializable]
public struct TireKey: IEquatable<TireKey> //Daha hızlı arar IEquatable sayesinde ayrıca Struct olduğu için(PERFORMANS)
{
    public int width; //205
    public int aspect; //55
    public int rim; //16
    public TireSeason season; //summer
    public TireCondition condition; //new
    public TireBrand brand;

    public TireKey(int width, int aspect, int rim, TireSeason season, TireCondition condition, TireBrand brand) //Constructor gelen bilgiyi eşliyor.
    {
        this.width = width;
        this.aspect = aspect;
        this.rim = rim;
        this.season = season;
        this.condition = condition;
        this.brand = brand;
    }

    public bool Equals(TireKey other) //Burada ise iki lastik tamamen aynı mı sorusunu soruyoruz ve ona göre onaylıyoruz.
        => width == other.width && aspect == other.aspect && rim == other.rim && season == other.season && condition == other.condition && brand == other.brand;

    public override bool Equals(object obj) => obj is TireKey other && Equals(other); //Object tabanlı karşılaştırmaları destekler.

    public override int GetHashCode() //Dictionary performansı için en önemli kısım,
    //Çünkü Dictionary<TireKey, int> kullanacaksın.
    //Dictionary key'leri HashCode üzerinden arama yapıyor.
    {
        unchecked
        {
            int h= 17;
            h = h * 31 + width;
            h = h * 31 + aspect;
            h = h * 31 + rim;
            h = h * 31 + (int)season;
            h = h * 31 + (int)condition;
            h = h * 31 + (int)brand;
            return h;
        }
    }

    public override string ToString() => $"{brand} {width}/{aspect}R{rim} {season} {condition}";

}