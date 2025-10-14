using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement; // Needed for potential Retry/Next Level actions

[RequireComponent(typeof(UIDocument))]
public class LevelCompleteUI : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField][Range(0, 1)] private float passThresholdPercentage = 0.7f; // 70% needed to pass
    [SerializeField] private string nextLevelSceneName = "NextLevelScene"; // Configure in inspector
    [SerializeField] private string mainMenuSceneName = "MainMenuScene"; // Configure in inspector


    [Header("References")]
    [SerializeField] private MathProblemMaster problemMaster;
    [SerializeField] private StyleSheet levelCompleteStyleSheet; // Assign your USS file here

    private VisualElement rootElement;
    private VisualElement container;

    // UI Elements inside the screen
    private Label titleLabel;
    private Label totalQuestionsLabel;
    private Label correctAnswersLabel;
    private Label accuracyLabel;
    private Label statusLabel;
    private Label remarksLabel;
    private Button nextLevelButton;
    private Button retryButton;
    private Button mainMenuButton; // Optional: Always available button


    void Awake()
    {
        rootElement = GetComponent<UIDocument>().rootVisualElement;

        // Ensure ProblemMaster is assigned (you might have a central manager)
        if (problemMaster == null)
        {
            Debug.LogError("ProblemMaster not assigned to LevelCompleteUIManager!");
            // Try to find it if not assigned, or handle error appropriately
            problemMaster = FindFirstObjectByType<MathProblemMaster>();
            if (problemMaster == null)
            {
                this.enabled = false; // Disable script if critical dependency missing
                return;
            }
        }
    }

    void OnEnable()
    {
        if (problemMaster != null)
        {
            problemMaster.OnLevelComplete += HandleLevelComplete;
        }
        // Initially hide UI if it exists from a previous run (or build on demand)
        if (container != null)
        {
            container.style.display = DisplayStyle.None;
        }

        //HandleLevelComplete();
    }

    void OnDisable()
    {
        if (problemMaster != null)
        {
            problemMaster.OnLevelComplete -= HandleLevelComplete;
        }
        // Clean up button listeners if the object persists between scenes
        if (nextLevelButton != null) nextLevelButton.clicked -= OnNextLevelClicked;
        if (retryButton != null) retryButton.clicked -= OnRetryClicked;
        if (mainMenuButton != null) mainMenuButton.clicked -= OnMainMenuClicked;
    }

    private void HandleLevelComplete()
    {
        // Get results from ProblemMaster
        LevelResultInfo results = problemMaster.CalculateLevelResults(r => r.IsAnsweredCorrect);

        // Build the UI if it hasn't been built yet
        if (container == null)
        {
            BuildLevelCompleteUI();
        }

        // Populate the UI
        PopulateUI(results);

        // Make the screen visible
        container.style.display = DisplayStyle.Flex;

        // Optional: Pause the game if not already paused
        //Time.timeScale = 0f;
    }

    private void BuildLevelCompleteUI()
    {
        container = new VisualElement { name = "LevelCompleteContainer" };
        container.AddToClassList("level-complete-container");
        if (levelCompleteStyleSheet != null)
        {
            container.styleSheets.Add(levelCompleteStyleSheet);
        }

        titleLabel = new Label("Level Complete!") { name = "Title" };
        titleLabel.AddToClassList("level-complete-title");

        totalQuestionsLabel = new Label { name = "TotalQuestions" };
        totalQuestionsLabel.AddToClassList("level-complete-stat");

        correctAnswersLabel = new Label { name = "CorrectAnswers" };
        correctAnswersLabel.AddToClassList("level-complete-stat");

        accuracyLabel = new Label { name = "Accuracy" };
        accuracyLabel.AddToClassList("level-complete-stat");

        statusLabel = new Label { name = "Status" };
        statusLabel.AddToClassList("level-complete-status");

        remarksLabel = new Label { name = "Remarks" };
        remarksLabel.AddToClassList("level-complete-remarks");

        nextLevelButton = new Button { text = "Next Level", name = "NextLevelButton" };
        nextLevelButton.AddToClassList("level-complete-button");
        nextLevelButton.clicked += OnNextLevelClicked;

        retryButton = new Button { text = "Retry Level", name = "RetryButton" };
        retryButton.AddToClassList("level-complete-button");
        retryButton.clicked += OnRetryClicked;

        mainMenuButton = new Button { text = "Main Menu", name = "MainMenuButton" };
        mainMenuButton.AddToClassList("level-complete-button");
        mainMenuButton.clicked += OnMainMenuClicked;


        container.Add(titleLabel);
        container.Add(totalQuestionsLabel);
        container.Add(correctAnswersLabel);
        container.Add(accuracyLabel);
        container.Add(statusLabel);
        container.Add(remarksLabel);
        container.Add(nextLevelButton);
        container.Add(retryButton);
        container.Add(mainMenuButton); // Add the main menu button

        // Add to root and hide initially
        rootElement.Add(container);
        container.style.display = DisplayStyle.None;
    }

    private void PopulateUI(LevelResultInfo results)
    {
        totalQuestionsLabel.text = $"Total Questions: {results.MaxQuestionsInLevel}";
        correctAnswersLabel.text = $"Correct Answers: {results.CorrectAnswers}";

        float accuracy = 0f;
        if (results.MaxQuestionsInLevel > 0)
        {
            accuracy = (float)results.CorrectAnswers / results.MaxQuestionsInLevel;
        }
        accuracyLabel.text = $"Accuracy: {accuracy:P0}"; // P0 formats as percentage with 0 decimal places

        bool passed = accuracy >= passThresholdPercentage;

        if (passed)
        {
            statusLabel.text = "Status: Passed!";
            remarksLabel.text = "Excellent work!";
            statusLabel.AddToClassList("passed"); // Add class for styling
            statusLabel.RemoveFromClassList("failed");
            nextLevelButton.style.display = DisplayStyle.Flex; // Show Next Level
            retryButton.style.display = DisplayStyle.None;    // Hide Retry
        }
        else
        {
            statusLabel.text = "Status: Failed";
            remarksLabel.text = "Keep practicing!";
            statusLabel.AddToClassList("failed");   // Add class for styling
            statusLabel.RemoveFromClassList("passed");
            nextLevelButton.style.display = DisplayStyle.None;    // Hide Next Level
            retryButton.style.display = DisplayStyle.Flex; // Show Retry
        }
        mainMenuButton.style.display = DisplayStyle.Flex; // Always show main menu button
    }

    private void OnNextLevelClicked()
    {
        Debug.Log("Next Level button clicked");
        Time.timeScale = 1f; // Resume game time
        // Load the next level scene - Make sure it's in Build Settings!
        if (!string.IsNullOrEmpty(nextLevelSceneName))
        {
            SceneManager.LoadScene(nextLevelSceneName);
        }
        else
        {
            Debug.LogWarning("Next Level Scene Name not set in inspector!");
        }
    }

    private void OnRetryClicked()
    {
        Debug.Log("Retry button clicked");
        Time.timeScale = 1f; // Resume game time
        // Reload the current scene - Make sure it's in Build Settings!
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnMainMenuClicked()
    {
        Debug.Log("Main Menu button clicked");
        Time.timeScale = 1f; // Resume game time
                             // Load the main menu scene - Make sure it's in Build Settings!
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogWarning("Main Menu Scene Name not set in inspector!");
        }
    }
}