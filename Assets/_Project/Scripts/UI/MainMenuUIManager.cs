using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuUIManager : MonoBehaviour
{
    public UIDocument uiDocument;
    public VisualTreeAsset mainMenuAsset;
    public VisualTreeAsset levelSelectAsset;
    public TextQuestionDatabase questionDatabase;
    public QuizSessionData quizSessionData;

    private VisualElement root;
    private VisualElement mainMenuRoot;
    private VisualElement levelSelectRoot;

    void Start()
    {
        root = uiDocument.rootVisualElement;

        // Load and attach the Main Menu by default
        ShowMainMenu();
    }

    void ShowMainMenu()
    {
        root.Clear();
        mainMenuRoot = mainMenuAsset.Instantiate();
        root.Add(mainMenuRoot);

        var startButton = mainMenuRoot.Q<Button>("StartButton");
        var quitButton = mainMenuRoot.Q<Button>("QuitButton");

        startButton.clicked += ShowLevelSelect;
        quitButton.clicked += Application.Quit;
    }

    void ShowLevelSelect()
    {
        root.Clear();
        levelSelectRoot = levelSelectAsset.Instantiate();
        root.Add(levelSelectRoot);

        var levelList = levelSelectRoot.Q<ScrollView>("LevelList");
        var backButton = levelSelectRoot.Q<Button>("BackButton");

        backButton.clicked += ShowMainMenu;

        // Generate level buttons from ScriptableObject
        for (int i = 0; i < questionDatabase.textQuestionLoaders.Count; i++)
        {
            var loader = questionDatabase.textQuestionLoaders[i];
            var levelButton = new Button { text = $"Level {i + 1}: {loader.name}" };
            levelButton.AddToClassList("common-button");

            int index = i;
            levelButton.clicked += () => OnLevelSelected(index);

            levelList.Add(levelButton);
        }
    }

    void OnLevelSelected(int index)
    {
        Debug.Log($"Selected level: {index}");
        // Later: transition to Quiz screen or load quiz UI dynamically
        quizSessionData.currentQuestionLoader = questionDatabase.textQuestionLoaders[index];
        SceneManager.LoadScene("QuizMainTesing");
    }
}
