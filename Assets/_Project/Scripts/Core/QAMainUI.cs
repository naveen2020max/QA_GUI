using UnityEngine;
using UnityEngine.UIElements;

public class QAMainUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private StyleSheet uiStyleSheet;
    [SerializeField] private ProblemMaster problemMaster;
    [SerializeField] private MathProblemUIManager problemUIManager;

    //private ProblemViewModel viewModel;

    // UI Element references
    private VisualElement _container;
    private Label _heading;
    private VisualElement _levelState;
    private Label _timerLabel;
    private Label _currQuestionLabel;
    private Label _questionText;
    private Label _answerText;
    private FloatField _answerField;
    private Button _submitButton;
    private Label _feedbackText;
    private Button _nextQuestionButton;

    // Public properties with getters
    public VisualElement Container => _container;
    public Label Heading => _heading;
    public VisualElement LevelState => _levelState;
    public Label TimerLabel => _timerLabel;
    public Label CurrQuestionLabel => _currQuestionLabel;
    public Label QuestionText => _questionText;
    public Label AnswerText => _answerText;
    public FloatField AnswerField => _answerField;
    public Button SubmitButton => _submitButton;
    public Label FeedbackText => _feedbackText;
    public Button NextQuestionButton => _nextQuestionButton;

    private void OnEnable()
    {
        if(problemMaster == null) problemMaster = GetComponent<ProblemMaster>();
        if(problemUIManager == null) problemUIManager = GetComponent<MathProblemUIManager>();
        Generate();
        //CreateProblemViewModel();
        problemUIManager.Initiation(this);
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            return;
        }
        Generate();
    }

    private void Generate()
    {
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument is not assigned.");
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;
        root.Clear();
        // Apply the external stylesheet
        if (uiStyleSheet != null && !root.styleSheets.Contains(uiStyleSheet))
        {
            root.styleSheets.Add(uiStyleSheet);
        }

        // Build the UI structure

        // Create Container
        _container = new VisualElement { name = "Container" };
        _container.AddToClassList("container");

        // Create Heading
        _heading = new Label("Addition +") { name = "Heading" };
        _heading.AddToClassList("Question");

        // Create LevelState container
        _levelState = new VisualElement { name = "LevelState" };
        _levelState.AddToClassList("level-state");

        // Create Timer Label inside LevelState
        _timerLabel = new Label("Label") { name = "Timer" };
        _timerLabel.AddToClassList("Question");

        // Create Current Question Label inside LevelState
        _currQuestionLabel = new Label("Label") { name = "CurrQuestion" };
        _currQuestionLabel.AddToClassList("Question");

        _levelState.Add(_timerLabel);
        _levelState.Add(_currQuestionLabel);

        // Create Question Text
        _questionText = new Label("Label") { name = "QuestionText" };
        _questionText.AddToClassList("Question");

        // Create Answer Text
        _answerText = new Label("Label") { name = "AnswerText" };
        _answerText.AddToClassList("Question");

        // Create Answer Field
        _answerField = new FloatField { name = "AnswerField" };
        _answerField.AddToClassList("answer-field");

        // Create Submit Button
        _submitButton = new Button { text = "Submit", name = "Submit" };
        _submitButton.AddToClassList("CommonButton");

        // Create Feedback Text
        _feedbackText = new Label("Label") { name = "FeedbackText" };
        _feedbackText.AddToClassList("Question");

        // Create Next Question Button
        _nextQuestionButton = new Button { text = "Next Question", name = "NextQuestion" };
        _nextQuestionButton.AddToClassList("CommonButton");

        // Add UI elements to container in order
        _container.Add(_heading);
        _container.Add(_levelState);
        _container.Add(_questionText);
        _container.Add(_answerText);
        _container.Add(_answerField);
        _container.Add(_submitButton);
        _container.Add(_feedbackText);
        _container.Add(_nextQuestionButton);

        // Finally, add the container to the root element
        root.Add(_container);

        // (Optional) Add event handlers as needed:
        //_submitButton.clicked += OnSubmitClicked;
        //_nextQuestionButton.clicked += () => Debug.Log("Next Question button clicked");
    }

    //private void CreateProblemViewModel()
    //{
    //    if (problemMaster == null)
    //    {
    //        Debug.LogError("ProblemMaster is not assigned.");
    //        return;
    //    }
    //    viewModel = new ProblemViewModel(problemMaster);
    //    viewModel.OnProblemUpdated += UpdateProblemUI;
    //    viewModel.OnTimerUpdated += UpdateTimerUI;
    //    viewModel.OnFeedbackUpdated += UpdateFeedbackUI;
    //    viewModel.OnQuestionNumberUpdated += UpdateQuestionNumberUI;
    //}

    //private void OnDisable()
    //{
    //    viewModel?.Dispose();
    //}

    //private void Update()
    //{
    //    // Update the timer in the ViewModel
    //    viewModel?.UpdateTimer(Time.deltaTime);
    //}

    //private void UpdateProblemUI()
    //{
    //    // Update the question label based on the current problem
    //    if (viewModel != null)
    //    {
    //        _questionText.text = $"{viewModel.CurrentProblem.Number1} {viewModel.CurrentProblem.Operator} {viewModel.CurrentProblem.Number2}";
    //    }
    //}

    //private void UpdateTimerUI()
    //{
    //    _timerLabel.text = $"Time: {Mathf.CeilToInt(viewModel.Timer)}";
    //}

    //private void UpdateFeedbackUI(string feedback)
    //{
    //    _feedbackText.text = feedback;
    //}

    //private void UpdateQuestionNumberUI()
    //{
    //    _currQuestionLabel.text = $"Question: {viewModel.CurrentQuestionNumber}";
    //}

    private void OnSubmitClicked()
    {
        // Instead of accessing ProblemMaster directly, you would have the ViewModel handle the submission logic.
        // For instance, you might pass the user's answer to the model through the ViewModel here.
        Debug.Log("Submit button clicked - forwarding answer processing via ViewModel.");
        // Example: viewModel.ProcessUserAnswer(answerValue);
    }
}
