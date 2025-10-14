using System;
using UnityEngine;

public enum ResultType
{
    None = 0,
    Correct = 1,
    Incorrect = 2,
    TimeUp = 3,
    LastQuestionCorrect = 4,
    LevelComplete = 5
}

public class ProblemViewModel<TProblem, TSolution, TResult, TMaster> : IDisposable
    where TMaster : ProblemMaster<TProblem, TSolution, TResult>
{
    public event Action OnProblemUpdated;
    public event Action OnTimerUpdated, OnTimeUp;
    public event Action OnLevelComplete;
    public event Action OnQuestionNumberUpdated;
    public event Action<string, Color> OnFeedbackUpdated;
    public event Action<ResultType> OnResultSubmited;

    public Func<float> InputAnswerValue; // For numeric problems (ignored for text problems)
    private bool _isTimerFrozen;
    private bool _isLevelComplete;
    private float _timer;

    private readonly TMaster _problemMaster;
    private int _currentQuestionNumber;

    public TProblem CurrentProblem { get; private set; }
    public TSolution CurrentSolution { get; private set; }

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

    public int MaxQuestions => _problemMaster.MaxQuestions;
    public bool IsLevelComplete => _currentQuestionNumber >= MaxQuestions;

    public ProblemViewModel(TMaster problemMaster)
    {
        _problemMaster = problemMaster;
        Timer = problemMaster.TimePerQuestion;
        CurrentQuestionNumber = 0;

        _problemMaster.OnProblemGenerated += HandleProblemGenerated;
        _problemMaster.OnResultRecorded += HandleResultRecorded;
        _problemMaster.RequestPauseTimer += PauseTimer;
        _problemMaster.RequestResumeTimer += ResumeTimer;

        OnTimeUp += _problemMaster.HandleTimeUp;
        OnLevelComplete += _problemMaster.HandleLevelComplete;
    }

    private void HandleProblemGenerated(TProblem problem)
    {
        CurrentProblem = problem;
        CurrentQuestionNumber++;
        OnProblemUpdated?.Invoke();
    }

    private void HandleResultRecorded(TResult result)
    {
        if (result is IResultFeedback feedbackProvider)
        {
            var (message, color) = feedbackProvider.GetFeedback();
            OnFeedbackUpdated?.Invoke(message, color);
        }
    }

    public void UpdateTimer(float deltaTime)
    {
        if (_isTimerFrozen || _isLevelComplete) return;

        Timer = Mathf.Max(0, Timer - deltaTime);
        if (Timer <= 0)
        {
            if (!IsLevelComplete)
                OnTimeUp?.Invoke();
            else
            {
                _isLevelComplete = true;
                OnLevelComplete?.Invoke();
                Dispose();
            }

            ResetTimer(_problemMaster.TimePerQuestion);
        }
    }

    public void ResetTimer(float newTime) => Timer = newTime;

    public void CreateNewProblem()
    {
        if (!_problemMaster.TryCreateNewProblem()) return;
        ResumeTimer();
        ResetTimer(_problemMaster.TimePerQuestion);
    }

    public void SubmitAnswer(float numericAnswer)
    {
        if (_problemMaster is IMathAnswerHandler<TSolution, TResult> handler)
        {
            TResult result = handler.RecordResult(numericAnswer);
            HandleSubmissionResult(result);
        }
    }

    public void SubmitAnswer(int optionIndex)
    {
        
        if (_problemMaster is ITextAnswerHandler<TSolution, TResult> handler)
        {
            TResult result = handler.RecordResult(optionIndex);
            HandleSubmissionResult(result);
        }
    }

    private void HandleSubmissionResult(TResult result)
    {
        if (result is IResultTypeProvider provider)
        {
            var type = provider.GetResultType();
            OnResultSubmited?.Invoke(type);

            if (type == ResultType.Correct || type == ResultType.LastQuestionCorrect)
            {
                PauseTimer();
                if (IsLevelComplete)
                {
                    OnLevelComplete?.Invoke();
                    _isLevelComplete = true;
                    Dispose();
                }
            }
        }
    }

    public void PauseTimer() => _isTimerFrozen = true;
    public void ResumeTimer() => _isTimerFrozen = false;

    public void Dispose()
    {
        _problemMaster.OnProblemGenerated -= HandleProblemGenerated;
        _problemMaster.OnResultRecorded -= HandleResultRecorded;
        _problemMaster.RequestPauseTimer -= PauseTimer;
        _problemMaster.RequestResumeTimer -= ResumeTimer;
        OnTimeUp -= _problemMaster.HandleTimeUp;
    }
}
