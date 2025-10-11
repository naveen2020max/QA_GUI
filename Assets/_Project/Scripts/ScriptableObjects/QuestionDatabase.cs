// QuestionDatabase.cs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Question Database", menuName = "Math Questions/Question Database")]
public class QuestionDatabase : ScriptableObject
{
    public List<QuestionData> questions;
}