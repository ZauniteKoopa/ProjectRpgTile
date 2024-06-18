using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IAbility : ScriptableObject
{
    [SerializeField]
    [Min(0)]
    private int manaCost = 5;
    [SerializeField]
    private string abilityName = "";
    [SerializeField]
    private IAbilityDelivery abilityDelivery;


    // Main function to apply effects over an enemy
    //  Pre: map is the map you want to attack on, attackerStats is the stats of the attacker, targetPositions are the positions on the map you want to target
    //  Post: apply effects to the target
    public abstract void applyEffects(IInBattleMapManager map, UnitStats attackerStats, List<Vector3Int> targetPositions);


    // Main function to get the available targets when doing this attack from this attacker position
    public List<Vector3Int> getAvailableTargets(IInBattleMapManager map, AbstractInBattleUnit unit) {
        return abilityDelivery.getAvailableTargets(map, unit);
    }


    public int getManaCost() {
        return manaCost;
    }


    public string getName() {
        return abilityName;
    }
}
