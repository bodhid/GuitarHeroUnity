using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	public int playerNumber;
	public PlayerInput playerInput;
	public int layerMask;
	public Song.Difficulty difficulty;
	public RenderTexture output;
	public Song song;
	public List<Song.Note> notes;
	public Transform cam;
	public Transform board;
	public PoolIndex index;
	public Pool pool;
	public List<NoteInstance> activeNotes;
	public List<NoteInstance> willRemove;
	public List<BarInstance> activeBars;
	public List<BarInstance> willRemoveBars;
	public Animation2D[] flame;
	public GameObject[] fredHighlight;
	public uint resolution;
	public float speed;
	public uint nextBar;

	[System.Serializable]
	public class Pool
	{
		public int noteSize;
		public int barSize;
		public int noteInstanceSize;
		public NoteModel[][] note;
		public NoteInstance[] noteInstance;
		public BarInstance[] bar;
	}

	[System.Serializable]
	public class PoolIndex
	{
		public int note;
		public int[] noteModel;
		public int bar;
		public int noteInstance;
	}

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

	public RenderTexture Initialize(int _playerNumber,Song _song, Song.Difficulty _difficulty,Vector2 _output, Pool _pool, PoolIndex _poolIndex, uint _resolution, float _speed)
	{
		playerNumber = _playerNumber;
		layerMask= 1 << (10 + playerNumber);
		song = _song;
		switch (_difficulty)
		{
			case Song.Difficulty.Easy:
				notes = song.data.notes.easy;
				break;
			case Song.Difficulty.Medium:
				notes = song.data.notes.medium;
				break;
			case Song.Difficulty.Hard:
				notes = song.data.notes.hard;
				break;
			case Song.Difficulty.Expert:
				notes = song.data.notes.expert;
				break;
		}
		pool = _pool;
		index = new PoolIndex();
		resolution = _resolution;
		nextBar = resolution;
		speed = _speed;
		index = _poolIndex;
		activeNotes = new List<NoteInstance>();
		activeBars = new List<BarInstance>();
		willRemove = new List<NoteInstance>();
		willRemoveBars = new List<BarInstance>();

		output = new RenderTexture(Mathf.CeilToInt(_output.x), Mathf.CeilToInt(_output.y), 16, RenderTextureFormat.ARGB32);
		cam.GetComponent<Camera>().targetTexture = output;
		cam.GetComponent<Camera>().cullingMask = layerMask;
		SetLayerRecursive(transform,10+ playerNumber);

		playerInput = new PlayerInput(PlayerInput.Device.Xinput, playerNumber);

		return output;
	}

	public void GetInput()
	{
		playerInput.Update();
	}

	public void SetLayerRecursive(Transform t, int layerMask)
	{
		foreach (Transform child in t)
		{
			//Debug.Log(child.name);
			child.gameObject.layer = layerMask;
			SetLayerRecursive(child, layerMask);
		}
		}

	public NoteModel[] MakePool(int size, GameObject prefab)
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

	public void SpawnObjects(double tick, double beatsPerSecond)
	{
		if (index.note >= notes.Count) return; //end of song
		Song.Note nextNote = notes[index.note];

		//double tenSeconds = Time.deltaTime * 10;
		//double tenSecondsInBeats =;
		double tenSecondsInTicks = beatsPerSecond * 3 * resolution;

		if (nextNote.timestamp < tick + MetersToTickDistance(4f)) //spawn tick + 10 seconds?
		{
			//Debug.Log("New Note");
			try
			{
				bool longNote = (nextNote.duration > 0);
				int poolNumber = (int)nextNote.fred + (longNote ? 5 : 0);
				NoteModel noteModel = pool.note[poolNumber][index.noteModel[poolNumber] % pool.noteSize];
				GameObject newNote = noteModel.gameObject;
				noteModel.myTransform.rotation = cam.rotation;
				newNote.SetActive(true);
				NoteInstance noteInstance = pool.noteInstance[index.noteInstance % pool.noteInstanceSize];
				index.noteInstance++;
				noteInstance.Update(noteModel, nextNote.timestamp, nextNote.fred, nextNote.duration, nextNote.star, nextNote.hammerOn);
				noteInstance.seen = false;
				activeNotes.Add(noteInstance);

				index.note++;
				index.noteModel[poolNumber]++;
				SpawnObjects(tick, beatsPerSecond);
			}
			catch (System.Exception e)
			{
				Debug.LogError(e.Message + " - " + e.StackTrace);
			}
		}
	}

	public void Dispose()
	{
		song = null;
		foreach (Transform child in transform)
		{
			if (child.name.ToLower().Contains("pool"))
			{
				Destroy(child.gameObject);
			}
		}
		cam.gameObject.SetActive(false);
		pool = null;
		index = null;
		Destroy(gameObject);
	}

	public void UpdateObjects(double smoothTick, NoteRenderer noteRenderer, int frameIndex)
	{
		Vector3 boardPosition = board.localPosition;
		boardPosition.z = (float)((TickDistanceToMeters(smoothTick) % 2) * -1f + 4);
		if (!float.IsNaN(boardPosition.z))
		{
			board.localPosition = boardPosition;
		}
		for (int i = 0; i < activeNotes.Count; ++i)
		{
			NoteInstance noteInstance = activeNotes[i];
			Transform noteTransform = noteInstance.noteModel.transform;
			Vector3 pos = noteTransform.localPosition;

			double tickDistance = noteInstance.timestamp - smoothTick;
			double distanceInMeters = TickDistanceToMeters(tickDistance);
			pos.z = (float)distanceInMeters;
			noteTransform.localPosition = pos;
			double noteDistance = tickDistance;
			double noteDistanceInMeters = TickDistanceToMeters(noteDistance);
			double endOfNoteDistance = tickDistance + noteInstance.duration;
			double endOfNoteInMeters = TickDistanceToMeters(endOfNoteDistance);
			if (noteInstance.duration > 0)
			{
				//update long note length
				float length = (float)(endOfNoteInMeters - distanceInMeters);
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
			if (endOfNoteInMeters < -1) //out of view
			{
				willRemove.Add(noteInstance);
			}
		}
	}

	

	public void CreateBar(double tick)
	{
		if (nextBar < tick + MetersToTickDistance(4f)) //spawn tick + 10 seconds?
		{

			BarInstance newBar = pool.bar[index.bar % pool.barSize];
			index.bar++;
			newBar.gameObject.SetActive(true);
			newBar.timestamp = nextBar;
			activeBars.Add(newBar);
			nextBar += resolution;
		}
	}

	public void UpdateActiveBars(double smoothTick)
	{
		for (int i = 0; i < activeBars.Count; ++i)
		{
			BarInstance barInstance = activeBars[i];
			double tickDistance = barInstance.timestamp - smoothTick;
			double distanceInMeters = TickDistanceToMeters(tickDistance);
			Vector3 pos = barInstance.myTransform.localPosition;
			pos.z = (float)distanceInMeters;
			barInstance.myTransform.localPosition = pos;
			if (tickDistance < 0)
			{
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

	public void RegisterHits(double tick)
	{
		for (int i = 0; i < playerInput.fred.Length; ++i)
		{
			fredHighlight[i].SetActive(playerInput.fred[i]);
		}
		for (int i = 0; i < activeNotes.Count; ++i)
		{
			NoteInstance noteInstance = activeNotes[i];
			if (playerInput.strumPressed)
			{
				if (playerInput.fred[noteInstance.fred])
				{
					float distance = Mathf.Abs((float)(tick - noteInstance.timestamp));
					if (distance < resolution/4)
					{
						flame[noteInstance.fred].gameObject.SetActive(true);
						flame[noteInstance.fred].Reset();
						flame[noteInstance.fred].seconds = (1f / 60f * 8f);
						willRemove.Add(noteInstance);
					}
				}
			}
		}
	}

	public void DiscardNotes()
	{
		for (int i = willRemove.Count - 1; i > -1; --i)
		{
			activeNotes.Remove(willRemove[i]);
			willRemove[i].noteModel.transform.gameObject.SetActive(false);
			willRemove.RemoveAt(i);
		}
	}

	public double TickDistanceToMeters(double tickDistance)
	{
		return (tickDistance / resolution) * speed;
	}

	private double MetersToTickDistance(double meters)
	{
		return (meters / speed * resolution);
	}
}
