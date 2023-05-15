using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatGradientVisual : MonoBehaviour
{
    private BaseGrid<int> grid;
    private Mesh mesh;

    private void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    public void SetGrid(BaseGrid<int> grid)
    {
        this.grid = grid;
    }

    public void updateMeshVisual()
    {
        CreateEmptyMeshData(grid.getWidth() * grid.getHeight(), 
            out Vector3[] vertices, out Vector2[] uv, out int[] triangles);

        for (int x = 0; x < grid.getWidth(); x++) {;
            for (int y = 0; y < grid.getHeight(); y++) {
                int index = x * grid.getHeight() + y;
                Vector3 quadSize = new Vector3(1, 1) * grid.getCellSize();

                int gridValue = grid.getValue(x,y);
                float gridValueNormalized = (float)gridValue / 100;      //change toi temp min/max
                Vector2 gridValueUV = new Vector2(gridValueNormalized, 0f);
                AddQuad(vertices, uv, triangles, index, grid.getWorldPos(x, y) + 0.5f * quadSize, quadSize, gridValueUV);
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }


    private void AddQuad(Vector3[] vertices, Vector2[] uvs, int[] triangles, int index, Vector3 GridPos, Vector3 QuadSize, Vector2 Uv)
    {
        vertices[index * 4] = new Vector3((-0.5f + GridPos.x) * QuadSize.x, (-0.5f + GridPos.y) * QuadSize.y);
        vertices[(index * 4) + 1] = new Vector3((-0.5f + GridPos.x) * QuadSize.x, (+0.5f + GridPos.y) * QuadSize.y);
        vertices[(index * 4) + 2] = new Vector3((+0.5f + GridPos.x) * QuadSize.x, (+0.5f + GridPos.y) * QuadSize.y);
        vertices[(index * 4) + 3] = new Vector3((+0.5f + GridPos.x) * QuadSize.x, (-0.5f + GridPos.y) * QuadSize.y);

        uvs[(index * 4)] = Uv;
        uvs[(index * 4) + 1] = Uv;
        uvs[(index * 4) + 2] = Uv;
        uvs[(index * 4) + 3] = Uv;

        triangles[(index * 6) + 0] = (index * 4) + 0;
        triangles[(index * 6) + 1] = (index * 4) + 1;
        triangles[(index * 6) + 2] = (index * 4) + 2;
        triangles[(index * 6) + 3] = (index * 4) + 2;
        triangles[(index * 6) + 4] = (index * 4) + 3;
        triangles[(index * 6) + 5] = (index * 4) + 0;
    }

    private void CreateEmptyMeshData(int quadCount, out Vector3[] vertices, out Vector2[] uvs, out int[] triangles)
    {
        vertices = new Vector3[quadCount * 4];
        uvs = new Vector2[quadCount * 4];
        triangles = new int[quadCount * 6];
    }

}
