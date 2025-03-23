using System;

// Struct to store the range of numbers
[Serializable]
public struct MathNumberRange
{
    public int Min;
    public int Max;

    public MathNumberRange(int min, int max)
    {
        Min = min;
        Max = max;
    }
}
