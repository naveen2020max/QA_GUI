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

public class ProblemMaster : MonoBehaviour
{
    public event Action OnInitialized;
    public event Action<MathProblem> OnProblemGenerated;
    public event Action<MathResult> OnResultRecorded;
    public event Action<int> OnQuestionChange;
    public event Action OnLevelComplete;
    // Events to signal pause/resume requests
    public event Action RequestPauseTimer;
    public event Action RequestResumeTimer;

    private MathProblemProcessor _mathProblemProcessor;
    //private RandomMathProblemGenerator _randomMathProblemGenerator;
    private Stack<MathResult> _resultStack; // storage for all the math results (solution + user input)
    private Stack<MathSolution> _solutionStack;

    [Header("Dependencies")]
    [SerializeField] private QuestionLoader _questionLoader;

    [SerializeField] private MathNumberRange _range;
    [SerializeField] private char _operator;

    [SerializeField] private float timePerQuestion = 30f;
    [SerializeField] private int maxQuestions = 10;

    private int currentQuestionNumber = 0;
    //private float timer;
    private bool isLevelComplete = false;
    //private bool _isTimerFrozen = false; // Flag to check if the timer is frozen
    private List<MathProblem> _currentLevelQuestions;

    public float TimePerQuestion { get => timePerQuestion;  }
    public int MaxQuestions { get => maxQuestions; }

    

    // Expose current solution (if available) for validation elsewhere
    public MathSolution CurrentSolution => _solutionStack.Count > 0 ? _solutionStack.Peek() : default;

    private void Start()
    {
        StartLevel(_operator.ToString(), 1, 1);
    }

    public async Task StartLevel(string symbol, int difficulty, int level)
    {

        _currentLevelQuestions = await _questionLoader.LoadQuestionsForLevel(symbol, difficulty, level);

        if(_currentLevelQuestions == null || _currentLevelQuestions.Count == 0)
        {
            Debug.LogError("No questions loaded for the level. Cannot start level.");
            return;
        }
        maxQuestions = Math.Min(maxQuestions, _currentLevelQuestions.Count);
        var random = new System.Random();
        _currentLevelQuestions = _currentLevelQuestions.OrderBy(x => random.Next()).Take(maxQuestions).ToList();
        currentQuestionNumber = -1;
        Initialize();
    }

    private void Initialize()
    {
        // Initialize the MathProblemProcessor
        _mathProblemProcessor = new MathProblemProcessor();

        // Initialize the RandomMathProblemGenerator
        //_randomMathProblemGenerator = new RandomMathProblemGenerator(_range, _operator, problem =>
        //{
        //    // Trigger the OnProblemGenerated event
        //    //OnProblemGenerated?.Invoke(problem);
        //    // Process the problem
        //    _mathProblemProcessor.ProcessProblem(problem);
        //});

        // Initialize the solution and result stacks
        _solutionStack = new Stack<MathSolution>();
        _resultStack = new Stack<MathResult>();

        // Subscribe to MathProblemProcessor events
        _mathProblemProcessor.OnSolutionCreated += solution =>
        {
            Debug.Log($"Solution Created: {solution.CorrectAnswer}");
            _solutionStack.Push(solution);
        };

        _mathProblemProcessor.OnValidation += (problem, isValid) =>
        {
            Debug.Log(isValid ? "Valid problem" : "Invalid problem");
        };

        OnResultRecorded += res =>
        {
            Debug.Log(res.ToString());
        };

        // Trigger the initialization event
        OnInitialized?.Invoke();

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

    public void HandleLevelComplete()
    {
        // Handle the level completion event here
        Debug.Log("Level complete.");
        isLevelComplete = true;
        OnLevelComplete?.Invoke();
    }

    private void CreateNewProblem()
    {
        if (currentQuestionNumber < maxQuestions)
        {
            currentQuestionNumber++;
            //OnQuestionChange(currentQuestionNumber);
            //OnTimerUpdate(timer);
            // Generate a new problem
            var problem = _currentLevelQuestions[currentQuestionNumber];

            // Trigger the OnProblemGenerated event and process the problem
            OnProblemGenerated?.Invoke(problem);
            _mathProblemProcessor.ProcessProblem(problem);
        }
        else
        {
            Debug.Log("Max questions reached.");
            // Optionally, trigger a game-over or session-complete event here.
        }
    }

    public LevelResultInfo CalculateLevelResults()
    {
        // Ensure results reflect the intended number of questions
        int totalAttempted = _resultStack.Count; // How many were actually answered/timed out
        int correct = _resultStack.Count(result => result.IsAnsweredCorrect);

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

    public MathResult RecordResult(float ans)
    {
        if (_solutionStack.Count == 0)
        {
            Debug.LogError("No solution available. Cannot record result.");
            return default; // Return a default/empty result
        }

        // Create a new result using the latest solution and the user's answer
        MathResult res = new MathResult(_solutionStack.Peek(), ans);
        _resultStack.Push(res);

        // Trigger the result recorded event
        OnResultRecorded?.Invoke(res);

        return res;
    }
}
