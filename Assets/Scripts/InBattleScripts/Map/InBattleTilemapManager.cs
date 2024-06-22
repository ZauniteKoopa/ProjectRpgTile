using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using UnityEngine.Assertions;

public class InBattleTilemapManager : MonoBehaviour, IInBattleMapManager
{
    // Main variables to keep track of where the map is
    [Header("Arena Variables")]
    [SerializeField]
    private Tilemap tilemap = null;
    [SerializeField]
    private BattleTileData[] tileData = null;
    [SerializeField]
    private int arenaWidth = 1;
    [SerializeField]
    private int arenaHeight = 1;
    [SerializeField]
    private Transform arenaOrigin;      // Origin of the arena. Usually the bottom left corner of the arena

    //Dictionaries to hold state of the arena
    private Dictionary<Vector3Int, BattleTile> arenaState;
    private BattleTileNavMesh navMesh;

    // Main DeathEvent Listeners
    private Dictionary<AbstractInBattleUnit, UnityAction> deathListeners = new Dictionary<AbstractInBattleUnit, UnityAction>();


    private void Awake() {
        // Set up tile base and arena state
        Dictionary<TileBase, BattleTileData> tileDictionary = new Dictionary<TileBase, BattleTileData>();
        arenaState = new Dictionary<Vector3Int, BattleTile>();

        foreach (BattleTileData td in tileData){
            foreach (TileBase tile in td.tiles){
                tileDictionary.Add(tile, td);
            }
        }

        //Initialize battleTiles
        Vector3Int originPos = tilemap.WorldToCell(arenaOrigin.position);
        arenaOrigin.GetComponent<SpriteRenderer>().enabled = false;

        for (int x = 0; x < arenaWidth; x++){
            for (int y = 0; y < arenaHeight; y++){
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                tilePos += originPos;
                tilemap.SetTileFlags(tilePos, TileFlags.None);

                TileBase curBase = tilemap.GetTile(tilePos);
                BattleTileData data = tileDictionary[curBase];

                BattleTile bt = new BattleTile(data);
                arenaState.Add(tilePos, bt);
            }
        }

        navMesh = new BattleTileNavMesh(arenaState);
    }


    // Main function to move a unit along a path
    //  Pre: unit != null and dest is an occupiable and navigatable location on the map
    //  Post: moves unit along the path in a sequence at a specified speed. returns a boolean if anyone actually moved
    public bool moveUnitAlongPath(AbstractInBattleUnit unit, Vector3Int dest, float speed) {
        Vector3Int start = getGridUnitPosition(unit);

        if (start != dest) {
            Stack<Vector3Int> path = navMesh.getPath(start, dest, unit);
            StartCoroutine(moveUnitPathSequence(unit, path, speed));
        }

        return start != dest;
    }


    // Main function to move the unit linearly to a destination
    //  Pre: unit != null and dest is an occupiable location on the map
    //  Post: Moves unit directly along a line towards dest at a specified speed
    public void directMoveUnit(AbstractInBattleUnit unit, Vector3Int dest, float speed) {
        StartCoroutine(moveUnitSequence(unit, getGridUnitPosition(unit), dest, speed, 0.1f));
    }


    // Main function to spawn units into the map when a battle starts
    //  Pre: units.Count > 0 && all units within the list are non-null and alive
    //  Post: Units are now being tracked by this Map Manager
    public void spawnInUnits(List<AbstractInBattleUnit> units) {
        Debug.Assert(units.Count > 0);

        // Track every unit that's on the map
        foreach (AbstractInBattleUnit unit in units) {
            Debug.Assert(unit != null);

            Vector3Int unitPos = tilemap.WorldToCell(unit.transform.position);
            if (!arenaState.ContainsKey(unitPos) || !arenaState[unitPos].canOccupy()) {
                Debug.LogError("Error! Unit is not in a valid position. make sure units are actually on the assigned arena AND not stacked on top of eachother on 1 tile");
            }

            // Track units
            arenaState[unitPos].occupyTile(unit);
            unit.transform.position = tilemap.GetCellCenterWorld(unitPos);

            // Set it to listen to unit events
            UnityAction deathListener = delegate { killUnit(unit); };
            unit.deathEvent.AddListener(deathListener);
            deathListeners.Add(unit, deathListener);
        }
    }


    // Main function to deactivate map when the battle is over
    //  Pre: none, the battle is over at this point
    //  Post: map is reset so that no one is considered on the map now, player teleports back to area they once were
    public void deactivate() {
        foreach(AbstractInBattleUnit unit in deathListeners.Keys) {
            Vector3Int unitPos = tilemap.WorldToCell(unit.transform.position);
            arenaState[unitPos].clearOccupation();
            unit.deathEvent.RemoveListener(deathListeners[unit]);
        }

        deathListeners.Clear();
    }


    // Main function to kill a unit's signal on the map
    //  Pre: unit is non-null and is still within records
    //  Post: This unit is no longer tracked within the battle map
    public void killUnit(AbstractInBattleUnit unit) {
        Vector3Int unitPos = tilemap.WorldToCell(unit.transform.position);

        if (!arenaState.ContainsKey(unitPos) || !arenaState[unitPos].canOccupy()) {
            Debug.LogError("Error! Unit is not in a valid position. make sure units are actually on the assigned arena AND not stacked on top of eachother on 1 tile");
        }

        // Clear the tile that the unit is on and disconnect from listener
        arenaState[unitPos].clearOccupation();
        unit.deathEvent.RemoveListener(deathListeners[unit]);
        deathListeners.Remove(unit);
    }


    // Main function to highlight a radius around you in a spepcified color
    //  Pre: center is a Vector3Int on the map. Int is the radius that you want to highlight arond
    //  Post: Will highlight the radius around the center and then return the list of Vector3Ints that are highlighted
    public List<Vector3Int> highlightRadius(Vector3Int center, AbstractInBattleUnit unit, int radius, Color color, bool considersCollision) {
        List<Vector3Int> targetTiles = navMesh.getAllPossibleLocations(center, radius, unit, considersCollision);
        
        Debug.Assert(targetTiles != null);
        foreach (Vector3Int tile in targetTiles) {
            tilemap.SetColor(tile, color);
        }

        return targetTiles;
    }


    // Main function to highlight a radius around a unit in a specified color 
    //  Pre: unit is the unit that you want to highlight around. Int is the radius. 
    //  Post: Will highlight the radius around the center and then return the list of Vector3Ints
    public List<Vector3Int> highlightRadius(AbstractInBattleUnit unit, int radius, Color color, bool considersCollision) {
        return highlightRadius(getGridUnitPosition(unit), unit, radius, color, considersCollision);
    }


    // Main function to get all enemy positions of a unit
    //  Pre: unit is the unit that wants to attack, radius is how far the unit can attack, considerCollisions checks if the attack considers collisions or not
    //  Post: returns all the grid positions that has an enemy on them
    public List<Vector3Int> getAllPossibleEnemies(AbstractInBattleUnit unit, int radius, bool considersCollision) {
        List<Vector3Int> enemyTiles = new List<Vector3Int>();
        List<Vector3Int> targetTiles = navMesh.getAllPossibleLocations(getGridUnitPosition(unit), radius, unit, considersCollision);


        foreach (Vector3Int tilePos in targetTiles) {
            AbstractInBattleUnit tileUnit = arenaState[tilePos].getUnit();

            if (tileUnit != null && !tileUnit.isAlly(unit)) {
                targetTiles.Add(tilePos);
            }
        }

        return enemyTiles;
    }


    // Main function to get the Unit that's found at a specific position
    //  Pre: Vector3Int is a position on the map
    //  Post: Returns the unit's that's at that location. Returns null if no unit is there
    public AbstractInBattleUnit getUnit(Vector3Int position) {
        if (!arenaState.ContainsKey(position)) {
            return null;
        }

        return arenaState[position].getUnit();
    }


    // Main IEnumerator to move unit along a path
    //  Pre: the path is not empty and unit is not null
    //  Post: unit is moving along a path
    private IEnumerator moveUnitPathSequence(AbstractInBattleUnit unit, Stack<Vector3Int> path, float speed) {
        Debug.Assert(unit != null);
        Debug.Assert(path != null && path.Count > 0);

        // Set up current position
        Vector3Int curPosition = getGridUnitPosition(unit);

        while (path.Count > 0) {
            // Move to next destination in the queue
            Vector3Int curDestination = path.Pop();
            yield return moveUnitSequence(unit, curPosition, curDestination, speed, 0.25f);

            // Update curPosition
            curPosition = curDestination;
        }
    }


    // Main IEnumerator to move a unit
    private IEnumerator moveUnitSequence(
        AbstractInBattleUnit unit, 
        Vector3Int src, 
        Vector3Int dest, 
        float unitSpeed, 
        float moveInputDelay = 0f
    ) {
        Debug.Assert(unitSpeed > 0f && unit != null);
        Vector3 worldSrc = tilemap.GetCellCenterWorld(src);
        Vector3 worldDest = tilemap.GetCellCenterWorld(dest);

        // Timer loop
        float travelTime = Vector3.Distance(worldSrc, worldDest) / unitSpeed;
        float timer = 0f;
        while (timer < travelTime) {
            yield return 0;
            timer += Time.deltaTime;
            unit.transform.position = Vector3.Lerp(worldSrc, worldDest, timer / travelTime);
        }

        unit.transform.position = worldDest;

        // Wait for move delay and then turn activeInputSequence to null
        yield return new WaitForSeconds(moveInputDelay);
    }


    // Main function to get the current tile position of a unit
    private Vector3Int getGridUnitPosition(AbstractInBattleUnit unit) {
        return tilemap.WorldToCell(unit.transform.position);
    }
}
