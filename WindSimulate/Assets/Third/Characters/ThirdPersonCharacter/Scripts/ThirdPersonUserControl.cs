using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using WindSimulation;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof (ThirdPersonCharacter))]
    public class ThirdPersonUserControl : MonoBehaviour
    {
        public GameObject m_Bullet = null;
        private ThirdPersonCharacter m_Character; // A reference to the ThirdPersonCharacter on the object
        private Transform m_Cam;                  // A reference to the main camera in the scenes transform
        private Vector3 m_CamForward;             // The current forward direction of the camera
        private Vector3 m_Move;
        private bool m_Jump;                      // the world-relative desired move direction, calculated from the camForward and user input.

        private WindMotorOmni m_WindMotorOmni = null;
        private void Start()
        {
            // get the transform of the main camera
            if (Camera.main != null)
            {
                m_Cam = Camera.main.transform;
            }
            else
            {
                Debug.LogWarning(
                    "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.", gameObject);
                // we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
            }

            // get the third person character ( this should never be null due to require component )
            m_Character = GetComponent<ThirdPersonCharacter>();
            m_WindMotorOmni = GetComponent<WindMotorOmni>();
        }


        private void Update()
        {
            if (!m_Jump)
            {
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }
            m_WindMotorOmni.m_Direction = transform.forward;
        }

        private float m_MobileHorizontal = 0;
        private float m_MobileVertical = 0;
        private void OnGUI()
        {
#if !UNITY_EDITOR
            if (GUI.Button(new Rect(0, Screen.height - 100, 100, 50), "Left"))
            {
                m_MobileHorizontal = -1;
            }
            else if (GUI.Button(new Rect(200, Screen.height - 100, 100, 50), "Right"))
            {
                m_MobileHorizontal = 1;
            }
            else if (GUI.Button(new Rect(100, Screen.height - 150, 100, 50), "Up"))
            {
                m_MobileVertical = 1;
            }
            else if (GUI.Button(new Rect(100, Screen.height - 50, 100, 50), "Down"))
            {
                m_MobileVertical = -1;
            }
            else if (GUI.Button(new Rect(Screen.width - 100, Screen.height - 150, 100, 50), "Shoot"))
            {
                GameObject item = GameObject.Instantiate(m_Bullet);
                item.transform.position = transform.position;

                Bullet bullet = item.GetComponent<Bullet>();
                bullet.m_Speed = transform.forward / 2f;
            }
            else if (GUI.Button(new Rect(Screen.width - 100, Screen.height - 50, 100, 50), "Stop"))
            {
                m_MobileVertical = 0;
                m_MobileHorizontal = 0;
            }
#endif
        }



        // Fixed update is called in sync with physics
        private void FixedUpdate()
        {
            // read inputs
#if UNITY_EDITOR
            float h = CrossPlatformInputManager.GetAxis("Horizontal");
            float v = CrossPlatformInputManager.GetAxis("Vertical");
#else
            float h = m_MobileHorizontal;
            float v = m_MobileVertical;
#endif
            bool crouch = Input.GetKey(KeyCode.C);

            // calculate move direction to pass to character
            if (m_Cam != null)
            {
                // calculate camera relative direction to move:
                m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
                m_Move = v*m_CamForward + h*m_Cam.right;
            }
            else
            {
                // we use world-relative directions in the case of no main camera
                m_Move = v*Vector3.forward + h*Vector3.right;
            }
#if !MOBILE_INPUT
			// walk speed multiplier
	        if (Input.GetKey(KeyCode.LeftShift)) m_Move *= 0.5f;
#endif

            // pass all parameters to the character control script
            m_Character.Move(m_Move, crouch, m_Jump);
            m_Jump = false;

            if (Input.GetKeyDown(KeyCode.F))
            {
                GameObject item = GameObject.Instantiate(m_Bullet);
                item.transform.position = transform.position;
               
                Bullet bullet = item.GetComponent<Bullet>();
                bullet.m_Speed = transform.forward / 2f;
            }
        }
    }
}
