// FirebaseQuestionProvider.cs
using Firebase;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FirebaseQuestionProvider : MonoBehaviour, IQuestionProvider
{
    public async Task<List<MathProblem>> GetQuestionsForLevelAsync(string symbol, int difficulty, int level)
    {
        var app = await GetFirebaseAppAsync();
        if (app == null)
        {
            Debug.LogWarning("Firebase App initialization failed.");
            return null;
        }
        try
        {
            FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

            Query levelQuery = db.Collection("questions")
                .WhereEqualTo("symbol", symbol)
                .WhereEqualTo("difficulty", difficulty)
                .WhereEqualTo("level", level);

            QuerySnapshot snapshot = await levelQuery.GetSnapshotAsync();

            if (snapshot.Count > 0)
            {
                var mathProblems = new List<MathProblem>();
                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    Dictionary<string, object> data = document.ToDictionary();
                    mathProblems.Add(new MathProblem(
                        System.Convert.ToSingle(data["number1"]),
                        System.Convert.ToSingle(data["number2"]),
                        data["symbol"].ToString()[0]
                    ));
                }
                Debug.Log($"Successfully loaded {mathProblems.Count} questions from FIREBASE provider.");
                return mathProblems;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Firebase query failed: {e.Message}");
        }

        // Return null on any failure
        return null;
    }

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
}