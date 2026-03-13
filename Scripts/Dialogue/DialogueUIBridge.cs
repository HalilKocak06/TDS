using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


[RequireComponent(typeof(UIDocument))]
public class DialogueUIBridge : MonoBehaviour
{
    [Header("UXML names")]
    [SerializeField] string npcTextName = "NpcText";
    [SerializeField] string choicesName = "ChoicesContainer";
    [SerializeField] string offerInputName = "OfferInput";
    [SerializeField] string offerSubmitName = "OfferSubmitButton";
    [SerializeField] string closeButtonName = "CloseButton";

    UIDocument doc;
    Label npcText;
    VisualElement choicesRoot;
    TextField offerInput;
    Button offerSubmit;
    Button closeBtn;

    Action<int> onChoiceSelected;
    Action<int> onOfferSubmitted;

    void Awake()
    {
        doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;

        npcText = root.Q<Label>(npcTextName);
        choicesRoot = root.Q<VisualElement>(choicesName);
        offerInput = root.Q<TextField>(offerInputName);
        offerSubmit = root.Q<Button>(offerSubmitName);
        closeBtn = root.Q<Button>(closeButtonName);

        offerSubmit.clicked += () =>
        {
            if (onOfferSubmitted == null) return;
            if (int.TryParse(offerInput.value, out int v)) onOfferSubmitted(v);
            else npcText.text = "Sayı gir usta 😅";
        };

        closeBtn.clicked += Hide;
        Hide();

    }

    public void Show() => doc.rootVisualElement.style.display = DisplayStyle.Flex;
    public void Hide()
    {
        doc.rootVisualElement.style.display = DisplayStyle.None;
        ClearChoices();
        onChoiceSelected = null;
        onOfferSubmitted = null;
    }

    public void SetNPCText(string text) => npcText.text = text;

    public void SetChoices(string[] labels, Action<int> onSelected)
    {
        ClearChoices();
        onChoiceSelected = onSelected;

        for (int i = 0; i < labels.Length; i++)
        {
            int idx = i;
            var b = new Button(() => onChoiceSelected?.Invoke(idx)) { text = labels[i] };
            choicesRoot.Add(b);
        }
    }

    public void RequestOffer(Action<int> onSubmit)
    {
        onOfferSubmitted = onSubmit;
        offerInput.value = "";
        offerInput.Focus();
    }

    void ClearChoices() => choicesRoot?.Clear();


}
