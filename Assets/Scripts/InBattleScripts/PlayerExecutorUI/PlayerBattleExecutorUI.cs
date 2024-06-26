using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBattleExecutorUI : MonoBehaviour
{
    [SerializeField]
    private List<UnitAbilityUI> unitAbilityDisplays;
    [SerializeField]
    private Button moveButton;
    [SerializeField]
    private Button abilitiesButton;
    [SerializeField]
    private GameObject startingExecutorPanel;
    private AbstractInBattleUnit displayedUnit;


    // On game start, do set up
    void Start() {

        for (int i = 0; i < unitAbilityDisplays.Count; i++) {
            int curIndex = i;       // Needed to have it so that delegates won't point to the same pointer (i)
            unitAbilityDisplays[i].abilityExecuteStart.AddListener( () => onAbilityExecuteStart(curIndex) );
        }
    }


    // Event handler to intialize UI with specified unit information
    public void initializeWithUnit(AbstractInBattleUnit unit) {
        Debug.Log("Do setup");
    }


    // Event handler for when player wants to move
    public void onMoveStart() {
        Debug.Log("Start movement");
    }


    // Event handler for when player wants to end their turn
    public void onEndTurn() {
        Debug.Log("end the turn");
    }


    // Main event handler for running abilities
    public void onAbilityExecuteStart(int abilityIndex) {
        Debug.Log("Start ability " + abilityIndex);
    }


    // Main event handler to handle when an action ends
    public void onActionEnd() {
        Debug.Log("Update button states here");
    }

}
