using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class TextQAMainUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private StyleSheet uiStyleSheet;
    [SerializeField] private TextProblemMaster problemMaster;
    [SerializeField] private TextProblemUIManager problemUIManager;

    private VisualElement _container;
    private Label _heading;
    private VisualElement _levelState;
    private Label _timerLabel;
    private Label _currQuestionLabel;
    private Label _questionText;
    private VisualElement _optionsContainer;
    private Label _feedbackText;
    private Button _nextQuestionButton;
    private Button _startLevelBtn;

    // For internal tracking
    private List<Button> _optionButtons = new();

    public VisualElement Container => _container;
    public Label Heading => _heading;
    public Label TimerLabel => _timerLabel;
    public Label CurrQuestionLabel => _currQuestionLabel;
    public Label QuestionText => _questionText;
    public Label FeedbackText => _feedbackText;
    public Button NextQuestionButton => _nextQuestionButton;
    public Button StartLevelBtn => _startLevelBtn;
    public VisualElement OptionsContainer => _optionsContainer;

    private void OnEnable()
    {
        if (problemMaster == null) problemMaster = GetComponent<TextProblemMaster>();
        if (problemUIManager == null) problemUIManager = GetComponent<TextProblemUIManager>();
        Generate();
        problemUIManager.Initiation(this);
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
            return;
        Generate();
    }

    private void Generate()
    {
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument not assigned.");
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;
        root.Clear();

        if (uiStyleSheet != null && !root.styleSheets.Contains(uiStyleSheet))
            root.styleSheets.Add(uiStyleSheet);


        _container = new VisualElement { name = "Container" };
        _container.AddToClassList("container");

        _heading = new Label("Text Quiz") { name = "Heading" };
        _heading.AddToClassList("Question");

        _levelState = new VisualElement { name = "LevelState" };
        _levelState.AddToClassList("level-state");

        _timerLabel = new Label("Timer") { name = "Timer" };
        _timerLabel.AddToClassList("Question");

        _currQuestionLabel = new Label("Question #") { name = "CurrQuestion" };
        _currQuestionLabel.AddToClassList("Question");

        _levelState.Add(_timerLabel);
        _levelState.Add(_currQuestionLabel);

        _questionText = new Label("Question text will appear here") { name = "QuestionText" };
        _questionText.AddToClassList("Question");

        _optionsContainer = new VisualElement { name = "OptionsContainer" };
        _optionsContainer.AddToClassList("options-container");

        _feedbackText = new Label("") { name = "FeedbackText" };
        _feedbackText.AddToClassList("Question");

        _nextQuestionButton = new Button { text = "Next Question", name = "NextQuestion" };
        _nextQuestionButton.AddToClassList("CommonButton");

        _startLevelBtn = new Button { text = "Start Level", name = "StartLevel" };
        _startLevelBtn.AddToClassList("CommonButton");

        _container.Add(_heading);
        _container.Add(_levelState);
        _container.Add(_questionText);
        _container.Add(_optionsContainer);
        _container.Add(_feedbackText);
        _container.Add(_nextQuestionButton);
        _container.Add(_startLevelBtn);

        root.Add(_container);
    }

    /// <summary>
    /// Populates option buttons dynamically from TextQuestion.
    /// </summary>
    public void DisplayOptions(List<string> options, System.Action<int> onOptionSelected)
    {
        _optionsContainer.Clear();
        _optionButtons.Clear();

        for (int i = 0; i < options.Count; i++)
        {
            int index = i;
            Button optionButton = new Button { text = options[i], name = $"Option_{i}" };
            optionButton.AddToClassList("option-button");
            optionButton.clicked += () => onOptionSelected?.Invoke(index);
            _optionsContainer.Add(optionButton);
            _optionButtons.Add(optionButton);
        }
    }

    public void DisableOptions()
    {
        if(_optionButtons == null) return;
        foreach (var button in _optionButtons)
        {
            button.SetEnabled(false);
        }
    }

    public void SetFeedback(string text, bool isCorrect)
    {
        _feedbackText.text = text;
        _feedbackText.style.color = isCorrect ? Color.green : Color.red;
    }

    public void HighlightCorrectOption(int correctIndex)
    {
        for (int i = 0; i < _optionButtons.Count; i++)
        {
            _optionButtons[i].style.backgroundColor = (i == correctIndex)
                ? new StyleColor(Color.green)
                : new StyleColor(Color.red);
        }
    }
}
