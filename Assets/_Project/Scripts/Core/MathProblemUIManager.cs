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
    private VisualElement container;
    private Label questionText, answerText;
    private Button newProblemButton, submitButton;
    private FloatField inputField;
    private Label feedBackText;
    private VisualElement numpad;
    private VisualElement levelState;
    private Label timer, currQuestion;

    private ProblemViewModel viewModel;

    public event Action<float> OnUpdateCalled;
    
    #endregion


    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        problemMaster = GetComponent<ProblemMaster>();
    }
    
    public void Initiation(QAMainUI mainui)
    {
        questionText = mainui.QuestionText;
        answerText = mainui.AnswerText;
        inputField = mainui.AnswerField;
        feedBackText = mainui.FeedbackText;
        submitButton = mainui.SubmitButton;
        timer = mainui.TimerLabel;
        currQuestion = mainui.CurrQuestionLabel;

        container = mainui.Container;
        // Find and Add listener to the button
        newProblemButton = mainui.NextQuestionButton;


        newProblemButton.clicked += problemMaster.CreateNewProblem;
        newProblemButton.clicked += UISetUpForAnsweringQuestion;

        submitButton.clicked += SubmitButtonFunc;
        // Subscribe to the OnProblemGenerated event
        //problemMaster.OnProblemGenerated += UpdateProblemUI;
        //problemMaster.OnQuestionChange += UpdateQuestionID;
        //problemMaster.OnTimerUpdate += UpdateTimeText;
        //problemMaster.OnResultRecorded += UpdateUIElementsBasedOnResult;
        //UISetUpForAnsweringQuestion();
        CreateProblemViewModel();
        SetDisplayAnswerTextOrField(UseTextForAnswer);

        answerText.text = "0";

    }

    private void CreateProblemViewModel()
    {
        if (problemMaster == null)
        {
            Debug.LogError("ProblemMaster is not assigned.");
            return;
        }
        viewModel = new ProblemViewModel(problemMaster);
        viewModel.OnProblemUpdated += UpdateProblemUI;
        viewModel.OnTimerUpdated += UpdateTimerUI;
        viewModel.OnFeedbackUpdated += UpdateFeedbackUI;
        viewModel.OnQuestionNumberUpdated += UpdateQuestionNumberUI;
        viewModel.OnLevelComplete += OnLevelCompletedFunc;

        OnUpdateCalled += viewModel.UpdateTimer;
    }

    private void OnDisable()
    {
        viewModel?.Dispose();
        // Unsubscribe from the event to avoid memory leaks
        
        newProblemButton.clicked -= problemMaster.CreateNewProblem;
        newProblemButton.clicked -= UISetUpForAnsweringQuestion;

        submitButton.clicked -= SubmitButtonFunc;
        OnUpdateCalled -= viewModel.UpdateTimer;

        //problemMaster.OnProblemGenerated -= UpdateProblemUI;
        //problemMaster.OnQuestionChange -= UpdateQuestionID;
        //problemMaster.OnTimerUpdate -= UpdateTimeText;
        //problemMaster.OnResultRecorded -= UpdateUIElementsBasedOnResult;

    }

    private void Update()
    {
        // Update the timer in the ViewModel
        OnUpdateCalled?.Invoke(Time.deltaTime);
    }

    private void OnLevelCompletedFunc()
    {
        // Handle level completion logic here
        Debug.Log("Level Completed! from Math Problem UI Manager");
        // You can also trigger any UI updates or transitions here.
        OnUpdateCalled -= viewModel.UpdateTimer;
    }

    private void UpdateQuestionID(int id)
    {
        currQuestion.text = id.ToString();
    }

    private void UpdateTimeText(float t)
    {
        timer.text = ((int)t).ToString();
    }

    private void UpdateProblemUI()
    {
        // Update the question label based on the current problem
        if (viewModel != null)
        {
            questionText.text = $"{viewModel.CurrentProblem.Number1} {viewModel.CurrentProblem.Operator} {viewModel.CurrentProblem.Number2}";
        }
    }

    private void UpdateTimerUI()
    {
        timer.text = $"Time: {Mathf.CeilToInt(viewModel.Timer)}";
    }

    private void UpdateFeedbackUI(string feedback)
    {
        feedBackText.text = feedback;
    }

    private void UpdateQuestionNumberUI()
    {
        currQuestion.text = $"Question: {viewModel.CurrentQuestionNumber}";
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
        //VisualElement container = root.Q<VisualElement>("Container");
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
        MathResult result = problemMaster.RecordResult(GetAnswerValue());
        UpdateUIElementsBasedOnResult(result);

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
