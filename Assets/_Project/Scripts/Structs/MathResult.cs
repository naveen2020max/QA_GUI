using UnityEngine;

// this is for storing user input
public struct MathResult
{
    public MathSolution Solution;
    public float InputAnswer;

    // Validate the input from UiManager and return it
    public bool IsAnsweredCorrect
    {
        get
        {
            return Mathf.Approximately(Solution.CorrectAnswer,InputAnswer);
        }
    }

    public MathResult(MathSolution sol, float ans)
    {
        Solution = sol;
        InputAnswer = ans;
    }

    override public string ToString()
    {
        return $"Solution: \n{Solution.ToString()}, \nInput: {InputAnswer}";
    }
}
