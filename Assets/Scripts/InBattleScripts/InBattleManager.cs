using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InBattleManager : MonoBehaviour
{
    // Events to listen to
    UnityEvent battleStarted;
    UnityEvent battleWin;
    UnityEvent battleLose;

    //Reference variables 
    [SerializeField]
    private TurnQueue turnQueue;                    // The turn queue that decides who goes next for a turn
    [SerializeField]
    private IInBattleMapManager mapManager;         // The map manager that keeps track of the unit's position on the map
    [SerializeField]
    private List<AbstractInBattleUnit> playerUnits; // The list of player units on the board
    [SerializeField]
    private List<AbstractInBattleUnit> enemyUnits;  // The list of enemy units on the board

    // Runtime variables
    private Dictionary<AbstractInBattleUnit, UnityAction> deathListeners;   // Mapping of death listeners for cleanup
    private int numEnemiesLeft; // Num players left
    private int numPlayersLeft; // Num enemies left
    private AbstractInBattleUnit currentActiveUnit; // Current unit who is moving this turn
    private bool battleActive = false;   // Boolean to check if the battle is active

    // Const variables
    private const int PLAYER_TEAM_ID = 0;
    private const int ENEMY_TEAM_ID = 1;


    // Main function to start the battle
    public void startBattle() {
        // Initialize your reference variables for this battle instance
        List<AbstractInBattleUnit> units = new List<AbstractInBattleUnit>();
        units.AddRange(enemyUnits);
        units.AddRange(playerUnits);

        mapManager.spawnInUnits(units);
        turnQueue.initialize(units);

        // Track each unit's state
        foreach (AbstractInBattleUnit unit in playerUnits) {
            trackUnit(unit, PLAYER_TEAM_ID);
        }

        foreach (AbstractInBattleUnit unit in enemyUnits) {
            trackUnit(unit, ENEMY_TEAM_ID);
        }

        InBattleTurnExecutor.mainExecutor.turnEnded.AddListener(onTurnEnd);

        // Start next turn
        battleActive = true;
        battleStarted.Invoke();
        startNextTurn();
    }


    // Private helper function to connect to a unit's listener
    private void trackUnit(AbstractInBattleUnit unit, int teamId) {
        unit.teamId = teamId;

        // Death listener
        UnityAction deathTracker = delegate { onUnitDeath(unit); };
        unit.deathEvent.AddListener(deathTracker);
        deathListeners.Add(unit, deathTracker);


        if (teamId == PLAYER_TEAM_ID) {
            numPlayersLeft++;
        } else {
            numEnemiesLeft++;
        }
    }


    // Private helper function to disconnect a unit
    private void forgetUnit(AbstractInBattleUnit unit) {
        if (unit.teamId == PLAYER_TEAM_ID) {
            numPlayersLeft--;
        } else {
            numEnemiesLeft--;
        }

        // Death event listener
        unit.deathEvent.RemoveListener(deathListeners[unit]);
        deathListeners.Remove(unit);
    }


    // Main event handler function for when turn ends
    public void onTurnEnd() {
        if (battleActive) {
            turnQueue.moveQueueForward();
            startNextTurn();
        }
    }

    // Main event handler for when a unit dies
    public void onUnitDeath(AbstractInBattleUnit unit) {
        forgetUnit(unit);

        if (numPlayersLeft == 0 || numEnemiesLeft == 0) {
            endBattle();
        }
    }

    // Private helper function to end the battle
    private void endBattle() {
        // Clear reference variables
        turnQueue.clear();
        mapManager.deactivate();

        // Remove all listeners
        foreach(KeyValuePair<AbstractInBattleUnit, UnityAction> handler in deathListeners) {
            handler.Key.deathEvent.RemoveListener(handler.Value);
        }

        InBattleTurnExecutor.mainExecutor.turnEnded.RemoveListener(onTurnEnd);
        deathListeners.Clear();

        // Broadcast whether or not you win or lose
        if (numPlayersLeft == 0) {
            battleWin.Invoke();
            Debug.Log("You win!");
        } else {
            battleLose.Invoke();
            Debug.Log("You lose");
        }

        // Clear runtime variables
        numPlayersLeft = 0;
        numEnemiesLeft = 0;
    }

    private void startNextTurn() {
        currentActiveUnit = turnQueue.getNextUnit();
        InBattleTurnExecutor.mainExecutor.startTurn(currentActiveUnit, true);
    }
}
