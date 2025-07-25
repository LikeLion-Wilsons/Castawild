using System.Collections;
using UnityEngine;

namespace Test.Shoot
{
	public class UICrosshair : MonoBehaviour
	{
		[SerializeField] private CanvasGroup _hitPerformedGroup;
		[SerializeField] private float _hitGroupDelay = 0.15f;
		[SerializeField] private float _hitGroupFadeInDuration = 0.1f;
		[SerializeField] private float _hitGroupFadeOutDuration = 0.8f;

        private Coroutine co;
		public void OnHit()
		{
            if (co != null)
            {
                StopCoroutine(co);
            }
            _hitPerformedGroup.alpha = 0f; // 초기화
            co = StartCoroutine(ShowHitEffect());
		}

        IEnumerator ShowHitEffect()
        {
            // 초기 딜레이
            yield return new WaitForSeconds(_hitGroupDelay);
            
            // 페이드 인
            float elapsedTime = 0f;
            while (elapsedTime < _hitGroupFadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = elapsedTime / _hitGroupFadeInDuration;
                _hitPerformedGroup.alpha = Mathf.Lerp(0f, 1f, normalizedTime);
                yield return null;
            }
            _hitPerformedGroup.alpha = 1f;
            
            // 잠시 대기
            yield return new WaitForSeconds(0.1f);
            
            // 페이드 아웃
            elapsedTime = 0f;
            while (elapsedTime < _hitGroupFadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = elapsedTime / _hitGroupFadeOutDuration;
                _hitPerformedGroup.alpha = Mathf.Lerp(1f, 0f, normalizedTime);
                yield return null;
            }
            _hitPerformedGroup.alpha = 0f;
            co = null;
        }

	}
}