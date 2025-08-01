using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Test
{
	public class GeneralInput : MonoBehaviour
	{
		public bool IsLocked => Cursor.lockState == CursorLockMode.Locked;
        void Update()
        {
            //엔터키 누르면, 커서잠금 or 해제.
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                ToggleCursor();
            }
        }


		private void ToggleCursor()
		{
            bool curLocked = Cursor.lockState == CursorLockMode.Locked;
			Cursor.lockState = curLocked ? CursorLockMode.None : CursorLockMode.Locked;
			Cursor.visible = !curLocked;
		}
	}
}
