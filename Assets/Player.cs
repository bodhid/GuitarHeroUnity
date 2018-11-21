using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	public RenderTexture output;
	public List<Song.Note> notes;
	public Transform cam;
	public Board board;
	public PoolIndex index;
	public Pool pool;
	public List<NoteInstance> activeNotes;
	public List<NoteInstance> willRemove;
	public List<BarInstance> activeBars;
	public List<BarInstance> willRemoveBars;
	public Animation2D[] flame;
	public uint resolution;
	public float speed;

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

	public void Initialize(List<Song.Note> _notes, Pool _pool, PoolIndex _poolIndex, uint _resolution, float _speed)
	{
		notes = _notes;
		pool = _pool;
		index = new PoolIndex();
		resolution = _resolution;
		speed = _speed;
		index = _poolIndex;
		activeNotes = new List<NoteInstance>();
		activeBars = new List<BarInstance>();
		willRemove = new List<NoteInstance>();
		willRemoveBars = new List<BarInstance>();
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
	private void UpdateObjects(double smoothTick, NoteRenderer noteRenderer, int frameIndex)
	{
		for (int i = 0; i < activeNotes.Count; ++i)
		{
			NoteInstance noteInstance = activeNotes[i];
			Transform noteTransform = noteInstance.noteModel.transform;
			Vector3 pos = noteTransform.localPosition;

			double tickDistance = noteInstance.timestamp - smoothTick;
			double distanceInMeters = TickDistanceToMeters(tickDistance);
			pos.z = (float)distanceInMeters;
			noteTransform.position = pos;
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

	public double TickDistanceToMeters(double tickDistance)
	{
		return (tickDistance / resolution) * speed;
	}

	private double MetersToTickDistance(double meters)
	{
		return (meters / speed * resolution);
	}
}
