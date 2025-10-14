using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "TextQuestionLoader", menuName = "Question Loaders/Text Question Loader")]
public class TextQuestionLoader : ScriptableObject
{
    [Header("Default Question Set (for Testing)")]
    [SerializeField] private List<TextQuestion> defaultQuestions = new List<TextQuestion>();

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
