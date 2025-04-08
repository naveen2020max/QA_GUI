using System;
using UnityEngine;

/// <summary>
/// ViewModel for managing the current math problem session.
/// It bridges the ProblemMaster (Model) with the UI (View).
/// </summary>
public class ProblemViewModel : IDisposable
{
    // Events to notify the View about property changes.
    public event Action OnProblemUpdated;
    public event Action OnTimerUpdated, OnTimeUp;
    public event Action OnLevelComplete;
    public event Action OnQuestionNumberUpdated;
    public event Action<string> OnFeedbackUpdated;

    private ProblemMaster _problemMaster;
    private float _timer;
    private int _currentQuestionNumber;
    private int _maxQuestions;
    private bool _isLevelComplete;

    // Exposed properties
    public MathProblem CurrentProblem { get; private set; }
    public MathSolution CurrentSolution { get; private set; }

    public float Timer
    {
        get => _timer;
        private set
        {
            _timer = value;
            OnTimerUpdated?.Invoke();
        }
    }

    public int CurrentQuestionNumber
    {
        get => _currentQuestionNumber;
        private set
        {
            _currentQuestionNumber = value;
            OnQuestionNumberUpdated?.Invoke();
        }
    }

    public int MaxQuestions
    {
        get => _maxQuestions;
        private set => _maxQuestions = value;
    }

    public bool IsLevelComplete => CurrentQuestionNumber >= MaxQuestions;

    /// <summary>
    /// Initializes the ViewModel with a reference to the ProblemMaster, max question count, and initial timer value.
    /// </summary>
    /// <param name="problemMaster">Reference to the model manager.</param>
    /// <param name="maxQuestions">Total number of questions for the session.</param>
    /// <param name="initialTimer">Initial time per question.</param>
    public ProblemViewModel(ProblemMaster problemMaster)
    {
        _problemMaster = problemMaster;
        MaxQuestions = problemMaster.MaxQuestions;
        Timer = problemMaster.TimePerQuestion;
        CurrentQuestionNumber = 0;

        // Subscribe to model events.
        _problemMaster.OnProblemGenerated += HandleProblemGenerated;
        _problemMaster.OnResultRecorded += HandleResultRecorded;

        // Subscribe to ViewModel events.
        OnTimeUp += _problemMaster.HandleTimeUp;
        OnLevelComplete += _problemMaster.HandleLevelComplete;
    }

    /// <summary>
    /// Handler for when a new problem is generated.
    /// </summary>
    /// <param name="problem">The newly generated MathProblem.</param>
    private void HandleProblemGenerated(MathProblem problem)
    {
        Debug.Log($"New Problem Generated: {problem}");
        CurrentProblem = problem;
        CurrentQuestionNumber++;
        OnProblemUpdated?.Invoke();
    }

    /// <summary>
    /// Handler for when a result is recorded.
    /// </summary>
    /// <param name="result">The MathResult containing the solution and user answer.</param>
    private void HandleResultRecorded(MathResult result)
    {
        // For simplicity, we assume MathResult has a UserAnswer and a ProblemSolution property.
        // Compare user answer with the correct answer.
        bool isCorrect = Mathf.Approximately(result.InputAnswer, result.Solution.CorrectAnswer);
        OnFeedbackUpdated?.Invoke(isCorrect ? "Correct Answer!" : "Incorrect Answer");
    }

    /// <summary>
    /// Updates the timer by subtracting deltaTime.
    /// Should be called from a MonoBehaviour's Update loop.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since the last update.</param>
    public void UpdateTimer(float deltaTime)
    {
        if(_isLevelComplete) return; // Skip if the level is already complete
        Timer = Mathf.Max(0, Timer - deltaTime);
        if(Timer <= 0)
        {
            if(!IsLevelComplete)
                OnTimeUp?.Invoke();
            else
            {
                OnLevelComplete?.Invoke();
                _isLevelComplete = true; // Mark the level as complete
                Dispose();
            }
            ResetTimer(_problemMaster.TimePerQuestion); // Reset timer for the next question
        }
    }

    /// <summary>
    /// Resets the timer for a new question.
    /// </summary>
    /// <param name="newTime">New time value for the timer.</param>
    public void ResetTimer(float newTime)
    {
        Timer = newTime;
    }

    /// <summary>
    /// Clean up subscriptions.
    /// </summary>
    public void Dispose()
    {
        _problemMaster.OnProblemGenerated -= HandleProblemGenerated;
        _problemMaster.OnResultRecorded -= HandleResultRecorded;

        OnTimeUp -= _problemMaster.HandleTimeUp;
        //OnLevelComplete -= _problemMaster.HandleLevelComplete;
        CleanUpAllEvent();
    }

    private void CleanUpAllEvent()
    {
        CleanUpEvent(OnProblemUpdated);
        CleanUpEvent(OnTimerUpdated);
        CleanUpEvent(OnTimeUp);
        CleanUpEvent(OnLevelComplete);
        CleanUpEvent(OnQuestionNumberUpdated);
        //CleanUpEvent(OnFeedbackUpdated);
    }

    private void CleanUpEvent(Action action)
    {
        if (action != null)
        {
            foreach (var d in action.GetInvocationList())
            {
                action -= (Action)d;
            }
        }
    }
}
