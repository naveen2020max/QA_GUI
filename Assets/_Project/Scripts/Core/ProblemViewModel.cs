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
    public event Action OnTimerUpdated;
    public event Action OnQuestionNumberUpdated;
    public event Action<string> OnFeedbackUpdated;

    private ProblemMaster _problemMaster;
    private float _timer;
    private int _currentQuestionNumber;
    private int _maxQuestions;

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

    /// <summary>
    /// Initializes the ViewModel with a reference to the ProblemMaster, max question count, and initial timer value.
    /// </summary>
    /// <param name="problemMaster">Reference to the model manager.</param>
    /// <param name="maxQuestions">Total number of questions for the session.</param>
    /// <param name="initialTimer">Initial time per question.</param>
    public ProblemViewModel(ProblemMaster problemMaster, int maxQuestions, float initialTimer)
    {
        _problemMaster = problemMaster;
        MaxQuestions = maxQuestions;
        Timer = initialTimer;
        CurrentQuestionNumber = 0;

        // Subscribe to model events.
        _problemMaster.OnProblemGenerated += HandleProblemGenerated;
        _problemMaster.OnResultRecorded += HandleResultRecorded;
    }

    /// <summary>
    /// Handler for when a new problem is generated.
    /// </summary>
    /// <param name="problem">The newly generated MathProblem.</param>
    private void HandleProblemGenerated(MathProblem problem)
    {
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
        Timer = Mathf.Max(0, Timer - deltaTime);
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
    }
}
