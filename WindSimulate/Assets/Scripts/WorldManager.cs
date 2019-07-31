using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public GameObject m_CopyItem = null;
    public GameObject m_Player = null;

    private List<IndirectDrawBuffer> m_IndirectDrawList = new List<IndirectDrawBuffer>();

    private void Start()
    {
        Application.targetFrameRate = 60;
    }

    private void OnGUI()
    {
        int sum = 0;
        for (int i = 0; i < m_IndirectDrawList.Count; ++i)
        {
            sum += m_IndirectDrawList[i].InstanceSize();
        }
        if (GUI.Button(new Rect(Screen.width - 100, 0, 100, 50), sum.ToString()))
        {
            Create();
        }
    }

    private void Create()
    {
        IndirectDrawBuffer newBuffer = new IndirectDrawBuffer(m_CopyItem, 10, 1023);
        m_IndirectDrawList.Add(newBuffer);
    }

    private float m_Countdown = 1f;
    private void Update()
    {
        if (m_Countdown > 0)
        {
            m_Countdown -= Time.deltaTime;
            if (m_Countdown <= 0)
            {
                m_Player.SetActive(true);
            }
        }

        Draw();
    }

    private void Draw()
    {
        for (int i = 0; i < m_IndirectDrawList.Count; ++i)
        {
            m_IndirectDrawList[i].Draw();
        }
    }
}
