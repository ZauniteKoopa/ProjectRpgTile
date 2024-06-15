using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class BattleTileData : ScriptableObject
{
    public TileBase[] tiles;

    public int movementCost;
    public bool walkable;
}
