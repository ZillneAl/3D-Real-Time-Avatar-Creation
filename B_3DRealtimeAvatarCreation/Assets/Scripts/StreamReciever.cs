using UnityEngine;
using System;
using LibVLCSharp;

public class StreamReciever : MonoBehaviour
{
    public GameObject SceneController;
    public RenderTexture renderTexture;
    public GameObject renderTarget;

    LibVLC _libVLC;
    MediaPlayer _mediaPlayer;
    const int seekTimeDelta = 5000;
    Texture2D tex = null;
    Texture2D tex2 = null;
    bool playing;
    const string localMedia = "rtsp://172.27.5.136:8554/kinect";

    void Awake()
    {
        Core.Initialize(Application.dataPath);

        _libVLC = new LibVLC("--no-osd", "--verbose=2");

        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        //_libVLC.Log += (s, e) => UnityEngine.Debug.Log(e.FormattedLog); // enable this for logs in the editor
        renderTarget = SceneController.GetComponent<SceneController>().requestRenderTarget();
        PlayPause();
    }

    public void SeekForward()
    {
        Debug.Log("[VLC] Seeking forward !");
        _mediaPlayer.SetTime(_mediaPlayer.Time + seekTimeDelta);
    }

    public void SeekBackward()
    {
        Debug.Log("[VLC] Seeking backward !");
        _mediaPlayer.SetTime(_mediaPlayer.Time - seekTimeDelta);
    }

    void OnDisable()
    {
        _mediaPlayer?.Stop();
        _mediaPlayer?.Dispose();
        _mediaPlayer = null;

        _libVLC?.Dispose();
        _libVLC = null;
    }

    public void PlayPause()
    {
        Debug.Log("[VLC] Toggling Play Pause !");
        if (_mediaPlayer == null)
        {
            _mediaPlayer = new MediaPlayer(_libVLC);
        }
        if (_mediaPlayer.IsPlaying)
        {
            _mediaPlayer.Pause();
        }
        else
        {
            playing = true;

            if (_mediaPlayer.Media == null)
            {
                // playing remote media
                _mediaPlayer.Media = new Media(_libVLC, new Uri(localMedia));
            }

            _mediaPlayer.Play();
        }

        if (renderTexture != null)
        {
            renderTarget.GetComponent<Renderer>().material.mainTexture = renderTexture;
        }
    }

    public void Stop()
    {
        Debug.Log("[VLC] Stopping Player !");

        playing = false;
        _mediaPlayer?.Stop();

        // there is no need to dispose every time you stop, but you should do so when you're done using the mediaplayer and this is how:
        // _mediaPlayer?.Dispose(); 
        // _mediaPlayer = null;
        renderTarget.GetComponent<Renderer>().material.mainTexture = null;
        tex = null;
    }

    void Update()
    {
        if (!playing) return;

        RenderTexture.active = renderTexture;
        if (tex == null)
        {
            // If received size is not null, it and scale the texture
            uint i_videoHeight = 0;
            uint i_videoWidth = 0;

            _mediaPlayer.Size(0, ref i_videoWidth, ref i_videoHeight);
            var texptr = _mediaPlayer.GetTexture(out bool updated);
            if (i_videoWidth != 0 && i_videoHeight != 0 && updated && texptr != IntPtr.Zero)
            {
                Debug.Log("Creating texture with height " + i_videoHeight + " and width " + i_videoWidth);
                tex = Texture2D.CreateExternalTexture((int)i_videoWidth,
                    (int)i_videoHeight,
                    TextureFormat.RGBA32,
                    false,
                    true,
                    texptr);
                renderTarget.GetComponent<Renderer>().material.mainTexture = renderTexture;
            }
        }
        else if (tex != null)
        {
            var texptr = _mediaPlayer.GetTexture(out bool updated);
            if (updated)
            {
                tex.UpdateExternalTexture(texptr);
                Graphics.Blit(tex, renderTexture);
                RenderTexture.active = null;

            }
        }
    }

}
