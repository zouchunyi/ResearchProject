using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowDynamicTexture : MonoBehaviour
{
    public Camera m_Camera = null;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 postion = m_Camera.ScreenToWorldPoint(new Vector3(200, Screen.height - 200, 5));
        this.transform.position = postion;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
