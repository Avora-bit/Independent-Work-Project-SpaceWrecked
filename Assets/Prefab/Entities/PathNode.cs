using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PathNode {
    private BaseGrid<PathNode> grid;            //reference to the overall grid
    public int x, y;
    public int costG, costH, costF;
    private PathNode prevNode;

    public PathNode(BaseGrid<PathNode> grid, int x, int y) {
        this.grid = grid;
        this.x = x;
        this.y = y;
    }

    public override string ToString() { return (x + ", " + y).ToString(); }

    public void setPrevNode(PathNode node) { prevNode = node; }
    public PathNode getPrevNode() { return prevNode; }

    public void calculateCostF() {
        costF = costG + costH;
    }
}
