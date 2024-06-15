using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class BattleTile
{
    // Base variables for battle tiles that are active in the game
    private int baseMoveCost;
    public bool walkable;
    private AbstractInBattleUnit occupiedUnit;


    // Main constructor
    //  Pre: td != null
    public BattleTile(BattleTileData td) {
        Debug.Assert(td != null);

        occupiedUnit = null;
        baseMoveCost = td.movementCost;
        walkable = td.walkable;
    }


    // Main function to occupy the tile
    //  Pre: occupiedUnit == null && IRPGUnit != null
    //  Post: IRPGUnit is not considered on the tile according to metadata
    public void occupyTile(AbstractInBattleUnit unit) {
        Debug.Assert(occupiedUnit == null && unit != null);
        occupiedUnit = unit;
    }


    // Main function to leave your current tile
    //  Pre: occupiedUnit != null
    //  Post: occupieUnit is now null, meaning people can land on this tile
    public void clearOccupation() {
        occupiedUnit = null;
    }

    
    // Main boolean function to check if you can occupy tile
    //  Pre: none
    //  Post: returns whether or not you can occupy the tile
    public bool canOccupy() {
        return occupiedUnit == null && walkable;
    }


    // Main function to check if you a tile blocks your movement
    //  Pre: unit is the unit that's trying to move through this tile
    //  Post: returns if tile blocks movement
    public bool blocksMovement(AbstractInBattleUnit movingUnit) {
        if (!walkable) {
            return true;
        } else {
            return (occupiedUnit != null && !movingUnit.isAlly(occupiedUnit));
        }
    }


    // Main function to get the movement cost of a tile
    //  Pre: none
    //  Post: access movement cost of this tile
    public int getMovementCost() {
        return baseMoveCost;
    }


    // Main function to get the unit occupying this tile
    //  Pre: none
    //  Post: returns a unit if it's within the tile. returns null if there's no unit
    public AbstractInBattleUnit getUnit() {
        return occupiedUnit;
    }
}
