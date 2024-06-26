using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitAbilityDescriptor : MonoBehaviour
{
    [SerializeField]
    private TMP_Text manaCostText;
    [SerializeField]
    private TMP_Text powerText;
    [SerializeField]
    private TMP_Text isHeavyText;
    [SerializeField]
    private Image attackTypeIcon;
    [SerializeField]
    private TMP_Text abilityDescriptionText;


    // Main function to display ability information
    public void displayAbilityInfo(IAbility ability) {
        Debug.Log("display ability info");
    }
}
