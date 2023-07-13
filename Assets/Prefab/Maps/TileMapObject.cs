using UnityEngine;

public class TileMapObject
{
    public enum TileType
    {
        None,
        Aluminium,
        Steel,
        Plasteel,
        Tungsten,
    }

    BaseGrid<TileMapObject> grid;            //reference to the overall grid
    public int x, y;
    TileType tileType = TileType.None;

    LooseObject looseObject;
    InstalledObject installedObject;

    public TileMapObject(BaseGrid<TileMapObject> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
    }

    public TileType getTileType() { return tileType; }

    public void setTileType(TileType sprite)
    {
        tileType = sprite;
        grid.setRebuild(true);
    }

    public override string ToString() { return tileType.ToString(); }
}
