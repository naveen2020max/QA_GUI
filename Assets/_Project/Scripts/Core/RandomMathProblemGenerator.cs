using System;

public class RandomMathProblemGenerator
{
    private readonly MathNumberRange _range;
    private readonly char _operator;
    private Action<MathProblem> _OnProblemCreated;

    // Constructor
    public RandomMathProblemGenerator(MathNumberRange range, char mathOperator, Action<MathProblem> onProblemCreated)
    {
        if (!ValidateRange(range))
            throw new ArgumentException("Invalid range provided. Make sure min <= max.");

        if (!ValidateOperator(mathOperator))
            throw new ArgumentException("Invalid operator provided. Supported operators are +, -, *, and /.");

        _range = range;
        _operator = mathOperator;
        _OnProblemCreated = onProblemCreated;
    }

    public MathProblem CreateProblem()
    {
        // Generate a random math problem and call the action
        var problem = GenerateProblem();
        _OnProblemCreated?.Invoke(problem);
        return problem;
    }

    // Validates the range
    private bool ValidateRange(MathNumberRange range)
    {
        return range.Min <= range.Max;
    }

    // Validates the operator
    private bool ValidateOperator(char mathOperator)
    {
        char[] validOperators = { '+', '-', '*', '/' };
        return Array.Exists(validOperators, op => op == mathOperator);
    }

    // Generates a random math problem
    private MathProblem GenerateProblem()
    {
        var random = new Random();
        float number1 = random.Next(_range.Min, _range.Max + 1);
        float number2 = random.Next(_range.Min, _range.Max + 1);

        // Avoid division by zero
        if (_operator == '/' && number2 == 0)
        {
            number2 = 1; // Ensure number2 is non-zero
        }

        return new MathProblem(number1, number2, _operator);
    }
}
