using System.IO;
using UnityEngine;

public class QuestionCacheProcessor : MonoBehaviour
{

    //caching and versioning
    private const string LOCAL_VERSION_KEY = "QuestionVersion";
    private const string CACHE_FILE_NAME = "question_cache.json";
    private string CachePath => Path.Combine(Application.persistentDataPath, CACHE_FILE_NAME);


}
