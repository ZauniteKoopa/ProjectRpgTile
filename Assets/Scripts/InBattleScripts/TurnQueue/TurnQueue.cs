using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Main TurnQueue Class
public class TurnQueue : MonoBehaviour
{
    // Main queue that's in charge of updating
    private PriorityQueue<TurnQueueUnit> queue = new PriorityQueue<TurnQueueUnit>();

    // Dictionary to keep track of what units to work with
    private Dictionary<AbstractInBattleUnit, TurnQueueUnit> trackedUnits = new Dictionary<AbstractInBattleUnit, TurnQueueUnit>();
    private Dictionary<AbstractInBattleUnit, UnityAction> deathEventHandlers = new Dictionary<AbstractInBattleUnit, UnityAction>();
    private Dictionary<AbstractInBattleUnit, UnityAction> speedEventHandlers = new Dictionary<AbstractInBattleUnit, UnityAction>();

    // Main way to keep track of the turn number
    private int turnNumber = 1;

    // Main lock to synchronize all operations on the queue
    private readonly object queueLock = new object();

    // Main runtime boolean for recalculation (to avoid doing recalculations 5 times in 1 frame)
    private Coroutine currentRecalculationSequence;
    public const int NUM_RECALC_FRAME_DELAY = 5;


    // Cached variables for the max speed of the units
    private float cachedMaxSpeed = -1f;
    private AbstractInBattleUnit fastestUnit = null;
    private bool maxSpeedCacheOutdated = false;


    // Main function to initialize queue
    public void initialize(List<AbstractInBattleUnit> units) {
        lock(queueLock) {
            // Calculate max speed
            float maxSpeed = getMaxSpeed(units);

            foreach (AbstractInBattleUnit unit in units) {
                // Track all units in the queue and in the map
                TurnQueueUnit queueUnit = new TurnQueueUnit(unit, maxSpeed);
                trackedUnits.Add(unit, queueUnit);
                queue.Enqueue(queueUnit);

                // Listen to death event
                UnityAction curDeathHandler = delegate { onUnitDeath(unit); };
                unit.deathEvent.AddListener(curDeathHandler);
                deathEventHandlers.Add(unit, curDeathHandler);

                // List to speed event
                UnityAction curSpeedHandler = delegate { onUnitSpeedChanged(unit); };
                unit.speedChangeEvent.AddListener(curSpeedHandler);
                speedEventHandlers.Add(unit, curSpeedHandler);
            }

            // Update management variables
            turnNumber = 1;
            currentRecalculationSequence = null;
            maxSpeedCacheOutdated = false;
        }
    }


    // Main function to clear the queue at the very end of the battle
    public void clear() {
        lock(queueLock) {
            // Clear dictionary and queue
            trackedUnits.Clear();
            queue.Clear();

            // Go through the death event handlers and disconnect
            foreach(KeyValuePair<AbstractInBattleUnit, UnityAction> handler in deathEventHandlers) {
                handler.Key.deathEvent.RemoveListener(handler.Value);
            }

            // Go through the speed change event handlers and disconnect
            foreach(KeyValuePair<AbstractInBattleUnit, UnityAction> handler in speedEventHandlers) {
                handler.Key.deathEvent.RemoveListener(handler.Value);
            }

            // Clear all handlers
            deathEventHandlers.Clear();
            speedEventHandlers.Clear();
        }
    }


    // Main function to get the next unit in the turn queue
    //  Pre: queue size > 0
    //  Post: Should return a non-null unit who would be going next
    public AbstractInBattleUnit getNextUnit() {
        Debug.Assert(!queue.IsEmpty());

        AbstractInBattleUnit nextUnit;

        lock(queueLock) {
            nextUnit = queue.Front().getUnit();
        }

        Debug.Assert(nextUnit != null);
        return nextUnit;
    }


    // Main function to get the current turn number
    public int getTurnNumber() {
        return turnNumber;
    }


    // Main function to update the queue accordingly. Must be run at the end of each turn
    public void moveQueueForward() {
        lock(queueLock) {
            // Get the first unit in the queue and calculate next turn
            TurnQueueUnit unitToGoBackInLine = queue.Dequeue();
            unitToGoBackInLine.calculateNextTurn();

            // Enqueue that unit back in
            queue.Enqueue(unitToGoBackInLine);
        }
    }


    // Main event handler function for when a unit dies
    //  Pre: corpse should be part of trackedUnits
    //  Post: starts recalculation sequence and remove unit from trackedUnits
    public void onUnitDeath(AbstractInBattleUnit corpse) {
        lock (queueLock) {
            Debug.Assert(trackedUnits.ContainsKey(corpse));

            // Remove corpse from tracked units and disconnect from events
            corpse.deathEvent.RemoveListener(deathEventHandlers[corpse]);
            corpse.speedChangeEvent.RemoveListener(speedEventHandlers[corpse]);
            deathEventHandlers.Remove(corpse);
            speedEventHandlers.Remove(corpse);
            trackedUnits.Remove(corpse);

            // If corpse was the fastest unit, set maxSpeedCacheOutdated to true
            if (fastestUnit == corpse) {
                maxSpeedCacheOutdated = true;
            }

            // Only start recalculation sequence if none have been started
            if (currentRecalculationSequence == null) {
                currentRecalculationSequence = StartCoroutine(recalculateQueueSequence());
            }
        }
    }


    // Main event handler function for when a unit's speed changed
    //  Post: starts recalculation sequence if unit is found within trackedUnit. Does nothing otherwise (Need this in case of race condition with unitDeath)
    public void onUnitSpeedChanged(AbstractInBattleUnit unit) {
        lock(queueLock) {
            if (trackedUnits.ContainsKey(unit)) {

                // If Unit is the fastest unit: if speed went down, cache is outdated. If speed went up, update maxSpeed cache variable
                if (fastestUnit == unit) {
                    if (unit.getStat(StatType.SPEED) < cachedMaxSpeed) {
                        maxSpeedCacheOutdated = true;
                    } else {
                        cachedMaxSpeed = unit.getStat(StatType.SPEED);
                    }

                // If unit isn't the fastest unit: check if unit speed is higher than fastest unit. If it is, update the cache
                } else if (fastestUnit != unit && unit.getStat(StatType.SPEED) > cachedMaxSpeed) {
                    cachedMaxSpeed = unit.getStat(StatType.SPEED);
                    fastestUnit = unit;
                }

                // Only start recalculation sequence if none have been started
                if (currentRecalculationSequence == null) {
                    currentRecalculationSequence = StartCoroutine(recalculateQueueSequence());
                }
            }
        }
    }


    // Private helper function to calculate the max 
    //  Pre: units.Count > 0
    //  Post: returns the max speed found in the units and update the cached variables
    private float getMaxSpeed(List<AbstractInBattleUnit> units) {
        Debug.Assert(units != null && units.Count > 0);

        float curMax = -1f;

        foreach (AbstractInBattleUnit unit in units) {
            if (unit.getStat(StatType.SPEED) > curMax) {
                curMax = unit.getStat(StatType.SPEED);
                fastestUnit = unit;
            }
        }

        // Update cache variables
        cachedMaxSpeed = curMax;
        maxSpeedCacheOutdated = false;
        return curMax;
    }


    // Main IEnumerator for recalculating the queue
    private IEnumerator recalculateQueueSequence() {
        // Wait a couple of frames so that we only do one recalculation if a group of units get affected all at once
        for(int i = 0; i < NUM_RECALC_FRAME_DELAY; i++) {
            yield return 0;
        }

        // Actual recalculation
        lock(queueLock) {
            // Get the max speed and clear the queue
            List<AbstractInBattleUnit> currentUnits = new List<AbstractInBattleUnit>(trackedUnits.Keys);
            float curMaxSpeed = (maxSpeedCacheOutdated) ? getMaxSpeed(currentUnits) : cachedMaxSpeed;
            queue.Clear();

            // For each current unit, recalculate speed and add them back in the queue
            foreach(AbstractInBattleUnit unit in currentUnits) {
                TurnQueueUnit curQueueUnit = trackedUnits[unit];
                curQueueUnit.recalculateDelta(curMaxSpeed);
                queue.Enqueue(curQueueUnit);
            }

            // Clean up meta variables around recalculation
            currentRecalculationSequence = null;
        }
    }
}
