using UnityEngine;
using UnityEngine.UIElements;

public class PopupFile
{
    private VisualElement _popupContainer;
    private Label _popupMessage;
    private Button _popupCloseButton;

    /// <summary>
    /// Creates a new PopupFile and attaches the popup to the given root element.
    /// </summary>
    /// <param name="root">The root VisualElement to which the popup will be added.</param>
    /// <param name="popupStyleSheet">USS stylesheet for popup styling.</param>
    public PopupFile(VisualElement root, StyleSheet popupStyleSheet)
    {
        // Create popup container and add style class.
        _popupContainer = new VisualElement { name = "PopupContainer" };
        _popupContainer.AddToClassList("popup-container");

        // Add the USS style sheet for styling.
        if (popupStyleSheet != null)
        {
            _popupContainer.styleSheets.Add(popupStyleSheet);
        }

        // Create popup message label and add style class.
        _popupMessage = new Label("Default Message") { name = "PopupMessage" };
        _popupMessage.AddToClassList("popup-message");

        // Create close button and add style class.
        _popupCloseButton = new Button(DeletePopup) { text = "OK", name = "PopupCloseButton" };
        _popupCloseButton.AddToClassList("popup-close-button");

        // Add the elements to the popup container.
        _popupContainer.Add(_popupMessage);
        _popupContainer.Add(_popupCloseButton);

        // Initially hide the popup.
        _popupContainer.style.display = DisplayStyle.Flex;

        // Add popup container to the root element.
        root.Add(_popupContainer);
    }

    /// <summary>
    /// Displays the popup with the specified message.
    /// </summary>
    /// <param name="message">Message to show.</param>
    public void ShowPopup(string message)
    {
        _popupMessage.text = message;
        _popupContainer.style.display = DisplayStyle.Flex;
    }

    /// <summary>
    /// Hides the popup.
    /// </summary>
    public void HidePopup()
    {
        _popupContainer.style.display = DisplayStyle.None;
    }

    /// <summary>
    /// Deletes the popup from the UI.
    /// </summary>
    public void DeletePopup()
    {
        // Remove popup container from its parent in the hierarchy.
        _popupContainer.RemoveFromHierarchy();
        // Optionally, you could set _popupContainer to null if needed:
        _popupContainer = null;
    }
}
