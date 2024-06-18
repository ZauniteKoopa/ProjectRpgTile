using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Ability/DamageAbility")]
public class DamageAbility : IAbility
{
    // Damage ratio for how much this would affect unit
    [SerializeField]
    private float damageRatio = 1.2f;

    // Flag for whether or not this uses the magic stat or the attack stat
    [SerializeField]
    private bool usesMagic = false;

    // Flag for whether this does magic damage or does physical damage
    [SerializeField]
    private bool isMagic = false;
    

    // Main function to apply damage to a unit, if it exists.
    public override void applyEffects(IInBattleMapManager map, UnitStats attackerStats, List<Vector3Int> targetPositions) {
        StatType damageStat = (usesMagic) ? StatType.MAGIC : StatType.ATTACK;
        float damage = attackerStats.getStat(damageStat) * damageRatio;

        foreach (Vector3Int pos in targetPositions) {
            AbstractInBattleUnit targetUnit = map.getUnit(pos);

            if (targetUnit != null) {
                targetUnit.inflictDamage(damage, isMagic);
            }
        }
    }
}
