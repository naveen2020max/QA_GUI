using System;
using UnityEngine;

public class MathProblemProcessor
{
    // Event for validation step, provides the MathProblem and a bool indicating if it's valid
    public event Action<MathProblem, bool> OnValidation;

    // Event for when a MathSolution is created
    public event Action<MathSolution> OnSolutionCreated;

    // Method to process a MathProblem
    public void ProcessProblem(MathProblem problem)
    {
        bool isValid = ValidateProblem(problem);

        // Trigger validation event
        OnValidation?.Invoke(problem, isValid);

        if (!isValid)
        {
            Debug.LogWarning("Invalid math problem provided.");
            return;
        }

        // Calculate the solution
        float correctAnswer = SolveProblem(problem);
        MathSolution solution = new MathSolution(problem, correctAnswer);

        // Trigger solution created event
        OnSolutionCreated?.Invoke(solution);
    }

    // Validates the math problem (e.g., no division by zero)
    private bool ValidateProblem(MathProblem problem)
    {
        if (problem.Operator == '/' && problem.Number2 == 0)
        {
            return false;
        }
        return true;
    }

    // Solves the math problem and returns the correct answer
    private float SolveProblem(MathProblem problem)
    {
        return problem.Operator switch
        {
            '+' => problem.Number1 + problem.Number2,
            '-' => problem.Number1 - problem.Number2,
            '*' => problem.Number1 * problem.Number2,
            '/' => problem.Number1 / problem.Number2,
            _ => throw new InvalidOperationException("Unsupported operator: " + problem.Operator),
        };
    }
}
