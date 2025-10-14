using UnityEngine;
public interface IResultFeedback
{
    (string message, Color color) GetFeedback();
}

public interface IResultTypeProvider
{
    ResultType GetResultType();
}

public interface IMathAnswerHandler<TSolution, TResult>
{
    TResult RecordResult(float userAnswer);
}

public interface ITextAnswerHandler<TSolution, TResult>
{
    TResult RecordResult(int selectedIndex);
}
