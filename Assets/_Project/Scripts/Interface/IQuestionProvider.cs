using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// A contract for any class that can provide a list of math questions for a level.
/// </summary>
public interface IQuestionProvider
{
    /// <summary>
    /// Asynchronously fetches a list of MathProblem for a specific level.
    /// </summary>
    /// <param name="symbol">The math operator (+, -, etc.)</param>
    /// <param name="difficulty">The difficulty stage (e.g., 1, 2, 3)</param>
    /// <param name="level">The level number (e.g., 1 to 10)</param>
    /// <returns>A Task that resolves to a List of MathProblem, or null if it fails.</returns>
    Task<List<MathProblem>> GetQuestionsForLevelAsync(string symbol, int difficulty, int level);
}
