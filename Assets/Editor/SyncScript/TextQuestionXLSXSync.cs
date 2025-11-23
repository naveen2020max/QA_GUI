using DocumentFormat.OpenXml.Office.SpreadSheetML.Y2023.MsForms;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;

[System.Serializable]
public struct ExcelQuestionData
{
    public string ExcelName; // optional, for identifying
    public List<List<TextQuestion>> Questions;
}

public class TextQuestionXLSXSync
{
    public string xlsxFilePath;
    public string xlsxParentFolderPath;

    private const string SAVE_PATH_PREF_KEY = "TextQuestionXLSXLoadercEditor_SavePath";

    public bool IsPathSet => !string.IsNullOrEmpty(xlsxFilePath);
    public bool IsParentPathSet => !string.IsNullOrEmpty(xlsxParentFolderPath);
    public void GetXLSXPath()
    {
        // Horizontal layout for the path and button
        using (new EditorGUILayout.HorizontalScope())
        {
            // Display the current path in a disabled field
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Get Excel File", xlsxFilePath);
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
                        xlsxFilePath = "Assets" + absolutePath.Substring(Application.dataPath.Length);
                        Debug.Log(xlsxFilePath);
                        EditorPrefs.SetString(SAVE_PATH_PREF_KEY, xlsxFilePath); // Save the new path
                    }
                    else
                    {
                        Debug.LogWarning("Selected folder must be inside the project's 'Assets' directory.");
                    }
                }
            }
        }
    }

    public string GetXLSXParentFolderPath()
    {
        // Horizontal layout for the path and button
        using (new EditorGUILayout.HorizontalScope())
        {
            // Display the current path in a disabled field
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Get Excel Parent Folder", xlsxParentFolderPath);
            EditorGUI.EndDisabledGroup();

            // The "Browse..." button
            if (GUILayout.Button("Browse...", GUILayout.Width(80)))
            {
                // Open the folder panel
                string absolutePath = EditorUtility.OpenFolderPanel("Select excel file", Application.dataPath, "xlsx Parent");

                // Check if the user selected a folder and didn't cancel
                if (!string.IsNullOrEmpty(absolutePath))
                {
                    // CRITICAL: Convert the absolute path to a Unity-relative path (starts with "Assets")
                    if (absolutePath.StartsWith(Application.dataPath))
                    {
                        xlsxParentFolderPath = "Assets" + absolutePath.Substring(Application.dataPath.Length);
                        Debug.Log(xlsxParentFolderPath);
                        EditorPrefs.SetString(SAVE_PATH_PREF_KEY, xlsxParentFolderPath); // Save the new path
                        return xlsxParentFolderPath;
                    }
                    else
                    {
                        Debug.LogWarning("Selected folder must be inside the project's 'Assets' directory.");
                    }
                }
            }
        }
        return xlsxParentFolderPath;
    }

    public List<List<TextQuestion>> GetTextQuestionFromXLSX()
    {
        //var questionLists = ExcelQuizLoader.LoadQuizLevelQuestionsFromExcel(xlsxFilePath);

        return GetTextQuestionFromXLSX(xlsxFilePath);
    }

    public List<List<TextQuestion>> GetTextQuestionFromXLSX(string xlsxpath)
    {
        var questionLists = ExcelQuizLoader.LoadQuizLevelQuestionsFromExcel(xlsxpath);

        var textQuestions = new List<List<TextQuestion>>();

        foreach (var questions in questionLists)
        {
            var textQuestionList = new List<TextQuestion>();
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
                    },
                    LevelName = q.LevelName
                };
                textQuestionList.Add(tq);
            }
            textQuestions.Add(textQuestionList);
        }

        return textQuestions;
    }

    public List<ExcelQuestionData> LoadAllExcelQuestions(string folderPath)
    {
        var result = new List<ExcelQuestionData>();

        var excelFiles = Directory.GetFiles(folderPath, "*.xlsx", SearchOption.TopDirectoryOnly)
                                  .Where(f => !Path.GetFileName(f).StartsWith("~"))
                                  .ToList();

        foreach (var excelPath in excelFiles)
        {
            var questions = GetTextQuestionFromXLSX(excelPath);

            ExcelQuestionData data = new ExcelQuestionData
            {
                ExcelName = Path.GetFileNameWithoutExtension(excelPath),
                Questions = questions
            };

            result.Add(data);
        }

        return result;
    }
}
