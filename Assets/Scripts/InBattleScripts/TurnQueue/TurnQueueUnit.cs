using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main class to represent a turn queue unit (TODO: Put this in another class)
public class TurnQueueUnit : IComparable<TurnQueueUnit> {
    // Unit that this needs to reference to
    private AbstractInBattleUnit unit;

    // The delta that this unit will move on the timeline
    private float delta;

    // The current timeline position of this unit
    private float currentTimelinePosition;

    // The timeline position that represent the unit's last turn
    private float earliestLastTurn;

    // The max number of consecutive turns
    private const int MAX_CONSEQUTIVE_TURNS = 3;


    // Main constuctor of a turn queue unit that calculates delta relative to maxUnitSpeed
    public TurnQueueUnit(AbstractInBattleUnit u, float maxUnitSpeed) {
        unit = u;
        earliestLastTurn = 0f;
        recalculateDelta(maxUnitSpeed);
        earliestLastTurn = delta;
    }


    // Main function to recalculate delta
    public void recalculateDelta(float maxUnitSpeed) {
        delta = (((maxUnitSpeed + 1f) - unit.getStat(StatType.SPEED)) / (maxUnitSpeed / MAX_CONSEQUTIVE_TURNS)) + 1f;
    }


    // Main accessor function to get the unit
    public AbstractInBattleUnit getUnit() {
        return unit;
    }


    // Main function to calculate next turn
    //  Should be run at the end of a turn
    public void calculateNextTurn() {
        earliestLastTurn = currentTimelinePosition;
        currentTimelinePosition += delta;
    }


    // Main comparator function
    public int CompareTo(TurnQueueUnit other) {
        // If other is null, this is "lesser" and takes priority
        if (other == null) {
            return -1;
        }

        int compareValue = currentTimelinePosition.CompareTo(other.currentTimelinePosition);
        if (compareValue == 0) {
            compareValue = earliestLastTurn.CompareTo(other.earliestLastTurn);
        }
        return compareValue;
    }
}
