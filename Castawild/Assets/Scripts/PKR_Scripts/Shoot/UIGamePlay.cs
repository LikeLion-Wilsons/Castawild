using System;
using UnityEngine;

namespace Test.Shoot
{
    public class UIGamePlay : MonoBehaviour
    {
        [SerializeField] private UICrosshair _crosshair;
        [SerializeField] private UIHitNumbers _hitNumber;

        void OnEnable()
        {
            DummyTarget.onDamaged += OnTargetDamaged;
        }

        void OnDisable()
        {
            DummyTarget.onDamaged -= OnTargetDamaged;
        }

        private void OnTargetDamaged(int damage)
        {
            _crosshair.OnHit();
            _hitNumber.OnHit(damage);
        }
    }
}