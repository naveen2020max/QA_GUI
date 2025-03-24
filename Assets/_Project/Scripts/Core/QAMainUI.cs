using UnityEngine;
using UnityEngine.UIElements;

public class QAMainUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private StyleSheet uiStyleSheet;

    private VisualElement _container;
    private Label _heading;
    private Label _timerLabel;
    private Label _currQuestionLabel;
    private Label _questionText;
    private Label _answerText;
    private FloatField _answerField;
    private Button _submitButton;
    private Label _feedbackText;
    private Button _nextQuestionButton;

    private void Start()
    {
        Generate();
    }

    private void OnValidate()
    {
        if(Application.isPlaying)
        {
            return;
        }
        Generate();
    }

    private void Generate()
    {
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument is missing. Please assign it in the Inspector.");
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;

        // Apply stylesheet
        if (uiStyleSheet != null)
        {
            root.styleSheets.Add(uiStyleSheet);
        }

        // Main container
        _container = new VisualElement
        {
            name = "Container"
        };
        _container.style.flexGrow = 1;
        _container.style.backgroundColor = new Color(1f, 1f, 1f);
        _container.style.width = Length.Percent(75);
        _container.style.height = Length.Percent(100);
        _container.style.flexDirection = FlexDirection.Column;
        _container.style.paddingTop = 20;
        _container.style.paddingBottom = 20;
        _container.style.paddingLeft = 20;
        _container.style.paddingRight = 20;

        // Heading Label
        _heading = new Label("Addition +")
        {
            name = "Heading"
        };
        _heading.AddToClassList("Question");

        // LevelState container
        VisualElement levelState = new VisualElement
        {
            name = "LevelState"
        };
        levelState.style.flexGrow = 0;
        levelState.style.height = Length.Percent(5);
        levelState.style.width = Length.Percent(100);
        levelState.style.flexDirection = FlexDirection.Row;
        levelState.style.justifyContent = Justify.SpaceBetween;
        levelState.style.alignItems = Align.Center;

        // Timer Label
        _timerLabel = new Label("Time: 00:00")
        {
            name = "Timer"
        };
        _timerLabel.AddToClassList("Question");

        // Current Question Label
        _currQuestionLabel = new Label("Question: 1")
        {
            name = "CurrQuestion"
        };
        _currQuestionLabel.AddToClassList("Question");

        // Add timer and question number to LevelState
        levelState.Add(_timerLabel);
        levelState.Add(_currQuestionLabel);

        // Question Text
        _questionText = new Label("What is 5 + 3?")
        {
            name = "QuestionText"
        };
        _questionText.AddToClassList("Question");

        // Answer Text
        _answerText = new Label("Your Answer:")
        {
            name = "AnswerText"
        };
        _answerText.AddToClassList("Question");

        // Answer Field
        _answerField = new FloatField
        {
            name = "AnswerField"
        };
        _answerField.style.width = 370;
        _answerField.style.height = 140;
        _answerField.style.alignSelf = Align.Center;

        // Submit Button
        _submitButton = new Button
        {
            text = "Submit",
            name = "Submit"
        };
        _submitButton.AddToClassList("CommonButton");

        // Feedback Text
        _feedbackText = new Label("")
        {
            name = "FeedbackText"
        };
        _feedbackText.style.alignSelf = Align.Center;
        _feedbackText.style.visibility = Visibility.Hidden;

        // Next Question Button
        _nextQuestionButton = new Button
        {
            text = "Next Question",
            name = "NextQuestion"
        };
        _nextQuestionButton.AddToClassList("CommonButton");
        _nextQuestionButton.style.display = DisplayStyle.None;

        // Add elements to container
        _container.Add(_heading);
        _container.Add(levelState);
        _container.Add(_questionText);
        _container.Add(_answerText);
        _container.Add(_answerField);
        _container.Add(_submitButton);
        _container.Add(_feedbackText);
        _container.Add(_nextQuestionButton);

        // Add container to the root UI
        root.Add(_container);

        // Event handlers
        _submitButton.clicked += OnSubmitClicked;
        _nextQuestionButton.clicked += OnNextQuestionClicked;
    }

    private void OnSubmitClicked()
    {
        Debug.Log($"Submitted Answer: {_answerField.value}");
        _feedbackText.text = "Checking...";
        _feedbackText.style.visibility = Visibility.Visible;
    }

    private void OnNextQuestionClicked()
    {
        Debug.Log("Next question triggered.");
        _feedbackText.style.visibility = Visibility.Hidden;
        _nextQuestionButton.style.display = DisplayStyle.None;
    }
}
