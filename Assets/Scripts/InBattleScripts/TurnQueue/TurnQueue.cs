using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main TurnQueue Class
public class TurnQueue : MonoBehaviour
{
    // Main queue that's in charge of updating
    private PriorityQueue<TurnQueueUnit> queue = new PriorityQueue<TurnQueueUnit>();

    // Dictionary to keep track of what units to work with
    private Dictionary<AbstractInBattleUnit, TurnQueueUnit> trackedUnits = new Dictionary<AbstractInBattleUnit, TurnQueueUnit>();

    // Main way to keep track of the turn number
    private int turnNumber = 1;

    // Main runtime boolean for recalculation (to avoid doing recalculations 5 times in 1 frame)
    private bool recalculateThisFrame = false;


    // Main function to initialize queue
    public void initialize(List<AbstractInBattleUnit> units) {

    }


    // Main function to get the next unit in the turn queue
    public AbstractInBattleUnit getNextUnit() {
        return null;
    }


    // Main function to get the current turn number
    public int getTurnNumber() {
        return turnNumber;
    }


    // Main function to update the queue accordingly. Must be run at the end of each turn
    public void updateQueue() {

    }


    // Main event handler function for when a unit dies
    public void onUnitDeath() {

    }


    // Main event handler function for when a unit's speed changed
    public void onUnitSpeedChanged() {

    }


    // Private helper function to calculate the max 
    private float getMaxSpeed(List<AbstractInBattleUnit> units) {
        return 0f;
    }


    // Private helper function to recalculate the queue
    private void recalculateQueue() {

    }
}
