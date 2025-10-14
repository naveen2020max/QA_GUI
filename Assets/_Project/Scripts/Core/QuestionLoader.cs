using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class QuestionLoader : MonoBehaviour
{
    private IQuestionProvider _offlineProvider;
    private FirebaseQuestionProvider _firebaseProvider;

    private void Awake()
    {
        _offlineProvider = GetComponent<OfflineQuestionProvider>();
        _firebaseProvider = GetComponent<FirebaseQuestionProvider>();
    }

    public async Task<List<MathProblem>> LoadQuestionsForLevel(string symbol, int difficulty, int level)
    {
        Debug.Log("---Starting Level Load");

        //List<MathProblem> questions = await _firebaseProvider.GetQuestionsForLevelAsync(symbol, difficulty, level);
        List<MathProblem> questions = null;
        if(questions == null)
        {
            questions = await _offlineProvider.GetQuestionsForLevelAsync(symbol, difficulty, level);
        }
        if (questions == null || questions.Count == 0)
        {
            Debug.LogError($"CRITICAL: Failed to load any questions for {symbol} D{difficulty} L{level} from any source.");
            return null;
        }

        Debug.Log($"---Loaded {questions.Count} questions for {symbol} D{difficulty} L{level}");

        return questions;
    }

}
