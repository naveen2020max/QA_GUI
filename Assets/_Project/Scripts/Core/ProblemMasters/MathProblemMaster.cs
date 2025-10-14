using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MathProblemMaster : ProblemMaster<MathProblem, MathSolution, MathResult>
{

    [SerializeField] private QuestionLoader _questionLoader;
    [SerializeField] private MathNumberRange _range;
    [SerializeField] private char _operator;

    private MathProblemProcessor _mathProblemProcessor;
    protected override MathResult CreateResult(float userAnswer)
    {
        if(_solutionStack.Count == 0) return default;
        return new MathResult(_solutionStack.Pop(), userAnswer);
    }

    protected override async Task<List<MathProblem>> LoadQuestionsAsync(int difficulty, int level)
    {
        return await _questionLoader.LoadQuestionsForLevel(_operator.ToString(), difficulty, level);
    }

    protected override void ProcessProblem(MathProblem problem)
    {
        _mathProblemProcessor ??= new MathProblemProcessor();
        _mathProblemProcessor.ProcessProblem(problem);
    }

    private async void Start()
    {
        await StartLevel(1, 1);
    }

    protected override void Initialize(Action anyThing)
    {
        base.Initialize(ConfigureSolutionHandler);
        
    }

    private void ConfigureSolutionHandler()
    {
        _mathProblemProcessor ??= new MathProblemProcessor();
        _mathProblemProcessor.OnSolutionCreated += solution =>
        {
            Debug.Log($"Solution Created: {solution.CorrectAnswer}");
            _solutionStack.Push(solution);
        };
        Debug.Log("MathProblemMaster initialized.");
    }
}
