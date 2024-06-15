using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main function representing a Dijkstra Node
public class TileDijkstraNode {
    public Vector3Int nodePosition;
    public int distanceCost;
    public TileDijkstraNode backPointer;


    public TileDijkstraNode(Vector3Int pos) {
        nodePosition = pos;
        distanceCost = -1;
        backPointer = null;
    }
}


// Main function representing a dijkstra edge
public class TileDijkstraEdge : IComparable<TileDijkstraEdge>{

    public Vector3Int src;
    public Vector3Int dest;
    public int cost;
    public int heuristicMeasure;


    // Constructor with no heuristic
    public TileDijkstraEdge(Vector3Int src, Vector3Int dest, int c) {
        this.src = src;
        this.dest = dest;
        cost = c;
        heuristicMeasure = c;
    }


    // Constructor with heuristic
    public TileDijkstraEdge(Vector3Int src, Vector3Int dest, int c, int h) {
        this.src = src;
        this.dest = dest;
        cost = c;
        heuristicMeasure = h;
    }

    // Main function to compare units
    //  Pre: other != null
    //  Post: returns an int that checks if this unit is greater or less than other unit
    public int CompareTo(TileDijkstraEdge other) {
        Debug.Assert(other != null);
        return heuristicMeasure.CompareTo(other.heuristicMeasure);
    }
}


// Main Nav Mesh class
public class BattleTileNavMesh
{
    // Processing condition for A*
    public delegate bool ProcessingCondition(int distanceTraveled);

    // Heuristic for A*
    public delegate int AStarHeuristic(Vector3Int dest);

    // array that lists the edges 
    private static readonly Vector3Int[] BASE_EDGES = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

    // Main variables to keep track of the graph
    private Dictionary<Vector3Int, BattleTile> map;
    private Dictionary<Vector3Int, TileDijkstraNode> navGraph;

    // Main variables to keep track of to avoid unnecessary reprocessing
    private bool processed = false;
    private Vector3Int processedSource;
    private bool forceRecalculation = false;


    // Main constructor
    //  Pre: m != null
    public BattleTileNavMesh(Dictionary<Vector3Int, BattleTile> m) {
        Debug.Assert(m != null);
        map = m;
        navGraph = new Dictionary<Vector3Int, TileDijkstraNode>();

        foreach (KeyValuePair<Vector3Int, BattleTile> tile in map) {
            navGraph.Add(tile.Key, new TileDijkstraNode(tile.Key));
        }
    }


    // Main function to A* process around a graph given radial distance
    public List<Vector3Int> getAllPossibleLocations(Vector3Int src, int radius, AbstractInBattleUnit unit, bool considersCollision) {
        return processAStar(
            src,
            unit,
            (x) => x <= radius,
            (d) => 0,
            considersCollision
        );
    }


    // Main function to get the path from src to dest
    //  Pre: src and dest are grid locations on the map. Unit is a unit that's moving through the map
    public Stack<Vector3Int> getPath(Vector3Int src, Vector3Int dest, AbstractInBattleUnit unit) {
        // Only force recalculation if any of the conditions are true:
        //   - forceRecalculation is true
        //   - current processed src is not this source
        //   - backPointer for dest is null
        if (forceRecalculation || !sourceIsProcessed(src) || navGraph[dest].backPointer == null) {
            processAStar(
                src,
                unit,
                (x) => navGraph[dest].backPointer != null,
                (n) => (Mathf.Abs(dest.x - n.x) + Mathf.Abs(dest.y - n.y))
             );
        }

        // Create path
        TileDijkstraNode curNode = navGraph[dest];
        Stack<Vector3Int> path = new Stack<Vector3Int>();
        while (curNode.backPointer != null) {
            path.Push(curNode.nodePosition);
            curNode = curNode.backPointer;
        }

        return path;
    }



    // Main function to process the A* given the specific sources
    //  Pre: src is where the navigation starts from, 
    //       unit is the unit that's moving.
    //       runningCondition is the condition in which this processing will continue.
    //       heuristic is the heuristic used to sort the queue. if no heuristic used, you're just doing the dijkstra algorithm
    //       considersBlocking will make it consider tiles that block movement
    //  Post: returns a list of all of the nodes that have been processed in the form of Vector3Ints
    private List<Vector3Int> processAStar(
        Vector3Int src,
        AbstractInBattleUnit unit,
        ProcessingCondition runningCondition,
        AStarHeuristic heuristic,
        bool considersBlocking = true
    ) {
        // Preconditions
        Debug.Assert(map.ContainsKey(src));
        Debug.Assert(unit != null);

        // Initial setup O(V). TODO: Find a way to avoid recalculation via cached source
        List<Vector3Int> possibleDests = new List<Vector3Int>();
        clearNavGraph();

        // Set up src node and queue
        PriorityQueue<TileDijkstraEdge> dijkstraQueue = new PriorityQueue<TileDijkstraEdge>();
        navGraph[src].distanceCost = 0;
        possibleDests.Add(src);

        foreach (Vector3Int edge in BASE_EDGES) {
            Vector3Int edgeDest = src + edge;
            if (map.ContainsKey(edgeDest) && (!considersBlocking || !map[edgeDest].blocksMovement(unit))) {
                dijkstraQueue.Enqueue(new TileDijkstraEdge(
                    src,
                    edgeDest,
                    map[src].getMovementCost(),
                    map[src].getMovementCost() + heuristic(edgeDest)
                ));
            }
        }

        // Loop through queue until radius worn out
        while (!dijkstraQueue.IsEmpty()) {
            TileDijkstraEdge runningEdge = dijkstraQueue.Dequeue();
            Vector3Int curDest = runningEdge.dest;
            int distanceTaken = runningEdge.cost;

            // Only move forward if you haven't visited yet
            if (navGraph[curDest].distanceCost < 0 && runningCondition(distanceTaken)) {
                // Set up destination
                navGraph[curDest].distanceCost = distanceTaken;
                navGraph[curDest].backPointer = navGraph[runningEdge.src];
                possibleDests.Add(curDest);

                // add new edges from destination
                foreach (Vector3Int edge in BASE_EDGES) {
                    Vector3Int edgeDest = curDest + edge;
                    if (map.ContainsKey(edgeDest) && (!considersBlocking || !map[edgeDest].blocksMovement(unit))) {
                        int totalEdgeCost = map[curDest].getMovementCost() + navGraph[curDest].distanceCost;
                        dijkstraQueue.Enqueue(new TileDijkstraEdge(
                            curDest,
                            edgeDest,
                            totalEdgeCost,
                            totalEdgeCost + heuristic(edgeDest)
                        ));
                    }
                }
            }
        }

        // Update recalc variables at the end
        processed = true;
        processedSource = src;
        forceRecalculation = !considersBlocking;

        return possibleDests;
    }


    // function to clear out the graph O(V). should be done at the end of a battle
    //  Pre: none
    //  Post: all nodes within the navGraph will be cleared
    public void clearNavGraph() {
        foreach (KeyValuePair<Vector3Int, TileDijkstraNode> entry in navGraph) {
            entry.Value.distanceCost = -1;
            entry.Value.backPointer = null;
        }
    }


    // Main function to check if A* Processing is already present in the current frame for a specified source node
    private bool sourceIsProcessed(Vector3Int src) {
        return processed && processedSource == src;
    }
}
