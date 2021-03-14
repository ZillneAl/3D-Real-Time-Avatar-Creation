using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameCounter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    int m_frameCounter = 0;
    float m_timeCounter = 0.0f;
    float m_lastFramerate = 0.0f;
    public float m_refreshTime = 10.0f;
    public bool start = false;

    void Update()
    {
        if (!start)
            return;
        if (m_timeCounter < m_refreshTime)
        {
            m_timeCounter += Time.deltaTime;
            m_frameCounter++;
        }
        else
        {
            m_lastFramerate = (float)m_frameCounter / m_timeCounter;
            Debug.Log("average framrate of last "+ m_refreshTime + "s = "+ m_lastFramerate);
            m_frameCounter = 0;
            m_timeCounter = 0.0f;
        }
    }

}
