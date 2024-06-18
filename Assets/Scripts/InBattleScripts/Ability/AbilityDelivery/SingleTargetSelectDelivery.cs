using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AbilityDelivery/SingleTargetSelect")]
public class SingleTargetSelectDelivery : IAbilityDelivery
{
    [SerializeField]
    [Min(1)]
    private int range = 1;

    [SerializeField]
    private bool canCollide = false;



    // Main function to get the list of available targets in the form of vector3s
    //  Pre: map and attacker Position is not null
    //  Post: returns a list of selectable Vector3Ints to choose from
    public override List<Vector3Int> getAvailableTargets(IInBattleMapManager map, AbstractInBattleUnit unit) {
        return map.getAllPossibleEnemies(unit, range, canCollide);
    }
}
