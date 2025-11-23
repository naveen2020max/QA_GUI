using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TextQuestionDatabase", menuName = "Text Questions/Question Database")]
public class TextQuestionDatabase : ScriptableObject
{
    public List<TextQuestionLoader> textQuestionLoaders;
}
