using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    public GameObject planePrefab;
    public GameObject emptyPlane;
    public GameObject cubePrefab;

    private GameObject renderTarget;
    private int renderTargetType;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public GameObject requestPlanePrefab()
    {
        if (renderTargetType != 1)
        {
            renderTarget = Instantiate(planePrefab, new Vector3(0, 0, 2), Quaternion.Euler(new Vector3(90, 180, 0)));
            renderTargetType = 1;
        }

        return renderTarget;
    }

    public GameObject requestPlanePrefabCustom(int xSize, int ySize, Vector2 scale, Vector3 position)
    {
        if (renderTargetType != 2)
        {
            if (renderTarget != null)
            {
                Destroy(renderTarget);
            }
            generatePlane(xSize, ySize, scale, position);
            renderTargetType = 2;
        }

        return renderTarget;
    }

    // generates cube prefab according to parameters
    //used by volumetric method
    public GameObject requestCubePrefab(Vector3 position , Vector3 scale)
    {
        if (renderTargetType != 1)
        {
            if(renderTarget != null)
            {
                Destroy(renderTarget);
            }
            renderTarget = Instantiate(cubePrefab, position, Quaternion.Euler(new Vector3(0, 180, 0)));
            renderTarget.transform.localScale = scale;
            renderTargetType = 1;
        }

        return renderTarget;
    }

    public GameObject requestPointCloud(Vector3 position, Vector3 scale, Vector2 pointcloudsize)
    {
        if (renderTargetType != 3)
        {
            if (renderTarget != null)
            {
                Destroy(renderTarget);
            }
            generatePointCloud(position, scale, pointcloudsize);
            renderTargetType = 2;
        }

        return renderTarget;
    }

    public GameObject requestRenderTarget()
    {
        return renderTarget;
    }

    //creates a planelike mesh according to parameters
    //used by displacement method
    private void generatePlane(int xSize, int ySize, Vector2 scale, Vector3 position)
    {
        renderTarget = Instantiate(emptyPlane, position, Quaternion.Euler(new Vector3(0, 0, 0)));
        renderTarget.transform.localScale = new Vector3(scale.x, scale.y, 1);
        Mesh mesh = renderTarget.GetComponent<MeshFilter>().mesh = mesh = new Mesh();

        Vector3[] vertices = new Vector3[(xSize + 1) * (ySize + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        
        mesh.name = "Procedural Grid";

        vertices = new Vector3[(xSize + 1) * (ySize + 1)];
        for (int i = 0, y = 0; y <= ySize; y++)
        {
            for (int x = 0; x <= xSize; x++, i++)
            {
                vertices[i] = new Vector3(x, y);
                uv[i] = new Vector2((float)x / xSize, (float)y / ySize);
            }
        }
        mesh.vertices = vertices;
        mesh.uv = uv;

        int[] triangles = new int[xSize * ySize * 6];
        for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++)
        {
            for (int x = 0; x < xSize; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
                triangles[ti + 5] = vi + xSize + 2;
            }
        }
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    //generates point topology mesh according to parameters
    //used by point cloud method
    void generatePointCloud(Vector3 position, Vector3 scale, Vector2 pointcloudsize)
    {
        renderTarget = Instantiate(emptyPlane, position, Quaternion.Euler(new Vector3(0, 0, 0)));
        renderTarget.transform.localScale = new Vector3(scale.x, scale.y, 1);
        Mesh mesh = renderTarget.GetComponent<MeshFilter>().mesh = mesh = new Mesh();

        mesh.Clear();
        int numberX = (int)pointcloudsize.x;
        int numberY = (int)pointcloudsize.y;
        Vector3[] points = new Vector3[numberX * numberY];
        int[] indecies = new int[numberX * numberY];

        float xStep = 2560 / pointcloudsize.x;
        float yStep = 720 / pointcloudsize.y; 
        int count = 0;
        for (int i = 0; i < numberX; i++)
        {
            for (int j = 0; j < numberY; j++)
            {

                points[count] = new Vector3(i * (xStep/2), j * yStep, 0);
                indecies[count] = count;
                count++;
            }
        }


        Vector2[] uvs = new Vector2[points.Length];
        for (var i = 0; i < uvs.Length; i++) //Give UV coords X,Z world coords
            uvs[i] = new Vector2(points[i].x / numberX / (xStep/2), points[i].y / numberY / yStep );

        mesh.vertices = points;
        mesh.uv = uvs;
        mesh.SetIndices(indecies, MeshTopology.Points, 0);
    }

    public void clearRenderTarget()
    {
        if(renderTarget != null)
        {
            Destroy(renderTarget);
            renderTarget = null;
            renderTargetType = -1;
        }
    }
}
