// Place this script in an "Editor" folder.
using DocumentFormat.OpenXml.Office.SpreadSheetML.Y2023.MsForms;
using DocumentFormat.OpenXml.Packaging;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq; // For OrderBy
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;


public class QuestionSyncEditor : EditorWindow
{
    #region Const
    // --- CONFIGURE THESE ---
    private const string SPREADSHEET_ID = "1bdgS3xluRYb3aSx-TMauZN_JgImMWQ5t0_9lWb8jM6U";
    private const string QUESTIONS_GRID_NAME = "Questions"; // The name of the sheet tab in Google Sheets
    private const string METADATA_GRID_NAME = "Metadata"; // The name of the metadata sheet tab
    private const string JSON_KEY_PATH = "Assets/Editor/Auth/speedmathsapp-58918c736a9b.json"; // Path to your secret service account key
    private const string SCRIPTABLE_OBJECT_PATH = "Assets/Questions/Database";
    private const string DATABASE_ASSET_NAME = "Generated_Question_Database.asset";
    private const string FIREBASE_QUESTIONS_COLLECTION = "questions";
    private const string FIREBASE_METADATA_COLLECTION = "metadata";
    // --------------------

    // --- Editor Preferences Key for saving the path ---
    private const string SAVE_PATH_PREF_KEY = "QuestionSyncEditor_SavePath";

    //Helper Class Object
    private TextQuestionXLSXSync xLSXSync = new TextQuestionXLSXSync();
    #endregion

    #region Classes
    // A temporary class to hold data parsed from the sheet
    private class QuestionRecord
    {
        public string questionId;
        public string symbol;
        public int difficulty;
        public int level;
        public float number1;
        public float number2;
    }
    #endregion

    #region UI Fields
    // --- UI FIELDS ---
    private string _status = "Ready.";
    private List<string> _logMessages = new List<string>(); // store all the status logs
    private Vector2 _scrollPosition, _logScrollPosition;
    private int _minLogWindowHeight = 50, _maxLogWindowHeight = 150;
    private string _minimizedLogButtonText = "Expand", _maximizedLogButtonText = "Minimize";
    private bool _isExpanded = false;
    private string _savePath = "Assets/Questions/Database"; // Store the path as a string 
    #endregion

    #region Init
    // This creates the menu item in the Unity Editor
    [MenuItem("My Tools/Question Sync Editor")]
    public static void ShowWindow()
    {
        GetWindow<QuestionSyncEditor>("Question Sync");
    }

    // Called when the window is enabled
    private void OnEnable()
    {
        // Load the saved path from EditorPrefs, with a default value
        _savePath = EditorPrefs.GetString(SAVE_PATH_PREF_KEY, "Assets/Questions/Database");
    }
    #endregion

    #region GUI
    // This method is called to draw the UI for the window
    void OnGUI()
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        EditorGUILayout.LabelField("Question Content Pipeline", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("This tool syncs data from a master Google Sheet. It updates both the local ScriptableObjects (for offline fallback) and the Firebase Firestore database (for live updates).", MessageType.Info);

        EditorGUILayout.Space(20);

        // --- NEW FOLDER BROWSER UI ---
        EditorGUILayout.LabelField("Save Location", EditorStyles.boldLabel);

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
                string absolutePath = EditorUtility.SaveFolderPanel("Select Save Folder for ScriptableObjects", Application.dataPath, "");

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

        EditorGUILayout.Space(20);

        EditorGUILayout.LabelField("Step 1: Sync to Local Project", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Pulls data from Google Sheets and creates/updates the ScriptableObject assets in your selected save folder.", MessageType.Info);
        if (GUILayout.Button("Sync Google Sheet to Local Assets", GUILayout.Height(40)))
        {
            // The async void pattern is safe to use for button clicks in editor scripts.
            // We don't need to await the result here, the async method will handle updating the status.
            if (string.IsNullOrEmpty(_savePath))
            {
                LogStatus("ERROR: Please select a 'SO Save Folder' before syncing.");
                EditorUtility.DisplayDialog("Error", "Save Folder is not set. Please use the 'Browse...' button to select a folder.", "OK");
            }
            else
            {
                _ = SyncFromGoogleSheetsToLocalAssets();
            }
        }

        // --- STEP 2: PUSH TO FIREBASE ---
        EditorGUILayout.LabelField("Step 2: Push to Live Server", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Takes the local ScriptableObject assets from the folder above and pushes them to the live Firebase database.", MessageType.Info);

        Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.6f, 1f, 0.6f); // Light green
        if (GUILayout.Button("Push Local Assets to Firebase", GUILayout.Height(40)))
        {

            if (string.IsNullOrEmpty(_savePath))
            {
                LogStatus("ERROR: Please select a 'SO Save Folder' before syncing.");
                EditorUtility.DisplayDialog("Error", "Save Folder is not set. Please use the 'Browse...' button to select a folder.", "OK");
            }
            else
            {
                _ = PushAssetsToFirebase();
            }
        }
        GUI.backgroundColor = originalColor;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Status:", EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.HelpBox(_status, MessageType.None, true);

        // --- LOG SECTION ---
        EditorGUILayout.LabelField("Logs:", EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.BeginVertical(GUI.skin.box);
        _logScrollPosition = EditorGUILayout.BeginScrollView(_logScrollPosition, GUILayout.Height(_isExpanded ? _maxLogWindowHeight : _minLogWindowHeight));
        foreach (var log in _logMessages)
        {
            EditorGUILayout.HelpBox(log, MessageType.None, true);
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.BeginHorizontal();
        if (_logMessages.Count > 0 && GUILayout.Button("Clear Logs"))
        {
            _logMessages.Clear();
        }
        if (GUILayout.Button(_isExpanded ? _maximizedLogButtonText : _minimizedLogButtonText))
        {
            _isExpanded = !_isExpanded;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        // --- NEW DANGER ZONE SECTION ---
        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); // Separator
        EditorGUILayout.LabelField("Danger Zone", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("This will delete all generated QuestionData assets and subfolders inside the selected save folder. The main database asset will be cleared but not deleted.", MessageType.Warning);

        // Change button color to red
        //Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(1f, 0.5f, 0.5f); // A light red

        if (GUILayout.Button("Delete All Generated Assets", GUILayout.Height(30)))
        {
            // Show a confirmation dialog before proceeding
            if (EditorUtility.DisplayDialog(
                "Confirm Deletion",
                "Are you sure you want to delete all generated question assets and subfolders from the selected directory?\n\nThis action CANNOT be undone.",
                "Yes, Delete Everything",
                "Cancel"))
            {
                DeleteAllGeneratedAssets();
            }
        }

        // Restore the original button color
        GUI.backgroundColor = originalColor;

        // Text Question from XLSX Sync UI
        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); // Separator
        EditorGUILayout.LabelField("XLXS Sync", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("This will store all the questions from xlsx file to scriptableObjects", MessageType.Warning);

        xLSXSync.GetXLSXPath();

        if(xLSXSync.IsPathSet && GUILayout.Button("Store Excel Questions to TextQuestion SO", GUILayout.Height(40)))
        {
            if (string.IsNullOrEmpty(_savePath))
            {
                Debug.LogError("Please select a valid SO save folder first.");
                return;
            }
            var questions = xLSXSync.GetTextQuestionFromXLSX();

            UpdateScriptableObject(questions);
        }

        //Multiple XLSX files
        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); // Separator
        EditorGUILayout.LabelField("Multiple XLSX files", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("This will store all the questions from xlsx file to scriptableObjects", MessageType.Warning);

        string pp = xLSXSync.GetXLSXParentFolderPath();

        if(xLSXSync.IsParentPathSet && GUILayout.Button("Store Multiple Excel Questions to TextQuestion SO", GUILayout.Height(40)))
        {
            if (string.IsNullOrEmpty(_savePath))
            {
                Debug.LogError("Please select a valid SO save folder first.");
                return;
            }
            var allQuestions = xLSXSync.LoadAllExcelQuestions(pp);
            foreach (var questions in allQuestions)
            {
                UpdateScriptableObject(questions.Questions);
            }

            //EditorCoroutineUtility.StartCoroutineOwnerless(ProcessAllExcelFiles(folderPath));
        }
        

        EditorGUILayout.EndScrollView();
    }

    public IEnumerator ProcessAllExcelFiles(string path)
    {
        var allQuestions = xLSXSync.LoadAllExcelQuestions(path);

        foreach (var questions in allQuestions)
        {
            yield return UpdateScriptableObjectCoroutine(questions.Questions, xLSXSync.xlsxParentFolderPath);
        }
    }
    #endregion

    #region GoogleSheet
    private async Task SyncFromGoogleSheetsToLocalAssets()
    {
        LogStatus("Step 1/5: Authenticating and Reading metadata from Google Sheets...");

        var metadata = await ReadMetadataFromSheetAsync();
        if (metadata == null)
        {
            LogStatus("ERROR: Failed to read metadata. Check the 'Metadata' sheet exists and is formatted correctly.");
            return;
        }
        LogStatus("Step 2/5: reading questions from Google Sheets...");

        // 1. Authenticate and get data from Google Sheets
        var records = await ReadQuestionSheetAsync();
        if (records == null || records.Count == 0)
        {
            LogStatus("ERROR: Failed to read questions from Google Sheets. Check console for details.");
            Repaint();
            return;
        }
        LogStatus($"Step 3/5: Successfully read {records.Count} questions from Google Sheets.");
        Repaint();

        // 2. Create/Update ScriptableObjects for offline use
        LogStatus("Step 4/5: Updating local ScriptableObject assets...");
        Repaint();
        UpdateScriptableObjects(records);
        LogStatus("Step 5/5: Local ScriptableObjects updated successfully.");
        Repaint();
        LogStatus($"SYNC COMPLETE! Version {metadata["version"]} with {records.Count} questions is now live.");
        Repaint();

        //// 3. Push data to Firebase for live updates
        //_status = "Step 3/3: Pushing data to Firebase Firestore...";
        //Repaint();
        //await UpdateFirebase(records);
        //_status = $"SYNC COMPLETE! {records.Count} questions are now live on Firebase and saved locally.";
        //Repaint();
    }

    // --- NEW: Method to read the Metadata sheet ---
    private async Task<Dictionary<string, object>> ReadMetadataFromSheetAsync()
    {
        try
        {
            var service = await GetSheetsServiceAsync();
            var request = service.Spreadsheets.Values.Get(SPREADSHEET_ID, METADATA_GRID_NAME);
            var response = await request.ExecuteAsync();
            var values = response.Values;

            if (values == null || values.Count <= 1)
            {
                Debug.LogError("No data found in Metadata sheet.");
                return null;
            }

            var metadata = new Dictionary<string, object>();
            for (int i = 1; i < values.Count; i++) // Skip header
            {
                var row = values[i];
                if (row.Count >= 2 && !string.IsNullOrEmpty(row[0].ToString()))
                {
                    metadata[row[0].ToString()] = row[1].ToString();
                }
            }

            // Automatically add/update the lastUpdated timestamp
            metadata["lastUpdated"] = DateTime.UtcNow.ToString("o"); // ISO 8601 format

            return metadata;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error reading Metadata sheet: {e.Message}");
            return null;
        }
    }

    private async Task<List<QuestionRecord>> ReadQuestionSheetAsync()
    {
        try
        {
            //GoogleCredential credential;
            //using (var stream = new FileStream(JSON_KEY_PATH, FileMode.Open, FileAccess.Read))
            //{
            //    credential = GoogleCredential.FromStream(stream).CreateScoped(SheetsService.Scope.SpreadsheetsReadonly);
            //}

            //var service = new SheetsService(new BaseClientService.Initializer()
            //{
            //    HttpClientInitializer = credential,
            //    ApplicationName = "Unity Math QA Sync",
            //});
            var servicee = await GetSheetsServiceAsync();
            var request = servicee.Spreadsheets.Values.Get(SPREADSHEET_ID, QUESTIONS_GRID_NAME);
            var response = await request.ExecuteAsync();
            var values = response.Values;

            if (values == null || values.Count <= 1)
            {
                Debug.LogError("No data found in Google Sheet or only header row exists.");
                return null;
            }

            var records = new List<QuestionRecord>();
            // Skip the header row (i=1)
            for (int i = 1; i < values.Count; i++)
            {
                var row = values[i];
                if (row.Count < 6) continue; // Skip malformed rows

                records.Add(new QuestionRecord
                {
                    questionId = row[0].ToString(),
                    symbol = row[1].ToString(),
                    difficulty = int.TryParse(row[2].ToString(), out var d) ? d : 0,
                    level = int.TryParse(row[3].ToString(), out var l) ? l : 0,
                    number1 = float.TryParse(row[4].ToString(), out var n1)? n1 : 0,
                    number2 = float.TryParse(row[5].ToString(), out var n2)? n2 : 0
                });
            }
            return records;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error reading Google Sheet: {e.Message}\n{e.StackTrace}");
            return null;
        }
    }
    // NEW: Helper method to avoid duplicating auth code
    private async Task<SheetsService> GetSheetsServiceAsync()
    {
        GoogleCredential credential;
        using (var stream = new FileStream(JSON_KEY_PATH, FileMode.Open, FileAccess.Read))
        {
            credential = await GoogleCredential.FromStreamAsync(stream, CancellationToken.None);
        }
        credential = credential.CreateScoped(SheetsService.Scope.Spreadsheets);

        return new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "Unity Math QA Sync",
        });
    }
    #endregion

    #region Firebase
    private static async Task<FirebaseApp> GetFirebaseAppAsync()
    {
        var app = FirebaseApp.DefaultInstance;
        if (app != null)
        {
            return app;
        }

        Debug.Log("Firebase App not found. Attempting to initialize...");
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            return FirebaseApp.DefaultInstance;
        }

        Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
        return null;
    }

    /// <summary>
    /// Workflow for Step 2: Reads local assets and pushes to Firebase.
    /// </summary>
    private async Task PushAssetsToFirebase()
    {
        var app = await GetFirebaseAppAsync();
        if (app == null)
        {
            LogStatus("ERROR: Firebase initialization failed. Check console for details.");
            Repaint();
            return;
        }

        // First, load the local database asset
        string dbPath = Path.Combine(_savePath, DATABASE_ASSET_NAME);
        QuestionDatabase mainDB = AssetDatabase.LoadAssetAtPath<QuestionDatabase>(dbPath);
        if (mainDB == null || mainDB.questions == null || mainDB.questions.Count == 0)
        {
            LogStatus("ERROR: Cannot push to Firebase. The local database asset is missing or empty. Please sync local assets first.");
            Repaint();
            return;
        }

        // We still fetch metadata from the sheet to get the correct version number
        LogStatus("Step 1/3: Reading latest metadata from Google Sheets...");
        Repaint();
        var metadata = await ReadMetadataFromSheetAsync();
        if (metadata == null)
        {
            LogStatus("ERROR: Failed to read metadata. Cannot push to Firebase without a version.");
            Repaint();
            return;
        }

        LogStatus("Step 2/3: Pushing metadata to Firebase...");
        Repaint();
        await UpdateMetadataInFirebaseAsync(metadata);

        LogStatus($"Step 3/3: Pushing {mainDB.questions.Count} local questions to Firebase...");
        Repaint();
        await UpdateQuestionsInFirebaseAsync(mainDB.questions); // Use the new override

        LogStatus($"FIREBASE PUSH COMPLETE! Version {metadata["version"]} is now live.");
        Repaint();
    }

    // This override now takes a list of QuestionData (from ScriptableObjects)
    private async Task UpdateQuestionsInFirebaseAsync(List<QuestionData> questionAssets)
    {
        var app = await GetFirebaseAppAsync();
        if (app == null) return;
        try
        {
            FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
            WriteBatch batch = db.StartBatch();

            foreach (var asset in questionAssets)
            {
                DocumentReference docRef = db.Collection(FIREBASE_QUESTIONS_COLLECTION).Document(asset.questionId);
                var data = new Dictionary<string, object>
                {
                    { "symbol", asset.symbol },
                    { "difficulty", asset.difficulty },
                    { "level", asset.level },
                    { "number1", asset.number1 },
                    { "number2", asset.number2 }
                };
                batch.Set(docRef, data);
            }
            await batch.CommitAsync();
            Debug.Log("Firebase questions updated successfully from local assets.");
        }
        catch (Exception e)
        {
            LogStatus($"ERROR: Failed to update Firebase questions. Check console.");
            Repaint();
            Debug.LogError($"Error updating Firebase questions: {e.Message}");
        }
    }

    // NEW: Method to write metadata to Firebase
    private async Task UpdateMetadataInFirebaseAsync(Dictionary<string, object> metadata)
    {
        var app = await GetFirebaseAppAsync();
        if (app == null) return;
        try
        {
            FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
            // We use a single document 'versionInfo' to hold all metadata
            DocumentReference docRef = db.Collection(FIREBASE_METADATA_COLLECTION).Document("versionInfo");
            await docRef.SetAsync(metadata);
            Debug.Log("Firebase metadata updated successfully.");
        }
        catch (Exception e)
        {
            LogStatus($"ERROR: Failed to update Firebase metadata. Check console.");
            Repaint();
            Debug.LogError($"Error updating Firebase metadata: {e.Message}");
        }
    }

    private async Task UpdateQuestionsInFirebaseAsync(List<QuestionRecord> records)
    {
        try
        {
            FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
            WriteBatch batch = db.StartBatch();

            foreach (var record in records)
            {
                DocumentReference docRef = db.Collection(FIREBASE_QUESTIONS_COLLECTION).Document(record.questionId);
                var data = new Dictionary<string, object>
                {
                    { "symbol", record.symbol },
                    { "difficulty", record.difficulty },
                    { "level", record.level },
                    { "number1", record.number1 },
                    { "number2", record.number2 }
                };
                batch.Set(docRef, data);
            }

            await batch.CommitAsync();
            Debug.Log("Firebase Firestore updated successfully.");
        }
        catch (System.Exception e)
        {
            LogStatus($"ERROR: Failed to update Firebase. Check console for details.");
            Repaint();
            Debug.LogError($"Error updating Firebase: {e.Message}\n{e.StackTrace}");
        }
    }
    #endregion

    #region Helper
    private void UpdateScriptableObjects(List<QuestionRecord> records)
    {
        if (!Directory.Exists(_savePath))
        {
            Directory.CreateDirectory(_savePath); // Create the directory if it doesn't exist
        }

        var allQuestionSOs = new List<QuestionData>();

        foreach (var record in records)
        {
            string SubfloderName = $"Difficulty_{record.difficulty}";
            string fullFolderPath = Path.Combine(_savePath, SubfloderName);

            Directory.CreateDirectory(fullFolderPath); // Ensure subfolder exists

            string assetPath = Path.Combine(fullFolderPath, $"{record.questionId}.asset");
            QuestionData questionSO = AssetDatabase.LoadAssetAtPath<QuestionData>(assetPath);

            if (questionSO == null)
            {
                questionSO = ScriptableObject.CreateInstance<QuestionData>();
                AssetDatabase.CreateAsset(questionSO, assetPath);
            }

            // Update the data
            questionSO.questionId = record.questionId;
            questionSO.symbol = record.symbol;
            questionSO.difficulty = record.difficulty;
            questionSO.level = record.level;
            questionSO.number1 = record.number1;
            questionSO.number2 = record.number2;

            EditorUtility.SetDirty(questionSO); // Mark the asset as changed
            allQuestionSOs.Add(questionSO);
        }

        // Now update the main database asset
        string dbPath = Path.Combine(_savePath, DATABASE_ASSET_NAME);
        QuestionDatabase mainDB = AssetDatabase.LoadAssetAtPath<QuestionDatabase>(dbPath);
        if (mainDB == null)
        {
            mainDB = ScriptableObject.CreateInstance<QuestionDatabase>();
            AssetDatabase.CreateAsset(mainDB, dbPath);
        }
        mainDB.questions = allQuestionSOs.OrderBy(q => q.questionId).ToList(); // Store sorted for consistency
        EditorUtility.SetDirty(mainDB);

        AssetDatabase.SaveAssets(); // Writes changes to disk
        AssetDatabase.Refresh();    // Reloads assets in Unity
    }
    
    private void UpdateScriptableObject(List<List<TextQuestion>> questionLists)
    {
        string filename = Path.GetFileNameWithoutExtension(xLSXSync.xlsxFilePath);
        string _thisExcelSavePath = Path.Combine(_savePath, filename);
        if (!Directory.Exists(_thisExcelSavePath))
        {
            Directory.CreateDirectory(_thisExcelSavePath); // Create the directory if it doesn't exist
        }

        var allTextQuestionSOs = new List<TextQuestionLoader>();
        // Create a new TextQuestionLoader SO
        foreach (var item in questionLists)
        {
            //string SubfloderName = $"LevelName_{item[0].LevelName}";
            //string fullFolderPath = Path.Combine(_thisExcelSavePath, SubfloderName);

            Directory.CreateDirectory(_thisExcelSavePath); // Ensure subfolder exists

            string assetPath = Path.Combine(_thisExcelSavePath, $"{item[0].LevelName}.asset");
            TextQuestionLoader loader = AssetDatabase.LoadAssetAtPath<TextQuestionLoader>(assetPath);
            if(loader == null)
            {
                loader = ScriptableObject.CreateInstance<TextQuestionLoader>();
                AssetDatabase.CreateAsset(loader, assetPath);
            }
            loader.defaultQuestions = new List<TextQuestion>();
            // Convert QuizQuestion to TextQuestion and add to the SO
            //foreach (var levelQuestions in item)
            //{
            //    loader.defaultQuestions.Add(levelQuestions);

            //}
            loader.defaultQuestions = item;
            // Save the TextQuestionLoader SO asset
            string soPath = Path.Combine(_savePath, "TextQuestionLoader.asset");
            EditorUtility.SetDirty(loader); // Mark the asset as changed
            allTextQuestionSOs.Add(loader);

        }

        // Now update the main database asset
        string dbPath = Path.Combine(_thisExcelSavePath, DATABASE_ASSET_NAME);
        TextQuestionDatabase mainDB = AssetDatabase.LoadAssetAtPath<TextQuestionDatabase>(dbPath);
        if (mainDB == null)
        {
            mainDB = ScriptableObject.CreateInstance<TextQuestionDatabase>();
            AssetDatabase.CreateAsset(mainDB, dbPath);
        }
        mainDB.textQuestionLoaders = allTextQuestionSOs; // Store sorted for consistency
        EditorUtility.SetDirty(mainDB);

        AssetDatabase.SaveAssets(); // Writes changes to disk
        AssetDatabase.Refresh();    // Reloads assets in Unity
        
        //Debug.Log($"Successfully saved {loader.defaultQuestions.Count} questions to '{soPath}'.");

    }

    private IEnumerator UpdateScriptableObjectsCoroutine(List<QuestionRecord> records)
    {
        if (!Directory.Exists(_savePath))
            Directory.CreateDirectory(_savePath);

        var allQuestionSOs = new List<QuestionData>();

        foreach (var record in records)
        {
            string SubfolderName = $"Difficulty_{record.difficulty}";
            string fullFolderPath = Path.Combine(_savePath, SubfolderName);

            Directory.CreateDirectory(fullFolderPath);

            string assetPath = Path.Combine(fullFolderPath, $"{record.questionId}.asset");
            QuestionData questionSO = AssetDatabase.LoadAssetAtPath<QuestionData>(assetPath);

            if (questionSO == null)
            {
                questionSO = ScriptableObject.CreateInstance<QuestionData>();
                AssetDatabase.CreateAsset(questionSO, assetPath);
            }

            questionSO.questionId = record.questionId;
            questionSO.symbol = record.symbol;
            questionSO.difficulty = record.difficulty;
            questionSO.level = record.level;
            questionSO.number1 = record.number1;
            questionSO.number2 = record.number2;

            EditorUtility.SetDirty(questionSO);
            allQuestionSOs.Add(questionSO);

            yield return null;  // Allow Unity to breathe
        }

        // Update main DB
        string dbPath = Path.Combine(_savePath, DATABASE_ASSET_NAME);
        QuestionDatabase mainDB = AssetDatabase.LoadAssetAtPath<QuestionDatabase>(dbPath);

        if (mainDB == null)
        {
            mainDB = ScriptableObject.CreateInstance<QuestionDatabase>();
            AssetDatabase.CreateAsset(mainDB, dbPath);
        }

        mainDB.questions = allQuestionSOs.OrderBy(q => q.questionId).ToList();
        EditorUtility.SetDirty(mainDB);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        yield return null;
    }

    private IEnumerator UpdateScriptableObjectCoroutine(List<List<TextQuestion>> questionLists, string excelPath)
    {
        string filename = Path.GetFileNameWithoutExtension(excelPath);
        string _thisExcelSavePath = Path.Combine(_savePath, filename);

        if (!Directory.Exists(_thisExcelSavePath))
            Directory.CreateDirectory(_thisExcelSavePath);

        var allTextQuestionSOs = new List<TextQuestionLoader>();

        int total = questionLists.Count;
        int processed = 0;

        foreach (var item in questionLists)
        {
            processed++;

            float progress = (float)processed / total;
            EditorUtility.DisplayProgressBar(
                $"Importing Excel: {filename}",
                $"Creating Level: {item[0].LevelName}",
                progress
            );

            Directory.CreateDirectory(_thisExcelSavePath);

            string assetPath = Path.Combine(_thisExcelSavePath, $"{item[0].LevelName}.asset");
            TextQuestionLoader loader = AssetDatabase.LoadAssetAtPath<TextQuestionLoader>(assetPath);

            if (loader == null)
            {
                loader = ScriptableObject.CreateInstance<TextQuestionLoader>();
                AssetDatabase.CreateAsset(loader, assetPath);
            }

            loader.defaultQuestions = item;
            EditorUtility.SetDirty(loader);

            allTextQuestionSOs.Add(loader);

            yield return null;
        }

        string dbPath = Path.Combine(_thisExcelSavePath, DATABASE_ASSET_NAME);
        TextQuestionDatabase mainDB = AssetDatabase.LoadAssetAtPath<TextQuestionDatabase>(dbPath);

        if (mainDB == null)
        {
            mainDB = ScriptableObject.CreateInstance<TextQuestionDatabase>();
            AssetDatabase.CreateAsset(mainDB, dbPath);
        }

        mainDB.textQuestionLoaders = allTextQuestionSOs;
        EditorUtility.SetDirty(mainDB);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.ClearProgressBar();
        yield return null;
    }

    private void LogStatus(string message)
    {
        _status = message;
        _logMessages.Add($"{_logMessages.Count + 1} - {message}");
        Repaint(); // Force the UI to update
    }

    /// <summary>
    /// Deletes all generated assets and subfolders from the configured save path.
    /// </summary>
    private void DeleteAllGeneratedAssets()
    {
        LogStatus("Deleting all generated assets...");
        Repaint();

        if (string.IsNullOrEmpty(_savePath) || !Directory.Exists(_savePath))
        {
            LogStatus("ERROR: Cannot delete because the save folder path is not valid.");
            Repaint();
            return;
        }

        // --- Step 1: Delete all subdirectories (like Difficulty_1, Difficulty_2) ---
        string[] subdirectories = Directory.GetDirectories(_savePath);
        foreach (string dir in subdirectories)
        {
            // AssetDatabase.DeleteAsset is the correct way to delete folders in Unity
            AssetDatabase.DeleteAsset(dir);
            Debug.Log($"Deleted folder: {dir}");
        }

        // --- Step 2: Delete loose .asset files, but spare the main database file ---
        string mainDbFullPath = Path.Combine(_savePath, DATABASE_ASSET_NAME).Replace('\\', '/');
        string[] assetFiles = Directory.GetFiles(_savePath, "*.asset");

        foreach (string file in assetFiles)
        {
            string formattedFile = file.Replace('\\', '/');
            if (formattedFile != mainDbFullPath)
            {
                AssetDatabase.DeleteAsset(formattedFile);
                Debug.Log($"Deleted asset file: {file}");
            }
        }

        // --- Step 3: Clear the list in the main database asset without deleting the file itself ---
        QuestionDatabase mainDB = AssetDatabase.LoadAssetAtPath<QuestionDatabase>(mainDbFullPath);
        if (mainDB != null)
        {
            mainDB.questions.Clear(); // Clear the list
            EditorUtility.SetDirty(mainDB); // Mark it as changed
            Debug.Log($"Cleared question list in '{DATABASE_ASSET_NAME}'.");
        }

        // --- Finalize ---
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(); // Important: Force Unity to recognize the file system changes

        LogStatus("Successfully deleted all generated assets.");
        Repaint();
    }
    #endregion

    #region XLSX File Sync


    #endregion
}