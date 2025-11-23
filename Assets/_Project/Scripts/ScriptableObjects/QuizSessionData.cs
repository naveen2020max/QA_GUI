using UnityEngine;

[CreateAssetMenu(fileName = "QuizSessionData", menuName = "SessionData/QuizSessionData")]
public class QuizSessionData : ScriptableObject
{
    public TextQuestionLoader currentQuestionLoader;
}
