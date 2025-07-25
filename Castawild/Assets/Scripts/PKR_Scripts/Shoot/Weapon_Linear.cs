using Fusion;
using UnityEngine;

namespace Test.Shoot
{
	public class Weapon_Linear : NetworkBehaviour
	{

        [SerializeField] private Transform _fireTransform;
        
		[SerializeField] private float _speed = 50f;
		[SerializeField] private LayerMask _hitMask;
		[SerializeField] private float _hitImpulse = 50f;
		[SerializeField] private float _lifeTime = 4f;
		[SerializeField] private float _lifeTimeAfterHit = 2f;
		[SerializeField] private DummyProjectile _dummyProjectilePrefab;

		[Networked] private int _fireCount { get; set; }
		[Networked, Capacity(64)] private NetworkArray<ProjectileData> _projectileData { get; }

		private DummyProjectile[] _projectiles = new DummyProjectile[64];

		private int _visibleFireCount;

		public void Fire()
		{
			_projectileData.Set(_fireCount % _projectileData.Length, new ProjectileData()
			{
				FireTick = Runner.Tick,
				FirePosition = _fireTransform.position,
				FireVelocity = _fireTransform.forward * _speed,
				FinishTick = Runner.Tick + Mathf.RoundToInt(_lifeTime / Runner.DeltaTime),
			});

			_fireCount++;
		}

		public override void Spawned()
		{
			_visibleFireCount = _fireCount;
		}

		public override void FixedUpdateNetwork()
		{
			int tick = Runner.Tick;

			for (int i = 0; i < _projectileData.Length; i++)
			{
				var data = _projectileData[i];

				if (data.IsActive == false) continue;
				if (data.FinishTick <= tick) continue;

				UpdateProjectile(ref data, tick);

				_projectileData.Set(i, data);
			}
		}

		public override void Render()
		{
			if (_visibleFireCount < _fireCount)
			{
				PlayFireEffect();
			}

			// Instantiate missing projectile objects
			for (int i = _visibleFireCount; i < _fireCount; i++)
			{
				int index = i % _projectileData.Length;
				var data = _projectileData[index];

				var previousProjectile = _projectiles[index];
				if (previousProjectile != null)
				{
					Destroy(previousProjectile.gameObject);
				}

				var projectile = Instantiate(_dummyProjectilePrefab, data.FirePosition, Quaternion.LookRotation(data.FireVelocity));

				_projectiles[index] = projectile;
			}

			// For proxies we move projectiles in remote time frame, for input/state authority we use local time frame
			float renderTime = Object.IsProxy == true ? Runner.RemoteRenderTime : Runner.LocalRenderTime;
			float floatTick = renderTime / Runner.DeltaTime;

			// Update projectile visuals
			for (int i = 0; i < _projectiles.Length; i++)
			{
				var projectile = _projectileData[i];
				var projectileObject = _projectiles[i];

				if (projectile.IsActive == false || projectile.FinishTick < floatTick)
				{
					if (projectileObject != null)
					{
						Destroy(projectileObject.gameObject);
					}

					continue;
				}

				if (projectile.HitPosition != Vector3.zero)
				{
					projectileObject.transform.position = projectile.HitPosition;
					//projectileObject.ShowHitEffect();
				}
				else
				{
					projectileObject.transform.position = GetMovePosition(ref projectile, floatTick);
				}
			}

			_visibleFireCount = _fireCount;
		}

		private void UpdateProjectile(ref ProjectileData projectileData, int tick)
		{
			if (projectileData.HitPosition != Vector3.zero) return;
				

			var previousPosition = GetMovePosition(ref projectileData, tick - 1f);
			var nextPosition = GetMovePosition(ref projectileData, tick);

			var direction = nextPosition - previousPosition;

			float distance = direction.magnitude;
			direction /= distance; // Normalize

			var hitOptions = HitOptions.IncludePhysX | HitOptions.IgnoreInputAuthority;

			if (Runner.LagCompensation.Raycast(previousPosition, direction, distance, Object.InputAuthority, out var hit, _hitMask, hitOptions))
			{
				projectileData.HitPosition = hit.Point;
				projectileData.FinishTick = tick + Mathf.RoundToInt(_lifeTimeAfterHit / Runner.DeltaTime);
				
				//do something with hit.
                if (hit.Hitbox != null)
                {
                    var target = hit.Hitbox.Root.GetComponent<IDamageable>();
                    if (target != null)
                    {
                        target.TakeDamage(10);
                    }    
                }
                
			}
		}

		private Vector3 GetMovePosition(ref ProjectileData data, float currentTick)
		{
			float time = (currentTick - data.FireTick) * Runner.DeltaTime;

			if (time <= 0f)
				return data.FirePosition;

			//그냥 등속운동.
			return data.FirePosition + data.FireVelocity * time;
		}
		private struct ProjectileData : INetworkStruct
		{
			public bool IsActive => FireTick > 0;

			public int FireTick;
			public int FinishTick;

			public Vector3 FirePosition;
			public Vector3 FireVelocity;

			public Vector3 HitPosition { get; set; }
		}
        private void PlayFireEffect()
        {
            //play sound.
            //play particle.
            //...
        }
	}
}
