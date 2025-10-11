using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class OfflineQuestionProvider : MonoBehaviour, IQuestionProvider
{
    [SerializeField] private QuestionDatabase offlineQuestionDatabase;
    public Task<List<MathProblem>> GetQuestionsForLevelAsync(string symbol, int difficulty, int level)
    {
        if(offlineQuestionDatabase == null)
        {
            Debug.LogError("Offline Question Database is no assigned");
            return Task.FromResult<List<MathProblem>>(null);
        }

        // Filter questions based on the provided criteria.
        var filteredQuestions = offlineQuestionDatabase.questions
            .Where(q => q.symbol == symbol && q.difficulty == difficulty && q.level == level)
            .ToList();

        var mathProblems = new List<MathProblem>();
        foreach (var qdata in filteredQuestions)
        {
            mathProblems.Add(new MathProblem(qdata.number1, qdata.number2, qdata.symbol[0]));
        }
        Debug.Log($"Loaded {mathProblems.Count} questions from OFFLINE provider.");

        return Task.FromResult(mathProblems);
    }


}
