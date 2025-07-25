using System.Collections.Generic;
using UnityEngine;

namespace Test.Shoot
{
	public class UIHitNumbers : MonoBehaviour
	{

		[SerializeField] private UIHitNumber _hitItem;

		public void OnHit(int damage)
		{
            var hitItem = Instantiate(_hitItem, _hitItem.transform.parent);
            hitItem.SetNumber(damage);
            
            hitItem.gameObject.SetActive(true);
            hitItem.transform.SetAsLastSibling();
		}

		protected void Awake()
		{
			_hitItem.gameObject.SetActive(false);
		}
	}
}
