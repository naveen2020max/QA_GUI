using UnityEngine;
using UnityEngine.UIElements;
using System;

public class TextProblemUIManager : MonoBehaviour
{
    private TextProblemMaster problemMaster;
    private TextQAMainUI ui;

    private ProblemViewModel<TextQuestion, TextAnswer, TextResult, TextProblemMaster> viewModel;

    private Label questionText;
    private VisualElement optionsContainer;
    private Label feedbackText;
    private Label timerLabel;
    private Label questionCounter;
    private Button nextQuestionButton;
    private Button startLevelButton;

    public event Action<float> OnUpdateCalled;

    private void Awake()
    {
        problemMaster = GetComponent<TextProblemMaster>();
    }

    public void Initiation(TextQAMainUI mainUI)
    {
        ui = mainUI;
        questionText = ui.QuestionText;
        optionsContainer = ui.OptionsContainer;
        feedbackText = ui.FeedbackText;
        timerLabel = ui.TimerLabel;
        questionCounter = ui.CurrQuestionLabel;
        nextQuestionButton = ui.NextQuestionButton;
        startLevelButton = ui.StartLevelBtn;

        CreateProblemViewModel();

        nextQuestionButton.clicked += viewModel.CreateNewProblem;
        nextQuestionButton.clicked += PrepareForNextQuestion;
        //startLevelButton.clicked += viewModel.StartLevel;

        SetVisualElementDisplayStyleNONE(new VisualElement[] { nextQuestionButton, startLevelButton });
    }

    private void CreateProblemViewModel()
    {
        if (problemMaster == null)
        {
            Debug.LogError("TextProblemMaster is not assigned.");
            return;
        }

        viewModel = new ProblemViewModel<TextQuestion, TextAnswer, TextResult, TextProblemMaster>(problemMaster);
        viewModel.OnProblemUpdated += UpdateProblemUI;
        viewModel.OnTimerUpdated += UpdateTimerUI;
        //viewModel.OnFeedbackUpdated += UpdateFeedbackUI;
        viewModel.OnQuestionNumberUpdated += UpdateQuestionNumberUI;
        viewModel.OnLevelComplete += HandleLevelCompleted;
        viewModel.OnResultSubmited += UpdateUIOnResult;

        OnUpdateCalled += viewModel.UpdateTimer;
    }

    private void OnDisable()
    {
        viewModel?.Dispose();

        if (nextQuestionButton != null)
        {
            nextQuestionButton.clicked -= viewModel.CreateNewProblem;
            nextQuestionButton.clicked -= PrepareForNextQuestion;
        }

        if (startLevelButton != null)
        {
            //startLevelButton.clicked -= viewModel.StartLevel;
        }

        OnUpdateCalled -= viewModel.UpdateTimer;
    }

    private void Update()
    {
        OnUpdateCalled?.Invoke(Time.deltaTime);
    }

    private void HandleLevelCompleted()
    {
        Debug.Log("Level Completed! from TextProblemUIManager");
        OnUpdateCalled -= viewModel.UpdateTimer;
        feedbackText.text = "🎉 Level Complete!";
        feedbackText.style.color = Color.green;
    }

    private void UpdateProblemUI()
    {
        if (viewModel == null || viewModel.CurrentProblem == null)
            return;

        var problem = viewModel.CurrentProblem;

        questionText.text = problem.QuestionText;
        ui.DisplayOptions(problem.Options, index =>
        {
            viewModel.SubmitAnswer(index);
            Debug.Log(index);
        });
    }

    private void UpdateTimerUI()
    {
        timerLabel.text = $"Time: {Mathf.CeilToInt(viewModel.Timer)}";
    }

    private void UpdateFeedbackUI(string feedback)
    {
        feedbackText.text = feedback;
        feedbackText.style.color = Color.black;
    }

    private void UpdateQuestionNumberUI()
    {
        questionCounter.text = $"Question: {viewModel.CurrentQuestionNumber}";
    }

    private void UpdateUIOnResult(ResultType result)
    {
        bool correct = result == ResultType.Correct || result == ResultType.LastQuestionCorrect;

        feedbackText.text = correct ? "Correct!" : "Wrong Answer!";
        feedbackText.style.color = correct ? Color.green : Color.red;

        if (viewModel.CurrentProblem != null)
        {
            ui.HighlightCorrectOption(viewModel.CurrentProblem.CorrectOptionIndex);
        }

        nextQuestionButton.style.display = correct ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void PrepareForNextQuestion()
    {
        feedbackText.text = string.Empty;
        nextQuestionButton.style.display = DisplayStyle.None;
    }

    private void SetVisualElementDisplayStyleNONE(VisualElement[] visual)
    {
        foreach (var element in visual)
        {
            element.style.display = DisplayStyle.None;
        }
    }
    private void SetVisualElementDisplayStyleFLEX(VisualElement[] visual)
    {
        foreach (var element in visual)
        {
            element.style.display = DisplayStyle.Flex;
        }
    }
}
