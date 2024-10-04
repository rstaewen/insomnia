#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Audio : SolipsoBehavior
{
	[System.Serializable] public class ProbableClip
	{
		[HideInInspector] public string name;
		public AudioClip clip;
		public float probability = 1f;
	}
	[System.Serializable] public class ClipSet
	{
		public List<ProbableClip> clips;
		public float volume;
		public float minimumDelayPerPlay;
		public float totalProbability {get {return clips.Select<ProbableClip, float>(pc => pc.probability).Sum();}}
		[HideInInspector] public float delayTime;
	}
	public AudioSource musicSource {get {return musicSources[activeMusicIndex];}}
	public List<AudioSource> musicSources;
	public int activeMusicIndex;
	public float fadeUpMusicTime;

	public ClipSet normalBaas;
	public ClipSet busyBaas;
	public ClipSet heroBaas;
	public ClipSet spaceBaas;
	public ClipSet fearBaas;
	public ClipSet blackBaas;
	public ClipSet knifeSlashes;
	public ClipSet crashes;
	public ClipSet ticks;
	public ClipSet victories;
	public ClipSet smokeRelease;
	public ClipSet normalDeaths;
	public ClipSet explosiveDeaths;
	public ClipSet spaceDeaths;
	public ClipSet busyDeaths;
	public ClipSet heroDeaths;
	public ClipSet normalFalls;
	private static Audio instance;
	public List<AudioSource> sourcePool;
	private int sourcePoolIndex;
	private List<ClipSet> allClipSets = new List<ClipSet>();
	private bool canPlayClips = true;
	private float musicVolume;
	private float fxVolume;
	private float baseMusicVolume;
	
	void Awake()
	{
		instance = this;
		SubscribeUIMessage();
		musicVolume = PlayerPrefsWrapper.GetFloat("MusicVolume", 1f);
		fxVolume = PlayerPrefsWrapper.GetFloat("FXVolume", 1f);
		setupClipSets();
	}

	void OnValidate()
	{
		setupClipSets();
		try
		{
		foreach(ClipSet cs in allClipSets)
		{
			foreach(ProbableClip pclip in cs.clips)
			{
				if(pclip.clip)
					pclip.name = pclip.clip.name;
			}
		}
		} catch (Exception e) {print(e.InnerException);};
	}

	void setupClipSets()
	{
		allClipSets.Clear();
		allClipSets.Add(normalBaas);
		allClipSets.Add(busyBaas);
		allClipSets.Add(heroBaas);
		allClipSets.Add(blackBaas);
		allClipSets.Add(knifeSlashes);
		allClipSets.Add(normalDeaths);
		allClipSets.Add(fearBaas);
		allClipSets.Add(ticks);
		allClipSets.Add(victories);
		allClipSets.Add(smokeRelease);
		allClipSets.Add(spaceBaas);
		allClipSets.Add(crashes);
		allClipSets.Add(spaceDeaths);
		allClipSets.Add(explosiveDeaths);
		allClipSets.Add(busyDeaths);
		allClipSets.Add(heroDeaths);
		allClipSets.Add(normalFalls);
	}

	void Start()
	{
		for(int i = 0; i<musicSources.Count; i++)
			if(musicSources[i].gameObject.activeInHierarchy)
				activeMusicIndex = i;
		baseMusicVolume = musicSource.volume;
		musicSource.volume = 0f;
		musicSource.Play();
		StartCoroutine("fadeUpMusic");
	}

	IEnumerator fadeUpMusic()
	{
		float t = fadeUpMusicTime;
		float targetVolume = baseMusicVolume * musicVolume;
		while (t > 0f)
		{
			musicSource.volume += (targetVolume * Time.deltaTime / fadeUpMusicTime);
			t -= Time.deltaTime;
			yield return null;
		}
		musicSource.volume = targetVolume;
	}

	public static void SetFXVolume (float fxVolume)
	{
		instance.fxVolume = fxVolume;
		NormalBaa(1f);
	}
	public static void SetMusicVolume (float musicVolume)
	{
		instance.musicVolume = musicVolume;
		instance.musicSource.volume = instance.baseMusicVolume * musicVolume;
	}
	public static void NormalBaa(float volume)
	{
		instance.playRandom(volume*instance.normalBaas.volume, instance.normalBaas);
	}
	public static int BusyBaa(float volume)
	{
		return instance.playRandom(volume*instance.busyBaas.volume, instance.busyBaas);
	}
	public static void HeroBaa(float volume)
	{
		instance.playRandom(volume*instance.heroBaas.volume, instance.heroBaas);
	}
	public static void SpaceBaa(float volume)
	{
		instance.playRandom(volume*instance.spaceBaas.volume, instance.spaceBaas);
	}
	public static void FearBaa(float volume)
	{
		instance.playRandom(volume*instance.fearBaas.volume, instance.fearBaas);
	}
	public static void BlackBaa(float volume)
	{
		instance.playRandom(volume*instance.blackBaas.volume, instance.blackBaas);
	}
	public static void KnifeSlash(float volume)
	{
		instance.playRandom(volume*instance.knifeSlashes.volume, instance.knifeSlashes);
	}
	public static void Crash(float volume)
	{
		instance.playRandom(volume*instance.crashes.volume, instance.crashes);
	}
	public static void NormalDeath(float volume)
	{
		instance.playRandom(volume*instance.normalDeaths.volume, instance.normalDeaths);
	}

	public static void NormalFall (float volume)
	{
		instance.playRandom(volume*instance.normalFalls.volume, instance.normalFalls);
	}

	public static void ExplosiveDeath(float volume)
	{
		instance.playRandom(volume*instance.explosiveDeaths.volume, instance.explosiveDeaths);
	}
	public static void SpaceDeath(float volume)
	{
		instance.playRandom(volume*instance.spaceDeaths.volume, instance.spaceDeaths);
	}
	public static void BusyDeath(float volume)
	{
		instance.playRandom(volume*instance.busyDeaths.volume, instance.busyDeaths);
	}
	public static void HeroDeath(float volume)
	{
		instance.playRandom(volume*instance.heroDeaths.volume, instance.heroDeaths);
	}
	public static void Tick (float volume)
	{
		instance.playRandom(volume*instance.ticks.volume, instance.ticks);
	}

	public static void SmokeRelease (float volume)
	{
		instance.playRandom(volume*instance.smokeRelease.volume, instance.smokeRelease);
	}

	public static void Victory (float volume)
	{
		instance.playRandom(volume*instance.victories.volume, instance.victories);
	}

	public static void FadeOutMusic (float time)
	{
		Debug.Log("fade out music...");
		instance.StartCoroutine(instance.fadeOutSource(instance.musicSource, time));
	}

	protected override void OnUIMessage (object[] data)
	{
		string msg = (string)data[0];
		switch(msg)
		{
		case "LoadScreenStart":
			canPlayClips = false;
			break;
		case "LoadScreenEnd":
			canPlayClips = true;
			break;
		default:
			break;
		}
	}

	int playRandom(float volume, ClipSet clipSet)
	{
		if(!canPlayClips)
			return -1;
		if(clipSet.delayTime <= 0f)
		{
			int clipIndex = getRandomClipIndex(clipSet);
			List<ProbableClip> clipCollection = clipSet.clips;
			if(clipCollection.Count == 0)
			{
				print("Attempting to play from empty clip collection! Assign some effects numbnuts!");
				return -1;
			}
			sourcePool[sourcePoolIndex].volume = volume * fxVolume;
			sourcePool[sourcePoolIndex].PlayOneShot(clipCollection[clipIndex].clip);
			sourcePoolIndex = (sourcePoolIndex + 1)%sourcePool.Count;
			clipSet.delayTime = clipSet.minimumDelayPerPlay;
			return clipIndex;
		}
		return -1;
	}

	int getRandomClipIndex (ClipSet clipSet)
	{
		float totalProbability = clipSet.totalProbability;
		float randomNumber = UnityEngine.Random.Range(0f, totalProbability);
		float currProb = 0f;
		for(int i = 0; i<clipSet.clips.Count; i++)
		{
			if(randomNumber <= currProb + clipSet.clips[i].probability)
				return i;
			else
				currProb += clipSet.clips[i].probability;
		}
		return 0;
	}
	
	IEnumerator fadeOutSource(AudioSource src, float overTime)
	{
		float t = overTime;
		float origVolume = src.volume;
		while(t > 0f)
		{
			src.volume -= ((origVolume*Time.deltaTime) / overTime);
			t -= Time.deltaTime;
			yield return null;
		}
	}

	void OnDestroy()
	{
		instance = null;
	}
	void Update()
	{
		foreach(ClipSet clipSet in allClipSets)
			clipSet.delayTime -= Time.deltaTime;
	}
}
