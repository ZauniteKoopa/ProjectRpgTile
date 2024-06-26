using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UnitAbilityUI : MonoBehaviour, IPointerEnterHandler
{
    private IAbility shownAbility;
    [SerializeField]
    private TMP_Text abilityNameLabel;
    [SerializeField]
    private UnitAbilityDescriptor abilityDescriptor;
    [SerializeField]
    private Button abilityExecuteButton;

    public UnityEvent abilityExecuteStart;


    // Main event handler function for when button is highlighted or moused over
    public void OnPointerEnter(PointerEventData data) {
        abilityDescriptor.displayAbilityInfo(shownAbility);
    }


    // Main event handler function for when button is clicked 
    public void onButtonClicked() {
        Debug.Log("execute ability");
        abilityExecuteStart.Invoke();
    }


    // Main function to display ability
    public void displayAbility(IAbility ability) {
        Debug.Log("Display ability name on button here");
    }


    // Main function to display nothing
    public void displayNothing() {
        Debug.Log("Display nothing here");
    }
}
