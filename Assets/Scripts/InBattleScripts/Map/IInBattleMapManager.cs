using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInBattleMapManager
{
    // Main function to move a unit along a path
    //  Pre: unit != null and dest is an occupiable and navigatable location on the map
    //  Post: moves unit along the path in a sequence at a specified speed
    public void moveUnitAlongPath(AbstractInBattleUnit unit, Vector3Int dest, float speed);


    // Main function to move the unit linearly to a destination
    //  Pre: unit != null and dest is an occupiable location on the map
    //  Post: Moves unit directly along a line towards dest at a specified speed
    public void directMoveUnit(AbstractInBattleUnit unit, Vector3Int dest, float speed);


    // Main function to spawn units into the map when a battle starts
    //  Pre: units.Count > 0 && all units within the list are non-null and alive
    //  Post: Units are now being tracked by this Map Manager
    public void spawnInUnits(List<AbstractInBattleUnit> units);


    // Main function to deactivate map when the battle is over
    //  Pre: none, the battle is over at this point
    //  Post: map is reset so that no one is considered on the map now, player teleports back to area they once were
    public void deactivate();


    // Main function to kill a unit's signal on the map
    //  Pre: unit is non-null and is still within records
    //  Post: This unit is no longer tracked within the battle map
    public void killUnit(AbstractInBattleUnit unit);


    // Main function to highlight a radius around you in a spepcified color
    //  Pre: center is a Vector3Int on the map. Int is the radius that you want to highlight arond
    //  Post: Will highlight the radius around the center and then return the list of Vector3Ints that are highlighted
    public List<Vector3Int> highlightRadius(Vector3Int center, AbstractInBattleUnit unit, int radius, Color color, bool considersCollision);


    // Main function to highlight a radius around a unit in a specified color 
    //  Pre: unit is the unit that you want to highlight around. Int is the radius. 
    //  Post: Will highlight the radius around the center and then return the list of Vector3Ints
    public List<Vector3Int> highlightRadius(AbstractInBattleUnit unit, int radius, Color color, bool considersCollision);
}
