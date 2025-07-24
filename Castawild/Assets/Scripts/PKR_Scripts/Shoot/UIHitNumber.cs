using UnityEngine;
using TMPro;

namespace Test.Shoot
{
	public class UIHitNumber : MonoBehaviour
	{
		[SerializeField]
		private TextMeshProUGUI _text;

		public void SetNumber(int value)
        {
            int intValue = Mathf.RoundToInt(value);
            _text.text = intValue.ToString();
        }

	}
}
