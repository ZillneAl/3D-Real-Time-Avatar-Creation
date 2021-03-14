using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    public GameObject MP4Manager;
    public GameObject StreamReciever;
    public GameObject ObjectManager;
    public GameObject VariantManager;

    [Header("Create Variants")]
    public bool MeshCreate;
    public bool PointCloud;
    public bool DShader;
    public bool VShader;

    [Header("Load Variants")]
    public bool MP4;
    public bool Stream;

    private int create_current = -1;
    private int load_current = -1;

    private int create_new = -1;
    private int load_new = -1;

    private bool option_change_create = false;
    private bool option_change_load = false;

    // Start is called before the first frame update
    void Start()
    {
        ObjectManager.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (option_change_load || option_change_create) //reacts to changes in the editor or calls via buttonpress 
        {
            if (option_change_create)
            {
                option_change_create = false;
                changeVariant(create_new);
            }
                
            else
            {
                option_change_load = false;
                changeLoadMethod(load_new);
            }
        }
    }

    //reacts to changes in editor mode
    private void OnValidate()  
    {
        bool change = false;

        if(MeshCreate)
        {
            change = true;
            create_new = 1;
            option_change_create = true;
        }

        else if (PointCloud)
        {
            change = true;
            create_new = 2;
            option_change_create = true;
        }

        else if (DShader)
        {
            change = true;
            create_new = 3;
            option_change_create = true;

        }
        else if (VShader)
        {
            change = true;
            create_new = 4;
            option_change_create = true;
        }
        else if (!change)
        {
            change = true;
            create_new = -1;
            option_change_create = true;
        }

        change = false;
        
        if (MP4 && load_current != 1)
        {
            load_new = 1;
            option_change_load = true;
            change = true;
        }
        else if (Stream && load_current != 2)
        {
            load_new = 2;
            option_change_load = true;
            change = true;
        }
        else if (!change)
        {
            load_new = -1;
            option_change_load = true;
            change = true;
        }
            
    }

    //communication with VariantManager
    private void changeVariant(int varNum)
    {
        if (varNum == create_current)
            return;

        if (create_current == -1)
            VariantManager.SetActive(true);

        deselectCurrent(create_current);
        create_current = varNum;
        ObjectManager.GetComponent<ObjectManager>().clearRenderTarget();
        VariantManager.GetComponent<VariantManager>().requestVariant(varNum);

    }

    //communication with either Mp4Manager or StreamReciever
    private void changeLoadMethod(int methNum)
    {
        if (load_current == methNum)
            return;

        switch (methNum)
        {
            case -1:
                MP4Manager.SetActive(false);
                StreamReciever.SetActive(false);
                break;
            case 1:
                MP4Manager.SetActive(true);
                break;
            case 2:
                StreamReciever.SetActive(true);
                break;
            default:
                break;
        }

        load_current = methNum;
    }

    // inhibits more than one variant being active at the same time
    private void deselectCurrent(int varNum)
    {
        switch (varNum)
        {
            case -1:
                break;
            case 1:
                MeshCreate = false;
                break;
            case 2:
                PointCloud = false;
                break;
            case 3:
                DShader = false;
                break;
            case 4:
                VShader = false;
                break;
            default:
                Debug.Log("Unknown Variant");
                break;
        }


    }

    //called by buttons changes the variant that creates the 3D model
    public void changeCreateOnButtonPress(int CreateID)
    {
        Debug.Log("Changing Create Method to: " + CreateID);
        create_new = CreateID;
        option_change_create = true;
    }

    //called by buttons changes the method data is loaded into the rendertexture
    public void changeLoadOnButtonPress(int LoadID)
    {
        Debug.Log("Changing Load Method to: " + LoadID);
        load_new = LoadID;
        option_change_load = true;
    }

    //callthrough for request for ObjectManager
    public GameObject requestRenderTarget()
    {
        return ObjectManager.GetComponent<ObjectManager>().requestRenderTarget();
    }
}
