using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TextProblemMaster : ProblemMaster<TextQuestion, TextAnswer, TextResult>,
                                    ITextAnswerHandler<TextAnswer, TextResult>
{
    [SerializeField] private TextQuestionLoader _textLoader;

    protected override async Task<List<TextQuestion>> LoadQuestionsAsync(int difficulty, int level)
    {
        return await _textLoader.LoadTextQuestionsForLevel(difficulty, level);
    }

    protected override void ProcessProblem(TextQuestion problem)
    {
        Debug.Log($"Processing text question: {problem.QuestionText}");
    }

    protected override TextResult CreateResult(float userChoiceIndex)
    {
        int selected = Mathf.RoundToInt(userChoiceIndex);
        var question = _currentLevelQuestions[currentQuestionNumber];
        return new TextResult
        {
            Question = question,
            Answer = new TextAnswer
            {
                Question = question,
                SelectedOptionIndex = selected
            }
        };
    }

    private async void Start()
    {
        await StartLevel(1, 1);
    }

    public TextResult RecordResult(int selectedOptionIndex)
    {
        var question = _currentLevelQuestions[currentQuestionNumber];
        var result = new TextResult(question, selectedOptionIndex);
        TriggerOnResultRecorded(result);
        return result;
    }

}

[Serializable]
public class TextQuestion
{
    [TextArea]
    public string QuestionText;

    public List<string> Options = new List<string>(); // e.g. ["Paris", "Rome", "Berlin"]
    public int CorrectOptionIndex;                    // 0-based index of correct answer

    public override string ToString()
    {
        string opts = string.Join(", ", Options);
        return $"{QuestionText} (Options: {opts}, Correct: {Options[CorrectOptionIndex]})";
    }
}

[Serializable]
public struct TextAnswer
{
    public TextQuestion Question;
    public int SelectedOptionIndex;

    public bool IsCorrect => SelectedOptionIndex == Question.CorrectOptionIndex;

    public override string ToString()
    {
        string selected = (Question != null && SelectedOptionIndex < Question.Options.Count)
            ? Question.Options[SelectedOptionIndex]
            : "Invalid Option";

        string correct = (Question != null && Question.CorrectOptionIndex < Question.Options.Count)
            ? Question.Options[Question.CorrectOptionIndex]
            : "Unknown";

        return $"Q: {Question.QuestionText}\n" +
               $"Selected: {selected}\n" +
               $"Correct: {correct}\n" +
               $"Result: {(IsCorrect ? "✅ Correct" : "❌ Incorrect")}";
    }
}

[System.Serializable]
public struct TextResult : IResultTypeProvider
{
    public TextQuestion Question;
    public TextAnswer Answer;
    public bool IsAnsweredCorrect => Answer.IsCorrect;

    public TextResult(TextQuestion question, int selectedOptionIndex)
    {
        Question = question;
        Answer = new TextAnswer
        {
            Question = question,
            SelectedOptionIndex = selectedOptionIndex
        };
    }

    public override string ToString()
    {
        return Answer.ToString();
    }

    public ResultType GetResultType()
    {
        return IsAnsweredCorrect ? ResultType.Correct : ResultType.Incorrect;
    }
}
