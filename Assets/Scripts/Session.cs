using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Session : MonoBehaviour
{
	public Song song;
	public Transform camera3DTransform;
	public Smoothing smoothing;
	public NoteRenderer noteRenderer;
	public bool playing = false;
	public float speed; //meter per second
	public GameObject[] prefabs;
	public GameObject[] barPrefabs;
	public Animation2D[] flame;
	public NoteModel[][] pool;
	public BarInstance[] barPool;
	public int[] poolIndex;
	public int frameIndex = 0;
	public int noteIndex = 0;
	public int syncIndex = 0;
	public int barPoolIndex = 0;
	public int noteinstancePoolIndex = 0;
	public int poolSize = 256;
	public int noteInstancePoolSize = 1024;
	public int barPoolSize = 64;
	public int noteResolution;
	public uint nextBar = 0;
	public float boardLength = 10; //meters
	public NoteInstance[] noteInstancePool;
	public List<NoteInstance> activeNotes;
	public List<NoteInstance> willRemove;
	public List<BarInstance> activeBars;
	public List<BarInstance> willRemoveBars;
	private List<Song.Note> allNotes;
	private AudioSource guitarSource, rhythmSource, songSource;
	public float time, previousTime;
	public double tick = 0;
	public double smoothTick = 0;
	public double starPowerDuration = 0;
	public double bpm, smoothBpm;
	public float RenderingFadeDistance = 3;
	public float RenderingFadeAmount = 1;

	[System.Serializable]
	public class NoteInstance
	{
		public void Update(NoteModel _noteModel, uint _timestamp, uint _fred, uint _duration, bool _star, bool _hammeron)
		{
			noteModel = _noteModel;
			timestamp = _timestamp;
			fred = _fred;
			duration = _duration;
			star = _star;
			hammeron = _hammeron;
		}
		public NoteModel noteModel;
		public uint timestamp;
		public bool seen, star, hammeron;
		public uint fred;
		public uint duration;
	}

	public enum Difficulty
	{
		Easy,
		Medium,
		Hard,
		Expert
	}

	public void Initialize(Song _song, Difficulty difficulty = Difficulty.Expert)
	{
		Debug.Log("initializing ");
		
		song = _song;
		
		//noteRenderer.Initialize();
		activeNotes = new List<NoteInstance>();
		willRemove = new List<NoteInstance>();
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
		switch (difficulty)
		{
			case Difficulty.Easy:
				allNotes = song.data.notes.easy;
				break;
			case Difficulty.Medium:
				allNotes = song.data.notes.medium;
				break;
			case Difficulty.Hard:
				allNotes = song.data.notes.hard;
				break;
			case Difficulty.Expert:
				allNotes = song.data.notes.expert;
				break;
		}

		noteInstancePool = new NoteInstance[noteInstancePoolSize];
		for (int i = 0; i < noteInstancePoolSize; ++i)
		{
			noteInstancePool[i] = new NoteInstance();
		}
		if (pool == null)
		{
			pool = new NoteModel[prefabs.Length][];
			for (int i = 0; i < prefabs.Length; ++i)
			{
				pool[i] = MakePool(poolSize, prefabs[i]);
			}
			poolIndex = new int[prefabs.Length];
		}
		barPool = new BarInstance[barPoolSize];
		GameObject barPoolParent = new GameObject("BarPool");
		barPoolParent.transform.SetParent(transform);
		activeBars = new List<BarInstance>();
		willRemoveBars = new List<BarInstance>();
		nextBar = song.data.info.resolution;
		for (int i = 0; i < barPoolSize; ++i)
		{
			barPool[i] = Instantiate(barPrefabs[i % 2]).GetComponent<BarInstance>();
			barPool[i].transform.SetParent(barPoolParent.transform);
			barPool[i].gameObject.SetActive(false);
		}
		System.GC.Collect();
		//GcControl.GC_disable();

	}

	public void EndSession()
	{
		song = null;
		smoothing = null;
		playing = false;
		for (int i = 0; i < pool.Length; ++i)
		{
			for (int j = 0; j < pool[i].Length; ++j)
			{
				Destroy(pool[i][j].gameObject);
			}
		}
		foreach (Transform child in transform)
		{
			if (child.name.ToLower().Contains("pool"))
			{
				Destroy(child.gameObject);
			}
			}
		pool = null;
		barPool = null;
		poolIndex = null;
		frameIndex = 0;
		noteIndex = 0;
		syncIndex = 0;
		barPoolIndex = 0;
		noteinstancePoolIndex = 0;
		noteInstancePool = null;
		activeNotes = null;
		willRemove = null;
		allNotes = null;
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
		camera3DTransform.gameObject.SetActive(false);
		System.GC.Collect();
	}

	public void StartPlaying()
	{
		camera3DTransform.gameObject.SetActive(true);
		playing = true;
	}

	private NoteModel[] MakePool(int size, GameObject prefab)
	{
		NoteModel[] newPool = new NoteModel[size];
		GameObject poolObject = new GameObject("Pool " + prefab.name);
		poolObject.transform.SetParent(transform);
		for (int i = 0; i < newPool.Length; ++i)
		{

			GameObject g = Instantiate(prefab);
			g.SetActive(false);
			g.transform.SetParent(poolObject.transform);
			newPool[i] = g.GetComponent<NoteModel>();
			if (newPool[i].line != null)
			{
				newPool[i].materialInstance = newPool[i].line.material;
			}
		}
		return newPool;
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
			//Debug.Log("going ");
			smoothBpm = smoothing.SmoothBPM(bpm);
			smoothTick = smoothing.SmoothTick(tick, song.data.info.resolution);
			CreateNote();
			UpdateActiveNotes();
			CreateBar();
			UpdateActiveBars();
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

	private void CreateNote()
	{
		if (noteIndex >= allNotes.Count) return; //end of song
		Song.Note nextNote = allNotes[noteIndex];

		//Debug.Log(nextNote.timestamp);
		double tenSeconds = Time.deltaTime * 10;
		double tenSecondsInBeats = beatsPerSecond * 3;
		double tenSecondsInTicks = tenSecondsInBeats * song.data.info.resolution;

		if (nextNote.timestamp < tick + MetersToTickDistance(4f)) //spawn tick + 10 seconds?
		{
			//Debug.Log("New Note");
			try
			{
				bool longNote = (nextNote.duration > 0);
				int poolNumber = (int)nextNote.fred + (longNote ? 5 : 0);
				NoteModel noteModel = pool[poolNumber][poolIndex[poolNumber] % poolSize];
				GameObject newNote = noteModel.gameObject;
				noteModel.myTransform.rotation = camera3DTransform.rotation;
				newNote.SetActive(true);
				NoteInstance noteInstance = noteInstancePool[noteinstancePoolIndex % noteInstancePoolSize];
				noteinstancePoolIndex++;
				noteInstance.Update(noteModel, nextNote.timestamp, nextNote.fred, nextNote.duration, nextNote.star, nextNote.hammerOn);
				noteInstance.seen = false;
				activeNotes.Add(noteInstance);

				

				noteIndex++;
				poolIndex[poolNumber]++;
				CreateNote();
			}
			catch (System.Exception e)
			{
				Debug.LogError(e.Message + " - " + e.StackTrace);
				Debug.LogError(nextNote.fred);
				Debug.LogError(poolIndex[nextNote.fred] % poolSize);
			}
		}
	}
	private void UpdateActiveNotes()
	{
		for (int i = 0; i < activeNotes.Count; ++i)
		{
			NoteInstance noteInstance = activeNotes[i];
			Transform noteTransform = noteInstance.noteModel.transform;
			Vector3 pos = noteTransform.localPosition;

			float tickDistance = noteInstance.timestamp - (float)smoothTick;
			float distanceInMeters = TickDistanceToMeters(tickDistance);
			pos.z = distanceInMeters;
			noteTransform.position = pos;
			float noteDistance = tickDistance;
			float noteDistanceInMeters = TickDistanceToMeters(noteDistance);
			float endOfNoteDistance = tickDistance + noteInstance.duration;
			float endOfNoteInMeters = TickDistanceToMeters(endOfNoteDistance);
			if (noteInstance.duration > 0)
			{
				//update long note length
				float length= endOfNoteInMeters - distanceInMeters;
				noteInstance.noteModel.SetLengt(length);
			}

			//show correct sprite
			SpriteRenderer spriteRenderer = noteInstance.noteModel.spriteRenderer;
			NoteRenderer.FredSpriteData fredSpriteData = noteRenderer.spriteData.fred[noteInstance.fred];
			if (noteInstance.star)
			{
				spriteRenderer.sprite = (noteInstance.hammeron) ? fredSpriteData.starHammerOn[frameIndex % 16] : fredSpriteData.star[frameIndex % 16];
			}
			else
			{
				spriteRenderer.sprite = (noteInstance.hammeron) ? fredSpriteData.hammerOn : fredSpriteData.normal;
			}

			if (noteDistance < 0)
			{
				flame[noteInstance.fred].gameObject.SetActive(true);
				flame[noteInstance.fred].Reset();
				flame[noteInstance.fred].seconds = (1f / 60f * 8f);
			}

			if (endOfNoteInMeters < 0) //out of view
			{
				willRemove.Add(noteInstance);
			}
			//noteRenderer.RenderNote(noteInstance, distanceInMeters);
		}

		for (int i = willRemove.Count - 1; i > -1; --i)
		{
			activeNotes.Remove(willRemove[i]);
			willRemove[i].noteModel.transform.gameObject.SetActive(false);
			
			//noteRenderer.RemoveMap(willRemove[i]);
			willRemove.RemoveAt(i);
		}
	}

	private void CreateBar()
	{
		if (nextBar < tick + MetersToTickDistance(4f)) //spawn tick + 10 seconds?
		{
			
			BarInstance newBar = barPool[barPoolIndex%barPoolSize];
			barPoolIndex++;
			newBar.gameObject.SetActive(true);
			newBar.timestamp = nextBar;
			activeBars.Add(newBar);
			nextBar += song.data.info.resolution;
		}
	}

	private void UpdateActiveBars()
	{
		for (int i = 0; i < activeBars.Count; ++i)
		{
			BarInstance barInstance = activeBars[i];
			float tickDistance = barInstance.timestamp - (float)smoothTick;
			float distanceInMeters = TickDistanceToMeters(tickDistance);
			Vector3 pos = barInstance.myTransform.localPosition;
			pos.z = distanceInMeters;
			barInstance.myTransform.position = pos;
			if (tickDistance < 0)
			{
			//	Debug.Log("Will Remove " + barInstance.gameObject);
				willRemoveBars.Add(barInstance);
			}
		}
		for (int i = willRemoveBars.Count - 1; i > -1; --i)
		{
			activeBars.Remove(willRemoveBars[i]);
			willRemoveBars[i].gameObject.SetActive(false);
			willRemoveBars.RemoveAt(i);
		}
	}

	public float TickDistanceToMeters(float tickDistance)
	{
		if (song == null) return 0;
		return (tickDistance / song.data.info.resolution)*speed;
	}
	public float MetersToTickDistance(float meters)
	{
		if (song == null) return 0;
		return (meters/speed * song.data.info.resolution);
	}
}
