
using UnityEngine;

public class PointCloudVariant : Variant
{
    public ComputeShader computeShader;
    public RenderTexture renderTexture;

    public float _Farcut = 0.01f;
    private RenderTexture rt;

    int number = 100;
    private ComputeBuffer depthbuffer;
    private ComputeBuffer colorbuffer;


    public struct Point
    {
        public Vector3 position;
        public Color color;
    }

    void Start()
    {
        
    }

    void Update()
    {

    }

    void init()
    {
        
    }

    void Meshtest()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;

        mesh.Clear();

        int numberL = 100;
        Vector3[] points = new Vector3[numberL];
        int[] indecies = new int[numberL];
        Color[] colors = new Color[numberL];


        int count = 0;
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {

                points[count] = new Vector3(i * 0.15f, j * 0.15f, 0);
                indecies[count] = count;
                colors[count] = new Color(1, 1, 1, 1);
            }
        }
        Debug.Log(count);

        mesh.vertices = points;
        mesh.colors = colors;
        mesh.SetIndices(indecies, MeshTopology.Points, 0);
        //renderTarget.GetComponent<MeshFilter>().mesh = mesh;
    }

    void MeshWithoutCompute()
    {
        Mesh mesh = renderTarget.GetComponent<MeshFilter>().mesh;

        mesh.Clear();
        int numberX = 960 / 4;
        int numberY = 720 / 2;
        Vector3[] points = new Vector3[numberX * numberY];
        int[] indecies = new int[numberX * numberY];
        Color[] colors = new Color[numberX * numberY];

        RenderTexture.active = renderTexture;
        Texture2D hmap = new Texture2D(renderTexture.width, renderTexture.height);
        //hmap.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        hmap.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0, false);
        hmap.Apply();

        //float xStep = 1 + (numberX / 960);
        float xStep = 960 / numberX;
        float yStep = 720 / numberY;
        int count= 0;
        int offx = Random.Range(0, 3);
        int offy = Random.Range(0, 1);
        for (int i = 0; i < numberX; i++)
        {
            for( int j = 0; j < numberY; j++){

                float depth = colorToDepth(hmap.GetPixel((int)(i * 4 + 1600 + offx), (j+offy) * 2));
                points[count] = new Vector3((i+offx)* xStep, j * yStep, depth);
                indecies[count] = count;
                if(depth > _Farcut)
                {
                    colors[count] = hmap.GetPixel((int)(i * 4 + 320 + offx ), (j+offy) * 2);
                }
                else
                {
                    colors[count] = new Color(0, 0, 0, 0);
                }
                
                count++;
            }
        }

        mesh.vertices = points;
        mesh.colors = colors;
        mesh.SetIndices(indecies, MeshTopology.Points, 0);
        //renderTarget.GetComponent<MeshFilter>().mesh = mesh;
    }

    //not implemented fully
    void MeshApproach()
    {
        Mesh mesh = new Mesh();

        Graphics.CopyTexture(rt, renderTexture);

        Vector3[] points = new Vector3[number];
        int[] indecies = new int[number];
        Color[] colors = new Color[number];

        ComputeBuffer pointsBuffer = new ComputeBuffer(number, sizeof(float)* 3);
        ComputeBuffer indeciesBuffer = new ComputeBuffer(number, sizeof(int));
        ComputeBuffer colorsBuffer = new ComputeBuffer(number, sizeof(float) * 4);

        
        pointsBuffer.SetData(points);
        indeciesBuffer.SetData(indecies);
        colorsBuffer.SetData(colors);

        computeShader.SetBuffer(0, "points", pointsBuffer);
        computeShader.SetBuffer(1, "indecies", indeciesBuffer);
        computeShader.SetBuffer(0, "colors", colorsBuffer);

        computeShader.SetTexture(0, "_RTexture", renderTexture);
        computeShader.Dispatch(0, 8, 8, 1);

        computeShader.Dispatch(1, indecies.Length/10,1, 1);

        pointsBuffer.GetData(points);
        indeciesBuffer.GetData(indecies);
        colorsBuffer.GetData(colors);

        Debug.Log(indecies[0]);
        Debug.Log(indecies[2]);
        Debug.Log(indecies[1]);

        mesh.vertices = points;
        mesh.colors = colors;
        mesh.SetIndices(indecies, MeshTopology.Points, 0);
        renderTarget.GetComponent<MeshFilter>().mesh = mesh;

        pointsBuffer.Dispose();
        indeciesBuffer.Dispose();
        colorsBuffer.Dispose();
        Debug.Log("hoi " + pointsBuffer.count);
    }

    //not implemented fully
    void DPNApprach()
    {
        int colorSize = sizeof(float) * 4;
        int positionSize = sizeof(float) * 3;
        Point[] data = new Point[number];

        ComputeBuffer pointBuffer = new ComputeBuffer(number, colorSize + positionSize);
        pointBuffer.SetData(data);

        computeShader.SetBuffer(0, "points", pointBuffer);
        computeShader.SetTexture(0, "_RTexture", renderTexture);
        computeShader.Dispatch(0, colorSize + positionSize / 8, 1, 1);

        pointBuffer.GetData(data);

        material.SetPass(0);
        material.SetBuffer("_PointBuffer", pointBuffer);
        Graphics.DrawProceduralNow(MeshTopology.Points, pointBuffer.count, 1);

        pointBuffer.Dispose();
    }

    void OnPostRender()
    {
    }

    void OnDestroy()
    {
    }

    void CreateRenderTexture( int w, int h)
    {
        renderTexture = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();
    }

    //Maybe zxy
    float colorToDepth(Vector4 col)
    {

        int red = (int)(col.x * 256);
        int green = (int)(col.y * 256);
        int blue = (int)(col.z * 256);
        int a = red | green << 8 | blue << 16;
        return ((float)a) / 0xFFFFFF;
    }
}
