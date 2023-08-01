
public class PathNode {
    private BaseGrid<PathNode> grid;            //reference to the overall grid
    public int x, y;
    public int costG, costH, costF;
    private PathNode prevNode;

    public bool isSolid = false;            //if solid then cannot move
    public bool isSpace = true;             //if space then need to have "fly" tag, else can walk
    public int movementCost = 1;            //multiplier on the NPC's speed, use 1/cost for the offset, never be negative
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
