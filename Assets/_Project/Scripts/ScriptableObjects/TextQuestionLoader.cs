using ClosedXML.Excel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "TextQuestionLoader", menuName = "Question Loaders/Text Question Loader")]
public class TextQuestionLoader : ScriptableObject
{
    [Header("Default Question Set (for Testing)")]
    [SerializeField] public List<TextQuestion> defaultQuestions = new List<TextQuestion>();

    /// <summary>
    /// Asynchronously loads text-based questions for the given difficulty and level.
    /// </summary>
    public async Task<List<TextQuestion>> LoadTextQuestionsForLevel(int difficulty, int level)
    {
        // Simulate async operation (e.g., reading from JSON or Firebase)
        await Task.Delay(100);

        // You can later expand this to filter by difficulty/level or load from files
        if (defaultQuestions == null || defaultQuestions.Count == 0)
        {
            Debug.LogWarning("No default questions found, using fallback examples.");
            return GetFallbackQuestions();
        }

        return new List<TextQuestion>(defaultQuestions);
    }

    private List<TextQuestion> GetFallbackQuestions()
    {
        return new List<TextQuestion>
        {
            new TextQuestion
            {
                QuestionText = "Which planet is known as the Red Planet?",
                Options = new List<string> { "Earth", "Mars", "Jupiter", "Saturn" },
                CorrectOptionIndex = 1
            },
            new TextQuestion
            {
                QuestionText = "What is the capital of France?",
                Options = new List<string> { "Berlin", "Paris", "Rome", "Madrid" },
                CorrectOptionIndex = 1
            },
            new TextQuestion
            {
                QuestionText = "Which gas do plants absorb during photosynthesis?",
                Options = new List<string> { "Oxygen", "Carbon Dioxide", "Nitrogen", "Hydrogen" },
                CorrectOptionIndex = 1
            }
        };
    }
}

[System.Serializable]
public class QuizQuestion
{
    public int QuestionNumber;
    public string Question;
    public string OptionA;
    public string OptionB;
    public string OptionC;
    public string OptionD;
    public string Answer;
    public string LevelName;
}

#if UNITY_EDITOR
[CustomEditor(typeof(TextQuestionLoader))]
public class TextQuestionLoaderEditor : Editor
{
    public string _savePath;

    private ExcelQuizLoader excelloader;

    // --- Editor Preferences Key for saving the path ---
    private const string SAVE_PATH_PREF_KEY = "TextQuestionLoadercEditor_SavePath";
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        TextQuestionLoader loader = (TextQuestionLoader)target;
        if (GUILayout.Button("Load Sample Questions"))
        {
            var questions = loader.LoadTextQuestionsForLevel(1, 1).Result;
            Debug.Log($"Loaded {questions.Count} questions.");
            loader.defaultQuestions = questions;
            foreach (var q in questions)
            {
                Debug.Log(q.ToString());
            }
        }

        // Horizontal layout for the path and button
        using (new EditorGUILayout.HorizontalScope())
        {
            // Display the current path in a disabled field
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("SO Save Folder", _savePath);
            EditorGUI.EndDisabledGroup();

            // The "Browse..." button
            if (GUILayout.Button("Browse...", GUILayout.Width(80)))
            {
                // Open the folder panel
                string absolutePath = EditorUtility.OpenFilePanel("Select excel file", Application.dataPath, "xlsx");

                // Check if the user selected a folder and didn't cancel
                if (!string.IsNullOrEmpty(absolutePath))
                {
                    // CRITICAL: Convert the absolute path to a Unity-relative path (starts with "Assets")
                    if (absolutePath.StartsWith(Application.dataPath))
                    {
                        _savePath = "Assets" + absolutePath.Substring(Application.dataPath.Length);
                        EditorPrefs.SetString(SAVE_PATH_PREF_KEY, _savePath); // Save the new path
                    }
                    else
                    {
                        Debug.LogWarning("Selected folder must be inside the project's 'Assets' directory.");
                    }
                }
            }
        }

        if (GUILayout.Button("Store Excel Questions to TextQuestion SO"))
        {
            if (string.IsNullOrEmpty(_savePath))
            {
                Debug.LogError("Please select a valid SO save folder first.");
                return;
            }
            excelloader = new ExcelQuizLoader();
            excelloader.filePath = _savePath;
            var questions = excelloader.LoadQuestions();
            // Create a new TextQuestionLoader SO
            //TextQuestionLoader textLoaderSO = ScriptableObject.CreateInstance<TextQuestionLoader>();
            loader.defaultQuestions = new List<TextQuestion>();
            // Convert QuizQuestion to TextQuestion and add to the SO
            foreach (var q in questions)
            {
                TextQuestion tq = new TextQuestion
                {
                    QuestionText = q.Question,
                    Options = new List<string> { q.OptionA, q.OptionB, q.OptionC, q.OptionD },
                    CorrectOptionIndex = q.Answer switch
                    {
                        "A" => 0,
                        "B" => 1,
                        "C" => 2,
                        "D" => 3,
                        _ => -1
                    }
                };
                loader.defaultQuestions.Add(tq);
            }

            EditorUtility.SetDirty(loader);
            AssetDatabase.SaveAssets();
            // Save the SO asset
            //    string assetPath = Path.Combine(_savePath, "ImportedTextQuestions.asset");
            //    AssetDatabase.CreateAsset(textLoaderSO, assetPath);
            //    AssetDatabase.SaveAssets();
            //    Debug.Log($"TextQuestionLoader SO created at: {assetPath} with {textLoaderSO.defaultQuestions.Count} questions.");
        }
    }

    private void StoreExcelToTextQuestions()
    {

    }
} 
#endif

public class ExcelQuizLoader
{
    public string filePath = "Assets/Questions.xlsx";
    public List<QuizQuestion> Questions { get; private set; }

    void Start()
    {
        LoadQuestions();
    }

    public List<QuizQuestion> LoadQuestions()
    {
        Questions = new List<QuizQuestion>();

        using (var workbook = new XLWorkbook(filePath))
        {
            var sheet = workbook.Worksheet(1);
            foreach (var row in sheet.RowsUsed().Skip(1)) // Skip header
            {
                var q = new QuizQuestion
                {
                    QuestionNumber = row.Cell(1).GetValue<int>(),
                    Question = row.Cell(2).GetValue<string>(),
                    OptionA = row.Cell(3).GetValue<string>(),
                    OptionB = row.Cell(4).GetValue<string>(),
                    OptionC = row.Cell(5).GetValue<string>(),
                    OptionD = row.Cell(6).GetValue<string>(),
                    Answer = row.Cell(7).GetValue<string>()
                };
                Questions.Add(q);
            }
        }

        Debug.Log($"Loaded {Questions.Count} questions from Excel.");
        return Questions;
    }

    public static List<List<QuizQuestion>> LoadQuizLevelQuestionsFromExcel(string path)
    {
        List<List<QuizQuestion>> levelQuestions = new List<List<QuizQuestion>>();
        using (var workbook = new XLWorkbook(path))
        {
            foreach (var sheet in workbook.Worksheets)
            {
                var questions = new List<QuizQuestion>();
                foreach (var row in sheet.RowsUsed().Skip(1)) // Skip header
                {
                    var q = new QuizQuestion
                    {
                        QuestionNumber = row.Cell(1).GetValue<int>(),
                        Question = row.Cell(2).GetValue<string>(),
                        OptionA = row.Cell(3).GetValue<string>(),
                        OptionB = row.Cell(4).GetValue<string>(),
                        OptionC = row.Cell(5).GetValue<string>(),
                        OptionD = row.Cell(6).GetValue<string>(),
                        Answer = row.Cell(7).GetValue<string>(),
                        LevelName = sheet.Name
                    };
                    questions.Add(q);
                }
                levelQuestions.Add(questions);
            }
        }
            Debug.Log($"Loaded questions for {levelQuestions.Count} levels from Excel.");
        return levelQuestions;
    }
}
