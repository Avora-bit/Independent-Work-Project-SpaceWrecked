using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PathNode {
    private BaseGrid<PathNode> grid;            //reference to the overall grid
    public int x, y;
    public int costG, costH, costF;
    private PathNode prevNode;

    public int costPenalty;                    //positive is bad, negative values is good

    public bool isWalkable = true;
    public int accessLayer = 1;

    public PathNode(BaseGrid<PathNode> grid, int x, int y) {
        this.grid = grid;                   //should not have a specific use case as of yet
        this.x = x;
        this.y = y;
    }

    public override string ToString() { return (x + ", " + y).ToString(); }

    public void setPrevNode(PathNode node) { prevNode = node; }
    public PathNode getPrevNode() { return prevNode; }

    public void calculateCostF() { costF = costG + costH; }

}
