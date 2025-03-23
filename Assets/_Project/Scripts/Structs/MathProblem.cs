using System;

// Record struct for MathProblem
[Serializable]
public struct MathProblem
{
    public float Number1;
    public float Number2;
    public char Operator;

    public MathProblem(float number1, float number2, char mathOperator)
    {
        Number1 = number1;
        Number2 = number2;
        Operator = mathOperator;
    }

    override public string ToString()
    {
        return $"MathProblem: {Number1} {Operator} {Number2}";
    }
}
