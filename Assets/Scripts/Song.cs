using System.Collections.Generic;
using UnityEngine;
using System.IO;
[System.Serializable]
public class Song
{
	//Data
	public FileInfo fileInfo;
	public bool ready;
	public Data data;
	public Audio audio;

	public enum Difficulty
	{
		Easy,
		Medium,
		Hard,
		Expert
	}

	[System.Serializable]
	public class Data
	{
		public Notes notes;
		public Info info;
		public List<SyncTrack> syncTrack;
		public List<SongEvent> events;
	}

	[System.Serializable]
	public class Info
	{
		public uint resolution;
	}

	[System.Serializable]
	public class Notes
	{
		public List<Note> easy, medium, hard, expert;
	}

	[System.Serializable]
	public class Note
	{
		public Note(uint _timestamp,  uint _fred, uint _duration, bool _star, bool _hammerOn)
		{
			timestamp = _timestamp;
			duration = _duration;
			fred = _fred;
			star = _star;
			hammerOn = _hammerOn;
		}
		public uint timestamp, duration, fred;
		public bool star, hammerOn;
	}

	[System.Serializable]
	public class SongEvent
	{
		public SongEvent(uint _timestamp, string _name)
		{
			timestamp = _timestamp;
			name = _name;
		}
		public uint timestamp;
		public string name;
	}

	[System.Serializable]
	public class SyncTrack
	{
		public SyncTrack(uint _timestamp,string _command, uint _value)
		{
			timestamp = _timestamp;
			command = _command;
			value = _value;
		}
		public uint timestamp, value;
		public string command;
	}

	[System.Serializable]
	public class Audio
	{
		public AudioClip song, guitar, rhythm, preview;
	}
}