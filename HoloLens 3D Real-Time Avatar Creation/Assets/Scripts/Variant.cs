using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Variant : MonoBehaviour
{
    public GameObject renderTarget;
    public Material material;
    // Start is called before the first frame update


    public void setRenderTarget(GameObject rT)
    {
        renderTarget = rT;
    }

    public void setMaterial(Material m)
    {
        material = m;
    }

    public void setRenderTarget_Material()
    {
        renderTarget.GetComponent<MeshRenderer>().material = material;
    }
}
