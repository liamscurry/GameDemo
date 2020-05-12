using UnityEngine;

//Timer which turns off the attached particle and recycles the projectile upon completion.

public class ParticleProjectileTimer : MonoBehaviour 
{
	[SerializeField]
	private ParticleProjectile projectile;
	[SerializeField]
	private float duration;

	private ParticleSystem deathParticle;
	private float timer;

	private void Awake()
	{
		deathParticle = GetComponent<ParticleSystem>();
	}

	//Timer
	private void Update() 
	{
		timer += Time.deltaTime;

		if (timer >= duration)
		{				
			gameObject.SetActive(false);
			deathParticle.Stop();
			projectile.Recycle();
			timer = 0;	
		}
	}
}