using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InBattleTurnExecutor : MonoBehaviour
{
    public static InBattleTurnExecutor mainExecutor;

    // Runtime variables that keeps track of 
    private const int MAX_EXECUTED_ACTIONS = 2;
    private const float UNIT_MOVE_SPEED = 10f;
    private int numActionsExecuted = 0;
    private bool heavyAbilityExecuted = false;
    private bool movementExecuted = false;
    private AbstractInBattleUnit activeUnit;

    // Reference variables
    [SerializeField]
    private IInBattleMapManager mapManager;

    // Events to listen (Mostly for PlayerExecutorUI)
    public UnityEvent playerTurnStarted;
    public UnityEvent turnEnded;
    private bool endTurnSequenceOn = false;



    // On start, set main executor to this (THERE SHOULD ONLY BE ONE MAIN EXECUTOR PER SCENE)
    void Awake()
    {
        mainExecutor = this;
    }

    
    // Main function to start a turn
    public void startTurn(AbstractInBattleUnit curUnit, bool isPlayer) {
        activeUnit = curUnit;
        numActionsExecuted = 0;
        heavyAbilityExecuted = false;
        movementExecuted = false;

        activeUnit.deathEvent.AddListener(onActiveUnitDeath);

        if (isPlayer) {
            playerTurnStarted.Invoke();
        }
    }


    // Boolean to check if you can execute an ability
    public bool canExecuteAbility() {
        return !heavyAbilityExecuted && numActionsExecuted < MAX_EXECUTED_ACTIONS;
    }


    // Main function to check if you can execute a move
    public bool canExecuteMoveUnit() {
        return !movementExecuted && numActionsExecuted < MAX_EXECUTED_ACTIONS;
    }


    // Main function to execute an ability
    public void executeAbility(int abilityIndex, List<Vector3Int> targetedTiles) {
        // Check if unit can even execute the ability (both on a turn execution level and a unit level)
        if (canExecuteAbility() && activeUnit.canUseAbility(abilityIndex)) {
            // Decrement unit mana
            activeUnit.payAbilityCost(abilityIndex);

            // execute the ability within the map
            activeUnit.getAbility(abilityIndex).applyEffects(mapManager, activeUnit.stats, targetedTiles);

            // Update flag
            numActionsExecuted++;
            heavyAbilityExecuted = heavyAbilityExecuted || activeUnit.getAbility(abilityIndex).isHeavyAbility();
        }
    }


    // Main function to execute moving a unit on the tile map
    public void executeMoveUnit(Vector3Int gridDest) {
        if (canExecuteMoveUnit()) {
            bool success = mapManager.moveUnitAlongPath(activeUnit, gridDest, UNIT_MOVE_SPEED);

            if (success) {
                movementExecuted = true;
                numActionsExecuted++;
            }
        }
    }


    // Main function to run when a turn ends. Must wait a delay before actually ending the turn
    public void endTurn() {
        if (!endTurnSequenceOn) {
            StartCoroutine(endTurnSequence());
        }
    }


    // Main event handler for when you die during your own turn
    public void onActiveUnitDeath() {
        endTurn();
    }


    // Main function to end a turn
    private IEnumerator endTurnSequence() {
        endTurnSequenceOn = true;

        // Wait a few frames before ending the turn for everything to be processed and then end it
        for (int i = 0; i < 6; i++) {
            yield return 0;
        }

        activeUnit.deathEvent.RemoveListener(onActiveUnitDeath);
        turnEnded.Invoke();
        endTurnSequenceOn = false;
    }


}
