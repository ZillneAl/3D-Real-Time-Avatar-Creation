using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class MP4Manager : MonoBehaviour
{
    public UnityEngine.Video.VideoClip video;
    public GameObject SceneController;
    public GameObject renderTarget;
    public RenderTexture renderTexture;
    
    public int videoNum = 0;
    public bool createPlane = false;

    private UnityEngine.Video.VideoPlayer videoPlayer;

    // Start is called before the first frame update
    void Start()
    {
        if(createPlane)
        {
            renderTarget = SceneController.GetComponent<SceneController>().requestRenderTarget();
            videoPlayer = renderTarget.AddComponent<UnityEngine.Video.VideoPlayer>();
        }
        else
        {
            videoPlayer = gameObject.AddComponent<UnityEngine.Video.VideoPlayer>();
        }
        videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.targetCameraAlpha = 0.5F;
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = true;


        startVideoByNum(videoNum);
    }

    private void Init()
    {
        videoPlayer = renderTarget.AddComponent<UnityEngine.Video.VideoPlayer>();
        videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.targetCameraAlpha = 0.5F;
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = true;


        startVideoByNum(videoNum);
    }

    private void startVideoByNum(int videoID)
    {
        switch (videoID)
        {
            case 0:
                videoPlayer.clip = video;
                break;

            case 1:
                videoPlayer.url = Application.dataPath + @"/Resources/colorizer_300_2000_lowsun_medium.mp4";
                break;
            case 2:
                videoPlayer.url = Application.dataPath + @"/Resources/col_300_2000_chair.mp4";
                break;
            case 3:
                videoPlayer.url = Application.dataPath + @"/Resources/col_300_2000_gq.mp4";
                break;
            case 4:
                videoPlayer.url = Application.dataPath + @"/Resources/col_300_2000_head.mp4";
                break;
            case 5:
                videoPlayer.url = Application.dataPath + @"/Resources/ohneunterlage.mp4";
                break;
            case 6:
                videoPlayer.url = Application.dataPath + @"/Resources/chair.mp4";
                break;
            case 7:
                videoPlayer.url = Application.dataPath + @"/Resources/earlymorning_dark 1.mp4";
                break;
            case 8:
                videoPlayer.url = Application.dataPath + @"/Resources/colorizer_300-2000_lowsun.mp4";
                break;
            default:
                break;
        }
        videoPlayer.Play();
    }

    private void OnDisable()
    {
        if(videoPlayer != null )
            videoPlayer.Stop();
    }

    private void OnEnable()
    {
        if (videoPlayer != null)
            videoPlayer.Play();
    }

    public void videoStop()
    {
        videoPlayer.Play();
    }

    public void videoPlay()
    {
        videoPlayer.Stop();
    }

    public void setRenderTarget(GameObject rT)
    {
        renderTarget = rT;
    }

    // Update is called once per frame
    void Update()
    {
        return;
    }
}
