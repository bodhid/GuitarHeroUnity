using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Session : MonoBehaviour
{
	public Song song;
	public Player playerPrefab;
	public Player[] players;
	public SessionRenderer sessionRenderer;
	public Smoothing smoothing;
	public NoteRenderer noteRenderer;
	public bool playing = false;
	public float speed; //meter per second
	public GameObject[] prefabs;
	public GameObject[] barPrefabs;
	public int frameIndex = 0;
	public int syncIndex = 0;
	public float boardLength = 10; //meters
	//public NoteInstance[] noteInstancePool;
	private AudioSource guitarSource, rhythmSource, songSource;
	public float time, previousTime;
	public double tick = 0;
	public double smoothTick = 0;
	public double starPowerDuration = 0;
	public double bpm, smoothBpm;
	public float RenderingFadeDistance = 3;
	public float RenderingFadeAmount = 1;

	public class PlayerInfo
	{
		public Song.Difficulty difficulty;
	}

	public void Initialize(Song _song, PlayerInfo[] _playerInfos)
	{
		Debug.Log("initializing ");
		song = _song;
		guitarSource = gameObject.AddComponent<AudioSource>();
		rhythmSource = gameObject.AddComponent<AudioSource>();
		songSource = gameObject.AddComponent<AudioSource>();
		guitarSource.playOnAwake = rhythmSource.playOnAwake = songSource.playOnAwake = false;
		guitarSource.clip = song.audio.guitar;
		rhythmSource.clip = song.audio.rhythm;
		songSource.clip = song.audio.song;
		Shader.SetGlobalFloat("_GH_Distance", RenderingFadeDistance);
		Shader.SetGlobalFloat("_GH_Fade", RenderingFadeAmount);
		smoothing = new Smoothing();
		List<RenderTexture> outputs = new List<RenderTexture>();

		players = new Player[_playerInfos.Length];
		for (int i = 0; i < _playerInfos.Length; ++i)
		{
			players[i] = Instantiate(playerPrefab.gameObject).GetComponent<Player>();
			players[i].transform.SetParent(transform);
			players[i].gameObject.SetActive(true);
		}

		for (int i = 0; i < players.Length; ++i)
		{
			Player.Pool pool = new Player.Pool();
			pool.barSize = 64;
			pool.noteInstanceSize = 1024;
			pool.noteSize = 256;
			Player.PoolIndex poolIndex = new Player.PoolIndex();

			pool.noteInstance = new Player.NoteInstance[pool.noteInstanceSize];
			for (int j = 0; j < pool.noteInstanceSize; ++j)
			{
				pool.noteInstance[j] = new Player.NoteInstance();
			}

			pool.note = new NoteModel[prefabs.Length][];
			for (int j = 0; j < prefabs.Length; ++j)
			{
				pool.note[j] = players[i].MakePool(pool.noteSize, prefabs[j]);
			}

			pool.bar = new BarInstance[pool.barSize];
			poolIndex.bar = poolIndex.note = poolIndex.noteInstance = 0;
			poolIndex.noteModel = new int[prefabs.Length];

			GameObject barPoolParent = new GameObject("BarPool");
			barPoolParent.transform.SetParent(players[i].transform);
			for (int j = 0; j < pool.barSize; ++j)
			{
				//Debug.Log(j + " - "+ pool.bar.Length);
				pool.bar[j] = Instantiate(barPrefabs[j % 2]).GetComponent<BarInstance>();
				pool.bar[j].transform.SetParent(barPoolParent.transform);
				pool.bar[j].gameObject.SetActive(false);
			}

			players[i].activeNotes = new List<Player.NoteInstance>();
			players[i].willRemove = new List<Player.NoteInstance>();
			players[i].activeBars = new List<BarInstance>();
			players[i].willRemoveBars = new List<BarInstance>();

			RenderTexture output = players[i].Initialize(i,song,_playerInfos[i].difficulty, new Vector2(1024,1024),pool, poolIndex, song.data.info.resolution, speed);
			outputs.Add(output);
		}
		sessionRenderer.Initialize(outputs.ToArray());
		System.GC.Collect();
		//GcControl.GC_disable();

	}

	public void EndSession()
	{
		song = null;
		smoothing = null;
		playing = false;
		foreach (Transform child in transform)
		{
			if (child.name.ToLower().Contains("pool"))
			{
				Destroy(child.gameObject);
			}
		}
		frameIndex = 0;
		//noteInstancePool = null;
		Destroy(guitarSource.clip);
		Destroy(rhythmSource.clip);
		Destroy(songSource.clip);
		Destroy(guitarSource);
		Destroy(rhythmSource);
		Destroy(songSource);
		guitarSource = rhythmSource = songSource = null;
		time = previousTime = 0;
		tick = 0;
		smoothTick = 0;
		starPowerDuration = 0;
		bpm = smoothBpm = 0;
		syncIndex = 0;
		for (int i = 0; i < players.Length; ++i)
		{
			players[i].Dispose();
		}
		System.GC.Collect();
	}

	public void StartPlaying()
	{
		for (int i = 0; i < players.Length; ++i)
		{
			players[i].cam.gameObject.SetActive(true);
		}
		playing = true;
	}

	
	void Update()
	{
		if (guitarSource != null && guitarSource.isPlaying)
		{
			//noteRenderer.NewFrame();
			frameIndex++;
			time = (guitarSource.time * 1000f);
			float millisecondsPassed = time - previousTime;
			rhythmSource.time = guitarSource.time;
			songSource.time = guitarSource.time;

			Sync(millisecondsPassed);
			smoothBpm = smoothing.SmoothBPM(bpm);
			smoothTick = smoothing.SmoothTick(tick, song.data.info.resolution);
			//Debug.Log(tick + " - " + smoothTick);
			for (int i = 0; i < players.Length; ++i)
			{
				players[i].SpawnObjects(tick, beatsPerSecond);
				players[i].UpdateObjects(smoothTick, noteRenderer, frameIndex);
				players[i].CreateBar(tick);
				players[i].UpdateActiveBars(smoothTick);
			}


			previousTime = time;
		}
		else
		{
			if (playing)
			{
				guitarSource.Play();
				rhythmSource.Play();
				songSource.Play();
			}
		}
	}

	private double beatsPerSecond, secondsPassed, beatsPassed, ticksPassed;

	private void Sync(float millisecondsPassed)
	{
		if (millisecondsPassed == 0)
		{
			//Debug.LogWarning("NO AUDIO PLAYED!");
		}

		//Debug.Log("milliseconds passed " + millisecondsPassed);
		beatsPerSecond = bpm / 60d;
		//Debug.Log("bps " + beatsPerSecond);
		secondsPassed = millisecondsPassed / 1000d;
		//Debug.Log("seconds passed " + secondsPassed);
		beatsPassed = beatsPerSecond * secondsPassed;
		//Debug.Log("beats passed " + beatsPassed);
		ticksPassed = beatsPassed * song.data.info.resolution;
		//Debug.Log("Ticks passed " + ticksPassed);
		if (!double.IsNaN(ticksPassed) && bpm > 0) tick += ticksPassed;


		//Debug.Log("TICK: " + tick);
		if (syncIndex < song.data.syncTrack.Count) //check if on final sync
		{
			Song.SyncTrack nextSync = song.data.syncTrack[syncIndex];
			if (nextSync.timestamp <= tick)
			{
				//Debug.Log("Executing command " + nextSync.command + " " + nextSync.value);
				switch (nextSync.command)
				{
					case "B":
						bpm = nextSync.value * 0.001d;
						//Debug.Log("Settings BPM to " + bpm);
						break;
					case "TS":
						//???????
						break;
				}
				syncIndex++;
				//Sync(0);
			}
		}
	}

	public float TickDistanceToMeters(float tickDistance)
	{
		if (song == null) return 0;
		return (tickDistance / song.data.info.resolution) * speed;
	}
	public float MetersToTickDistance(float meters)
	{
		if (song == null) return 0;
		return (meters / speed * song.data.info.resolution);
	}
}
