using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CoreFramework 
{
    public class JoystickPanel : MonoBehaviour 
    {
        public static JoystickPanel Instance = null;
		public Transform m_JoystickTrans = null;
		public Image m_JoystickBorder = null;
        public Button m_AttackButton = null;

        public FixedJoystickHandler m_LeftJoystickHandler = null;
        public FixedJoystickHandler m_RightJoystickHandler = null;

        private const float AUTO_HIDE_TIME = 5f;

		private Matrix4x4 m_RotationMatrix = new Matrix4x4 ();
        private Action<float,float> m_PositionAction = null;

        private Matrix4x4 m_CameraRotationMatrix = new Matrix4x4();
        private Action<float, float> m_CameraAction = null;

        public void SetPositionAction(Action<float, float> action, float offsetAngle)
        {
            m_PositionAction = action;

            float angle = (Mathf.PI / 180f) * offsetAngle;
            m_RotationMatrix.SetRow(0, new Vector4(Mathf.Cos(angle), 0, -Mathf.Sin(angle), 0));
            m_RotationMatrix.SetRow(1, new Vector4(0, 1, 0, 0));
            m_RotationMatrix.SetRow(2, new Vector4(Mathf.Sin(angle), 0, Mathf.Cos(angle), 0));
            m_RotationMatrix.SetRow(3, new Vector4(0, 0, 0, 1));
        }

        public void SetCameraAction(Action<float, float> action, float offsetAngle)
        {
            m_CameraAction = action;

            float angle = (Mathf.PI / 180f) * offsetAngle;
            m_CameraRotationMatrix.SetRow(0, new Vector4(Mathf.Cos(angle), 0, -Mathf.Sin(angle), 0));
            m_CameraRotationMatrix.SetRow(1, new Vector4(0, 1, 0, 0));
            m_CameraRotationMatrix.SetRow(2, new Vector4(Mathf.Sin(angle), 0, Mathf.Cos(angle), 0));
            m_CameraRotationMatrix.SetRow(3, new Vector4(0, 0, 0, 1));
        }

        public void SetAttackAction(UnityAction action)
        {
            m_AttackButton.onClick.AddListener(action);
        }

        void Awake() 
        {
			Instance = this;
			InvokeRepeating ("JoystickCheck", 0, 0.1f);
        }

        void OnDestroy()
        {
			Instance = null;
        }
			
        void JoystickCheck()
        {
            if (m_LeftJoystickHandler != null)
            {
				Vector2 value = m_LeftJoystickHandler.GetVector2 ();
                if (m_PositionAction != null)
                {
                    Vector3 res = m_RotationMatrix.MultiplyPoint3x4(new Vector3(value.x,0,value.y));
                    m_PositionAction(res.x, res.z);
                }
            }

            if (m_RightJoystickHandler != null)
            {
                Vector2 value = m_RightJoystickHandler.GetVector2();
                if (m_CameraAction != null)
                {
                    Vector3 res = m_CameraRotationMatrix.MultiplyPoint3x4(new Vector3(value.x, 0, value.y));
                    m_CameraAction(res.x, res.z);
                }
            }
        }

		public void SetJoystick(Vector3 screenPosition)
        {
			float scale = 720f / Screen.height;
			Vector3 viewPos = new Vector3((screenPosition.x - Screen.width / 2) * scale, (screenPosition.y - Screen.height / 2) * scale, 0);
            m_JoystickTrans.localPosition = viewPos;
		}
    }
}