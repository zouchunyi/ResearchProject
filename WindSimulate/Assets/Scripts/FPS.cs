using UnityEngine;

namespace CoreFramework
{
    public class FPS : MonoBehaviour 
    {
	    public float m_UpdateInterval = 0.5F;
        private double m_LastInterval = 0;
	    private int m_Frames = 0;
        private float m_Fps = 0.0f;

	    private void Start()
	    {
		    m_LastInterval = Time.realtimeSinceStartup;
		    m_Frames = 0;
	    }

	    private void Update()
	    {
		    ++m_Frames;
		    float timeNow = Time.realtimeSinceStartup;
		    if (timeNow > m_LastInterval + m_UpdateInterval)
		    {
                m_Fps = (float)(m_Frames / (timeNow - m_LastInterval));
			    m_Frames = 0;
			    m_LastInterval = timeNow;
		    }
	    }

	    public float Fps
	    {
		    get 
            { 
                return m_Fps; 
            }
	    }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(0, 0, 100, 50), m_Fps.ToString()))
            {
            
            }
        }
    }
}
