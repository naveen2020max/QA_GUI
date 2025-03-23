using System;

// struct for MathSolution
[Serializable]
public struct MathSolution
{
    public MathProblem Problem;
    public float CorrectAnswer;

    public MathSolution(MathProblem problem, float correctAnswer)
    {
        Problem = problem;
        CorrectAnswer = correctAnswer;
    }
    override public string ToString()
    {
        return $"Problem: \n{Problem.ToString()}, \nCorrect Answer: {CorrectAnswer}";
    }
}
