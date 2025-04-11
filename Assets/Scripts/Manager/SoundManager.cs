using UnityEngine;

public class SoundManager : MonoBehaviour
{
	public static SoundManager Instance { get; private set; }

	[System.Serializable]
	public class Sound
	{
		[HideInInspector] public string name;
		public AudioClip clip;
		[Range(0f, 1f)] public float volume = 1f;
		[Range(0.1f, 3f)] public float pitch = 1f;
		public bool loop = false;

		[HideInInspector]
		public AudioSource source;
	}

	public enum SoundType { Death, Pickup, Ghost, Bonus }

	[System.Serializable]
	public class SoundEntry
	{
		public SoundType type;
		public Sound settings;
	}

	public SoundEntry[] soundEntries;

	private void Awake()
	{
		transform.SetParent(null);

		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
			InitializeAudioSources();
		}
		else if (Instance != this)
		{
			Destroy(gameObject);
			return;
		}
	}

	private void InitializeAudioSources()
	{
		foreach (SoundEntry entry in soundEntries)
		{
			entry.settings.source = gameObject.AddComponent<AudioSource>();
			entry.settings.source.clip = entry.settings.clip;
			entry.settings.source.volume = entry.settings.volume;
			entry.settings.source.pitch = entry.settings.pitch;
			entry.settings.source.loop = entry.settings.loop;
			entry.settings.source.playOnAwake = false; 
		}
	}

	public void Play(SoundType soundType)
	{
		SoundEntry entry = System.Array.Find(soundEntries, e => e.type == soundType);
		if (entry == null)
		{
			Debug.LogWarning($"Sound type: {soundType} not found!");
			return;
		}
		entry.settings.source.Play();
	}

	public void Stop(SoundType soundType)
	{
		SoundEntry entry = System.Array.Find(soundEntries, e => e.type == soundType);
		if (entry == null)
		{
			Debug.LogWarning($"Sound type: {soundType} not found!");
			return;
		}
		entry.settings.source.Stop();
	}

	public void PlayDeathSound() => Play(SoundType.Death);
	public void PlayPickupSound() => Play(SoundType.Pickup);
	public void PlayGhostSound() => Play(SoundType.Ghost);
	public void PlayBonusSound() => Play(SoundType.Bonus);
}