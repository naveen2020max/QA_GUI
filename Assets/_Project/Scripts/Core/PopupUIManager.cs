using System;
using UnityEngine;
using UnityEngine.UIElements;

public class PopupUIManager : MonoBehaviour
{
    [SerializeField] private StyleSheet _defaultUSS;

    private VisualElement _root;
    private PopupFile _levelOverFile;

    private ProblemMaster _problemMaster;
    private void Awake()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _problemMaster = GetComponent<ProblemMaster>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        _problemMaster.OnLevelComplete += ShowLevelCompletePopup;
    }

    private void OnDisable()
    {
        _problemMaster.OnLevelComplete -= ShowLevelCompletePopup;
    }

    private void ShowLevelCompletePopup()
    {
        _levelOverFile = CreatePopupFile(_defaultUSS);
    }

    

    public PopupFile CreatePopupFile(StyleSheet ss)
    {
        PopupFile popupFile = new PopupFile(_root, ss);
        popupFile.ShowPopup("Popup Created");
        return popupFile;
    }
}
