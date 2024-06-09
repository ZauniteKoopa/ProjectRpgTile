using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public enum StatType {
    ATTACK,
    MAGIC,
    DEFENSE,
    MAGIC_DEFENSE,
    SPEED,
    MAX_HEALTH,
    CUR_HEALTH,
    MAX_MANA,
    CUR_MANA
}

[CreateAssetMenu(menuName = "Unit/UnitStats")]
public class UnitStats : ScriptableObject
{
    // Main variables to set up stats
    [Header("Stats")]
    [SerializeField]
    [Min(0f)]
    private float attack = 0;
    [SerializeField]
    [Min(0f)]
    private float magic = 0;
    [SerializeField]
    [Min(0f)]
    private float defense = 0;
    [SerializeField]
    [Min(0f)]
    private float magicDefense = 0;
    [SerializeField]
    [Min(0f)]
    private float speed = 0;
    [SerializeField]
    [Range(0, 4)]
    private int movement = 0;
    [SerializeField]
    [Min(0f)]
    private float maxHealth = 10;
    [SerializeField]
    [Min(0f)]
    private float maxMana = 10;

    // Main runtime variables
    private Dictionary<StatType, float> statMap = new Dictionary<StatType, float>();
    private bool initialized = false;
    private const float MAX_DEFENSE = 75;
    private const float MAX_DAMAGE_REDUCTION = 0.25f;

    // Defense constant helps calculate the damage reduction on damaging this unit with the following formula. Defense constant is calculated using the 2 constants above
    //   ACTUAL_DAMAGE = MAX_DAMAGE * (DEF_CONST / (DEF_CONST + UNIT_DEFENSE))
    private const float DEFENSE_CONSTANT = (MAX_DEFENSE * MAX_DAMAGE_REDUCTION) / (1 - MAX_DAMAGE_REDUCTION);



    // Main function to initialize 
    public void initialize() {
        if (!initialized) {
            // Initialize stat map
            statMap.Add(StatType.ATTACK, attack);
            statMap.Add(StatType.MAGIC, magic);
            statMap.Add(StatType.DEFENSE, defense);
            statMap.Add(StatType.MAGIC_DEFENSE, magicDefense);
            statMap.Add(StatType.SPEED, speed);
            statMap.Add(StatType.MAX_HEALTH, maxHealth);
            statMap.Add(StatType.MAX_MANA, maxMana);

            // Initialize curHealth and curMana
            statMap.Add(StatType.CUR_HEALTH, maxHealth);
            statMap.Add(StatType.CUR_MANA, maxMana);
        }
    }


    // Main function to access a stat with a stat type key
    public float getStat(StatType stat) {
        return statMap[stat];
    }

    
    // Main function to access base movement of the specific unit
    public int getBaseMovement() {
        return movement;
    }

    
    // Main function to check if this current unit is Alive
    public bool isAlive() {
        return statMap[StatType.CUR_HEALTH] > 0f;
    }

    
    // Main function to check if unit can use a spell given a spell cost
    //  Pre: spellCost >= 0f
    public bool canUseSpell(float spellCost) {
        Debug.Assert(spellCost >= 0f);

        return statMap[StatType.CUR_MANA] >= spellCost;
    }


    // Main function for user to use a spell
    //  Pre: spellCost >= 0f, canUseSpell() for this spell is true
    //  Post: curMana >= 0f
    public void useSpell(float spellCost) {
        Debug.Assert(spellCost >= 0f);
        Debug.Assert(canUseSpell(spellCost));

        statMap[StatType.CUR_MANA] -= spellCost;

        Debug.Assert(statMap[StatType.CUR_MANA] >= 0f);
    }


    // Main function to damage a unit
    //  Pre: damage is >= 0
    //  Post: health decremented in some way and returns the amount the unit was damaged
    public float inflictDamage(float damage, bool isMagic) {
        Debug.Assert(damage >= 0f);

        float curDefenseStat = (isMagic) ? statMap[StatType.MAGIC_DEFENSE] : statMap[StatType.DEFENSE];
        float curDamageReduction = DEFENSE_CONSTANT / (DEFENSE_CONSTANT + curDefenseStat);
        float actualDamage = damage * curDamageReduction;

        statMap[StatType.CUR_HEALTH] -= actualDamage;
        return actualDamage;
    }
}
