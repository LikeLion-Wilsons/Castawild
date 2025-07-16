using TMPro;
using UnityEngine;

namespace Test
{
	public class NicknameUI : MonoBehaviour
	{
		public TMP_Text NicknameText;
		private Transform _cameraTransform;

		public void SetNickname(string nickname)
		{
			NicknameText.text = nickname;
		}

		private void Awake()
		{
			_cameraTransform = Camera.main.transform;
			NicknameText.text = string.Empty;
		}

		private void LateUpdate()
		{
			transform.rotation = _cameraTransform.rotation;
		}
	}
}
