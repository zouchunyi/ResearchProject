using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WindSimulation
{
    public class WindMotorOmni : MonoBehaviour
    {
        public float m_Force = 0;
        public float m_Radius = 0;
        public Vector3 m_Direction;

        private void OnEnable()
        {
            WindManager.instance.RegisterWind(this);
        }

        private void OnDisable()
        {
            WindManager.instance.UnregisterWind(this);
        }
    }
}
