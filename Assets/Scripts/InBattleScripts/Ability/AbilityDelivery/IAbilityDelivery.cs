using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IAbilityDelivery : ScriptableObject
{
    // Main function to get the list of available targets in the form of vector3s
    //  Pre: map and attacker Position is not null
    //  Post: returns a list of selectable Vector3Ints to choose from
    public abstract List<Vector3Int> getAvailableTargets(IInBattleMapManager map, AbstractInBattleUnit attacker);
}
