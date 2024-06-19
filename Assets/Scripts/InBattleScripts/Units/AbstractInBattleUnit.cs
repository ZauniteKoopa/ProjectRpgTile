using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class AbstractInBattleUnit : MonoBehaviour
{
    // Event for when this unit dies
    public UnityEvent deathEvent;

    // Event for which the unit has ended his turn
    public UnityEvent endTurnEvent;

    // Event for which the unit has changed his speed
    public UnityEvent speedChangeEvent;


    // Collection of Unit Stats (TO-DO: DON'T MAKE THIS PUBLIC AND EXPOSE TO EVERYONE)
    public UnitStats stats;

    // Dictionary of effects that apply on stats
    private Dictionary<StatType, float> statEffects = new Dictionary<StatType, float>();

    // Movement reduction. This is gen erally subtractive instead of multiplicative
    private int movementReduction = 0;

    // Main variable for team ID. If set to -1, it is an undefined team Id
    public int teamId = -1;

    private bool isActiveThisTurn = false;



    public void Awake() {
        // Add statEffects
        statEffects.Add(StatType.ATTACK, 1f);
        statEffects.Add(StatType.DEFENSE, 1f);
        statEffects.Add(StatType.MAGIC_DEFENSE, 1f);
        statEffects.Add(StatType.MAGIC, 1f);
        statEffects.Add(StatType.SPEED, 1f);
    }
    
    
    // Main function to start a turn
    public void startTurn() {
        StartCoroutine(turnSequence());
    }

    
    // Main function to run the turn sequence
    public IEnumerator turnSequence() {
        isActiveThisTurn = true;
        yield return executeTurn();

        // Wait a few frames before ending the turn for everything to be processed and then end it
        for (int i = 0; i < 6; i++) {
            yield return 0;
        }
        tryEndTurn();      
    }


    // Private helper function to end the turn by updating the tracking variables IFF you are active this turn. If you aren't active, do nothing
    private void tryEndTurn() {
        if (isActiveThisTurn) {
            isActiveThisTurn = false;
            endTurnEvent.Invoke();
        }
    }


    // Main function to actually execute the turn. This is where the unit should decide what to do for the turn (player or enemy)
    public abstract IEnumerator executeTurn();


    // Main function to check if unit is an ally or not
    public bool isAlly(AbstractInBattleUnit other) {
        return teamId == other.teamId;
    }


    // Main function to access a stat
    public float getStat(StatType stat) {
        float statModifier = (isResourceStatType(stat)) ? 1.0f : statEffects[stat];
        return statModifier * stats.getStat(stat);
    }


    // Main function to get the unit's current movement
    public int getCurrentMovement() {
        return Mathf.Max(stats.getBaseMovement() - movementReduction, 0);
    }


    // main function to get the unit's base movement
    public int getBaseMovement() {
        return stats.getBaseMovement();
    }


    // Main helper method to check if a stat type is a resource stat type (health or mana)
    private bool isResourceStatType(StatType stat) {
        return stat == StatType.CUR_HEALTH ||
            stat == StatType.CUR_MANA ||
            stat == StatType.MAX_HEALTH ||
            stat == StatType.MAX_MANA;
    }


    // Main function to inflict damage
    public void inflictDamage(float damage, bool isMagic) {
        stats.inflictDamage(damage, isMagic);

        // If you died in the middle of your turn, invoke death event and end turn if you were active this turn
        if (!stats.isAlive()) {
            deathEvent.Invoke();
            tryEndTurn();
        }
    }

}
