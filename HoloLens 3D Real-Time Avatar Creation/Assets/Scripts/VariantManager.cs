using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VariantManager : MonoBehaviour
{
    public GameObject ObjectManager;
    public Variant aktiveVariant;
    public Vector2 customPlaneSize;
    public Vector2 pointCloudSize;

    private GameObject renderTarget;
    private int create_current = -1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // gets called by SceneController and activates requested variant 
    public void requestVariant(int newVar)
    {
        if (newVar == create_current)
            return;

        create_current = newVar;
        
        switch (newVar)
        {
            case -1:
                if(aktiveVariant != null)
                    aktiveVariant.enabled = false;
                return;
            case 1://Mesh
                renderTarget = ObjectManager.GetComponent<ObjectManager>().requestPlanePrefab();
                aktiveVariant = GetComponent<MeshCreateVariant>();
                break;
            case 2://PC
                renderTarget = ObjectManager.GetComponent<ObjectManager>().requestPointCloud(new Vector3(-0.6f,-0.5f,3),new Vector3(0.001f,0.001f,1), pointCloudSize);
                aktiveVariant = GetComponent<PointCloudVariant>();
                break;
            case 3://DShader
                renderTarget = ObjectManager.GetComponent<ObjectManager>().requestPlanePrefabCustom((int)customPlaneSize.x,(int) customPlaneSize.y, new Vector2(0.007f,0.007f), new Vector3(-0.7f,-1,3));
                aktiveVariant = GetComponent<DisplacementVariant>();
                break;
            case 4://VShader
                renderTarget = ObjectManager.GetComponent<ObjectManager>().requestCubePrefab(new Vector3(0,0,3),new Vector3(1,1,1));
                aktiveVariant = GetComponent<VolumetricVariant>();
                break;
            default:
                Debug.Log("Unknown Variant");
                break;

        }
        aktiveVariant.enabled = true;
        aktiveVariant.setRenderTarget(renderTarget);
        aktiveVariant.setRenderTarget_Material();
    }

    public GameObject requestRenderTarget()
    {
        return ObjectManager.GetComponent<ObjectManager>().requestRenderTarget();
    }
}
