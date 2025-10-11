// QuestionData.cs
using UnityEngine;

// The [CreateAssetMenu] attribute is now less important, as our editor will create these.
// But it can be useful for manual one-off creations.
[CreateAssetMenu(fileName = "New Question", menuName = "Math Questions/Question Data")]
public class QuestionData : ScriptableObject
{
    [Header("Question Identity")]
    public string questionId; // e.g., "add_d1_l1_q1"
    public string symbol;     // e.g., "+"
    public int difficulty;    // e.g., 1
    public int level;         // e.g., 1

    [Header("Problem Data")]
    public float number1;
    public float number2;

    // We can hide the operator since it's stored in the 'symbol' string.
    public char Operator => symbol.Length > 0 ? symbol[0] : '?';
}