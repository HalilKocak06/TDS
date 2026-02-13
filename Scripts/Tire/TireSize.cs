using System;

    [Serializable]
    public struct TireSize
    {
        public int width;
        public int aspect;
        public int rim;

        public TireSize(int width, int aspect, int rim)
        {
            this.width = width;
            this.aspect = aspect;
            this.rim = rim;
        }

    public override string ToString() => $"{width}/{aspect} R{rim}" ;

  }