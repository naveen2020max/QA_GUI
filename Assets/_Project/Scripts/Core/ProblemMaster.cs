using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
public struct LevelResultInfo
{
    public int TotalQuestionsAttempted;
    public int CorrectAnswers;
    public int MaxQuestionsInLevel; // Added this for clarity
}

public abstract class ProblemMaster<TProblem, TSolution, TResult> : MonoBehaviour
{
    public event Action OnInitialized;
    public event Action<TProblem> OnProblemGenerated;
    public event Action<TResult> OnResultRecorded;
    public event Action<int> OnQuestionChange;
    public event Action OnLevelComplete;
    // Events to signal pause/resume requests
    public event Action RequestPauseTimer;
    public event Action RequestResumeTimer;

    //->private MathProblemProcessor _mathProblemProcessor;
    //private RandomMathProblemGenerator _randomMathProblemGenerator;
    protected Stack<TResult> _resultStack; // storage for all the math results (solution + user input)
    protected Stack<TSolution> _solutionStack;

    [Header("Dependencies")]
    //->[SerializeField] private QuestionLoader _questionLoader;

    //->[SerializeField] private MathNumberRange _range;
    //->[SerializeField] private char _operator;

    [SerializeField] private float timePerQuestion = 30f;
    [SerializeField] private int maxQuestions = 10;

    protected int currentQuestionNumber = 0;
    //private float timer;
    private bool isLevelComplete = false;
    //private bool _isTimerFrozen = false; // Flag to check if the timer is frozen
    protected List<TProblem> _currentLevelQuestions;

    public float TimePerQuestion { get => timePerQuestion;  }
    public int MaxQuestions { get => maxQuestions; }

    

    // Expose current solution (if available) for validation elsewhere
    public TSolution CurrentSolution => _solutionStack.Count > 0 ? _solutionStack.Peek() : default;

    //abstracts for subclass specific logic
    protected abstract Task<List<TProblem>> LoadQuestionsAsync(int difficulty, int level);
    protected abstract void ProcessProblem(TProblem problem);
    protected abstract TResult CreateResult(float userAnswer);

    protected void TriggerOnResultRecorded(TResult result) => OnResultRecorded?.Invoke(result);

    public async Task StartLevel(int difficulty, int level)
    {

        _currentLevelQuestions = await LoadQuestionsAsync(difficulty, level);

        if(_currentLevelQuestions == null || _currentLevelQuestions.Count == 0)
        {
            Debug.LogError("No questions loaded for the level. Cannot start level.");
            return;
        }
        maxQuestions = Math.Min(maxQuestions, _currentLevelQuestions.Count);
        var random = new System.Random();
        _currentLevelQuestions = _currentLevelQuestions.OrderBy(x => random.Next()).Take(maxQuestions).ToList();
        currentQuestionNumber = -1;
        Initialize(null);
    }

    protected virtual void Initialize(Action anyThing)
    {
        // Initialize the MathProblemProcessor
        //->_mathProblemProcessor = new MathProblemProcessor();

        // Initialize the RandomMathProblemGenerator
        //_randomMathProblemGenerator = new RandomMathProblemGenerator(_range, _operator, problem =>
        //{
        //    // Trigger the OnProblemGenerated event
        //    //OnProblemGenerated?.Invoke(problem);
        //    // Process the problem
        //    _mathProblemProcessor.ProcessProblem(problem);
        //});

        // Initialize the solution and result stacks
        _solutionStack = new Stack<TSolution>();
        _resultStack = new Stack<TResult>();

        // Subscribe to MathProblemProcessor events
        /*_mathProblemProcessor.OnSolutionCreated += solution =>
        {
            Debug.Log($"Solution Created: {solution.CorrectAnswer}");
            _solutionStack.Push(solution);
        };*/

        /*_mathProblemProcessor.OnValidation += (problem, isValid) =>
        {
            Debug.Log(isValid ? "Valid problem" : "Invalid problem");
        };*/

        OnResultRecorded += res =>
        {
            Debug.Log(res.ToString());
        };

        // Trigger the initialization event
        OnInitialized?.Invoke();

        anyThing?.Invoke();

        // Start the first problem
        CreateNewProblem();
    }


    // Methods for ProblemMaster (or other scripts) to call
    public void TriggerPause()
    {
        Debug.Log("ProblemMaster: Requesting Timer Pause");
        RequestPauseTimer?.Invoke();
    }

    public void TriggerResume()
    {
        Debug.Log("ProblemMaster: Requesting Timer Resume");
        RequestResumeTimer?.Invoke();
    }


    public void HandleTimeUp()
    {
        // Handle the time-up event here
        Debug.Log("Time's up for the current question.");
        // Optionally record a default result for unanswered question here
        RecordResult(0f);
        CreateNewProblem();
    }

    public virtual void HandleLevelComplete()
    {
        // Handle the level completion event here
        Debug.Log("Level complete.");
        isLevelComplete = true;
        OnLevelComplete?.Invoke();
    }

    protected virtual void CreateNewProblem()
    {
        if (++currentQuestionNumber >= maxQuestions)
        {
            HandleLevelComplete();
            return;

        }
        var problem = _currentLevelQuestions[currentQuestionNumber];

        OnProblemGenerated?.Invoke(problem);
        //->_mathProblemProcessor.ProcessProblem(problem);
        ProcessProblem(problem);
    }

    public virtual LevelResultInfo CalculateLevelResults(Func<TResult, bool> isCorrectPredicate)
    {
        // Ensure results reflect the intended number of questions
        int totalAttempted = _resultStack.Count; // How many were actually answered/timed out
        int correct = _resultStack.Count(result => isCorrectPredicate(result));
        Debug.Log($"Level Results: Attempted={totalAttempted}, Correct={correct}, MaxQuestions={this.maxQuestions}");
        return new LevelResultInfo
        {
            TotalQuestionsAttempted = totalAttempted,
            CorrectAnswers = correct,
            MaxQuestionsInLevel = this.maxQuestions // Use the configured max questions
        };
    }

    public bool TryCreateNewProblem()
    {
        if (currentQuestionNumber < maxQuestions)
        {
            CreateNewProblem();
            return true;
        }
        else
        {
            Debug.Log("Max questions reached. Cannot create new problem.");
            return false;
        }
    }

    public bool IsNewProblemAvailable()
    {
        return currentQuestionNumber < maxQuestions;
    }

    public TResult RecordResult(float ans)
    {
        if (_solutionStack.Count == 0)
        {
            Debug.LogError("No solution available. Cannot record result.");
            return default; // Return a default/empty result
        }

        //// Create a new result using the latest solution and the user's answer
        //MathResult res = new MathResult(_solutionStack.Peek(), ans);
        //_resultStack.Push(res);

        //// Trigger the result recorded event
        //OnResultRecorded?.Invoke(res);

        var result = CreateResult(ans);
        _resultStack.Push(result);
        OnResultRecorded?.Invoke(result);

        return result;
    }
}
