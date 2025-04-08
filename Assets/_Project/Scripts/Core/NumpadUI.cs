using UnityEngine;
using UnityEngine.UIElements;

public class NumpadUI : MonoBehaviour
{
    private VisualElement root;

    [SerializeField] private float buttonWidth = 60f;
    [SerializeField] private float buttonHeight = 60f;
    [SerializeField] private float buttonSpacing = 10f;

    private MathProblemUIManager uiManager;

    private void Awake()
    {
        uiManager = GetComponent<MathProblemUIManager>();
    }

    private void Start()
    {
        // Get the root VisualElement
        root = GetComponent<UIDocument>().rootVisualElement;

        // Calculate container dimensions based on button size and spacing
        float containerWidth = (buttonWidth) * 4 - buttonSpacing * 2;
        float containerHeight = (buttonHeight + buttonSpacing) * 4 + buttonSpacing;


        // Create a container for the numpad
        var numpadContainer = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                flexWrap = Wrap.Wrap,
                justifyContent = Justify.Center,
                alignItems = Align.Center,
                alignSelf = Align.Center,
                width = containerWidth, // 3 columns
                height = containerHeight, // 4 rows
                backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1),
                marginTop = buttonSpacing
            }
        };

        // Add numbers to the numpad
        for (int i = 1; i <= 9; i++)
        {
            AddNumpadButton(numpadContainer, i.ToString());
        }

        // Fourth row (., 0, <)
        var row4 = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                justifyContent = Justify.Center,
                width = Length.Percent(100)
            }
        };

        // Add DECIMAL BUTTON (disabled)
        Button decimalButton = new Button
        {
            text = ".",
            style =
            {
                width = buttonWidth,
                height = buttonHeight,
                marginRight = buttonSpacing,
                marginBottom = buttonSpacing,
                backgroundColor = Color.white,
                unityTextAlign = TextAnchor.MiddleCenter,
                fontSize = 20
            }
        };
        decimalButton.SetEnabled(false); // Disable the button
        row4.Add(decimalButton);

        // Add the 0 button
        AddNumpadButton(row4, "0");

        // Add the backspace button
        AddBackspaceButton(row4);

        numpadContainer.Add(row4);

        // Add the numpad to the root
        uiManager.AddNumpadUIToContainer(numpadContainer);
    }

    private void AddNumpadButton(VisualElement container, string number)
    {
        var button = new Button
        {
            text = number,
            style =
            {
                width = buttonWidth,
                height = buttonHeight,
                marginBottom = buttonSpacing,
                marginRight = buttonSpacing,
                backgroundColor = Color.white,
                unityTextAlign = TextAnchor.MiddleCenter,
                fontSize = 20
            }
        };

        button.clicked += () =>
        {
            if (uiManager != null)
            {
                uiManager.UpdateAnswer(number);
            }
        };

        container.Add(button);
    }

    private void AddBackspaceButton(VisualElement container)
    {
        var button = new Button
        {
            text = "<",
            style =
            {
                width = buttonWidth,
                height = buttonHeight,
                marginBottom = buttonSpacing,
                marginRight = buttonSpacing,
                backgroundColor = Color.red,
                unityTextAlign = TextAnchor.MiddleCenter,
                fontSize = 20,
                color = Color.white
            }
        };

        button.clicked += () =>
        {
            if (uiManager != null)
            {
                uiManager.RemoveLastCharacter();
            }
        };

        container.Add(button);
    }
}
