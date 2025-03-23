using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using System;

public class MathProblemUIManager : MonoBehaviour
{

    public bool UseTextForAnswer;

    #region PrivateField
    private ProblemMaster problemMaster;

    //UI Elements
    private VisualElement root;

    private Label questionText, answerText;
    private Button newProblemButton, submitButton;
    private FloatField inputField;
    private Label feedBackText;
    private VisualElement numpad;
    private VisualElement levelState;
    private Label timer, currQuestion; 
    #endregion


    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        problemMaster = GetComponent<ProblemMaster>();
    }
    
    private void OnEnable()
    {
        questionText = root.Q<Label>("QuestionText");
        answerText = root.Q<Label>("AnswerText");
        inputField = root.Q<FloatField>("AnswerField");
        feedBackText = root.Q<Label>("FeedbackText");
        submitButton = root.Q<Button>("Submit");
        timer = root.Q<Label>("Timer");
        currQuestion = root.Q<Label>("CurrQuestion");


        // Find and Add listener to the button
        newProblemButton = root.Q<Button>("NextQuestion");
        

        newProblemButton.clicked += problemMaster.CreateNewProblem;
        newProblemButton.clicked += UISetUpForAnsweringQuestion;

        submitButton.clicked += SubmitButtonFunc;
        // Subscribe to the OnProblemGenerated event
        problemMaster.OnProblemGenerated += UpdateProblemUI;
        problemMaster.OnQuestionChange += UpdateQuestionID;
        problemMaster.OnTimerUpdate += UpdateTimeText;
        problemMaster.OnResultRecorded += UpdateUIElementsBasedOnResult;
        //UISetUpForAnsweringQuestion();

        SetDisplayAnswerTextOrField(UseTextForAnswer);

        answerText.text = "0";

    }

    private void OnDisable()
    {
        // Unsubscribe from the event to avoid memory leaks
        if (problemMaster != null)
        {
            problemMaster.OnProblemGenerated -= UpdateProblemUI;
        }
        newProblemButton.clicked -= problemMaster.CreateNewProblem;
        newProblemButton.clicked -= UISetUpForAnsweringQuestion;

        submitButton.clicked -= SubmitButtonFunc;

        problemMaster.OnProblemGenerated -= UpdateProblemUI;
        problemMaster.OnQuestionChange -= UpdateQuestionID;
        problemMaster.OnTimerUpdate -= UpdateTimeText;
        problemMaster.OnResultRecorded -= UpdateUIElementsBasedOnResult;

    }

    private void UpdateQuestionID(int id)
    {
        currQuestion.text = id.ToString();
    }

    private void UpdateTimeText(float t)
    {
        timer.text = ((int)t).ToString();
    }

    private void UpdateProblemUI(MathProblem problem)
    {
        // Update the problem text on the UI
        questionText.text = $"{problem.Number1} {problem.Operator} {problem.Number2}";
    }

    public void UpdateAnswer(string str){
        if(UseTextForAnswer)
            UpdateAnswerText(str);
        else
            UpdateInputField(str);

    }
    // using this in numpadui
    private void UpdateInputField(string input)
    {
        if (inputField != null)
        {
            if (float.TryParse(inputField.value + input, out float result))
            {
                inputField.value = result;
            }
        }
    }

    private void UpdateAnswerText(string text){
        if (answerText != null)
        {
            //answerText.text = string.Concat(answerText.text,text);
            float temp = float.Parse(answerText.text);
            if (float.TryParse(temp + text, out float result))
            {
                answerText.text = result.ToString();
            }
        }
    }

    public void RemoveLastCharacter()
    {
        if(UseTextForAnswer) RemoveLastCharacterText();
        else RemoveLastCharacterNumber();
    }

    // backspace func  using in numpadui
    public void RemoveLastCharacterNumber()
    {
        if (inputField != null && !string.IsNullOrEmpty(inputField.value.ToString()))
        {
            string currentValue = inputField.value.ToString();
            currentValue = currentValue.Substring(0, currentValue.Length - 1);

            if (float.TryParse(currentValue, out float result))
            {
                inputField.value = result;
            }
            else
            {
                inputField.value = 0;
            }
        }
    }

    public void RemoveLastCharacterText()
    {
        if (answerText != null && !string.IsNullOrEmpty(answerText.text))
        {
            string currentValue = answerText.text;
            currentValue = currentValue.Substring(0, currentValue.Length - 1);

            if (float.TryParse(currentValue, out float result))
            {
                answerText.text = result.ToString();
            }
            else
            {
                answerText.text = "0";
            }
        }
    }

    // self-explanatory
    public void AddNumpadUIToContainer(VisualElement ve)
    {
        VisualElement container = root.Q<VisualElement>("Container");
        container.Insert(container.IndexOf(submitButton), ve);
        numpad = ve;
        UISetUpForAnsweringQuestion();
    }

    //self-explanatory
    private void SetDisplayNumpadUI(bool value)
    {
        numpad.style.display = value? DisplayStyle.Flex : DisplayStyle.None;

    }

    private void SetDisplayAnswerTextOrField(bool enableText){
        answerText.style.display = enableText? DisplayStyle.Flex : DisplayStyle.None;
        inputField.style.display = enableText? DisplayStyle.None : DisplayStyle.Flex;
    }

    // getting value from inputfield
    private float GetAnswerValue()
    {
        if (!UseTextForAnswer)
        {
            if (inputField != null)
            {
                return inputField.value;
            }
        }
        else
        {
            if(answerText != null)
            {
                return float.Parse(answerText.text);
            }
        }
        return default;
    }

    public void DisplayFeedBack(MathResult res)
    {
        if (res.IsAnsweredCorrect)
        {
            Debug.Log("Correct Answer!");
            // Trigger success feedback
            feedBackText.text = "Correct Answer!";
            feedBackText.style.color = Color.green;
        }
        else
        {
            Debug.Log("Incorrect Answer. Try Again!");
            // Trigger failure feedback
            feedBackText.text = "Incorrect Answer. Try Again!";
            feedBackText.style.color = Color.red;


        }
    }

    //function for submit button 
    // DONE need to do: delete onresultrecorded and get the value here from recordresult
    // and change display status only if the answer is correct 
    // if it is wrong only change feedback and floatfield value to 0
    private void SubmitButtonFunc()
    {
        problemMaster.RecordResult(GetAnswerValue());
        //UpdateUIElementsBasedOnResult(result);

    }

    private void UpdateUIElementsBasedOnResult(MathResult result)
    {
        bool resultbool = result.IsAnsweredCorrect;
        ResetAnswerValue();
        SetVisualElementDisplayStyleFLEX(new VisualElement[] { feedBackText });

        if (resultbool)
        {
            SetVisualElementDisplayStyleFLEX(new VisualElement[] { newProblemButton });
            SetVisualElementDisplayStyleNONE(new VisualElement[] { submitButton, numpad });
        }
        DisplayFeedBack(result);
    }

    private void ResetAnswerValue()
    {
        if(inputField != null)inputField.value = 0;
        if(answerText != null)answerText.text = "0";
    }

    private void UISetUpForAnsweringQuestion()
    {
        SetVisualElementDisplayStyleNONE(new VisualElement[] { newProblemButton, feedBackText });
        SetVisualElementDisplayStyleFLEX(new VisualElement[] { submitButton, numpad });
        
    }

    private void SetVisualElementDisplayStyleFLEX(VisualElement[] visual)
    {
        foreach (var element in visual)
        {
            element.style.display = DisplayStyle.Flex;
        }
    }

    private void SetVisualElementDisplayStyleNONE(VisualElement[] visual)
    {
        foreach (var element in visual)
        {
            element.style.display = DisplayStyle.None;
        }
    }
}
