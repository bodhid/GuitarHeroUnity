using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;
using UnityEngine.Networking;

public class SongLoader : MonoBehaviour
{
	private static SongLoader instance;
	public static SongLoader Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new GameObject("SongLoader").AddComponent<SongLoader>();
			}
			return instance;
		}
	}
	public delegate void OnLoaded(Song song);
	public delegate void OnPrepared();

	//threading
	private Song song;
	private string error;
	object lockObject = new object();

	public void Load(string chartFile, OnLoaded onLoaded)
	{
		StartCoroutine(LoadCoroutine(chartFile, onLoaded));
	}

	public void PrepareAudio(Song song, OnPrepared onPrepared)
	{
		StartCoroutine(PrepareCoroutine(song, onPrepared));
	}

	private IEnumerator LoadCoroutine(string chartFile, OnLoaded onLoaded)
	{
		yield return null;
		song = new Song();
		song.ready = false;
		song.fileInfo = new FileInfo(chartFile);
		if (!song.fileInfo.Exists) throw new System.Exception(".chart file not found: "+chartFile);
		Thread thread = new Thread(Parse);
		thread.IsBackground = true;
		yield return null;
		thread.Start();
		//Parse();
		while (true)
		{
			lock (lockObject)
			{
				if (song.ready) break;
			}
			yield return null;
		}
		if (error != null) throw new System.Exception(error);
		onLoaded(song);
	}

	private IEnumerator PrepareCoroutine(Song song, OnPrepared onPrepared)
	{
		Debug.Log("Loading guitar");
		yield return null;
		Song.Audio audio = new Song.Audio();
		FileInfo guitarFileInfo = new FileInfo(song.fileInfo.Directory.FullName + "/guitar.ogg");
		if (guitarFileInfo.Exists)
		{
			using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(guitarFileInfo.FullName, AudioType.OGGVORBIS))
			{
				yield return uwr.SendWebRequest();
				if (uwr.isNetworkError || uwr.isHttpError)
				{
					Debug.LogError(uwr.error);
					yield break;
				}
				yield return null;
				audio.guitar = DownloadHandlerAudioClip.GetContent(uwr);
			}
		}
		Debug.Log("Loading song");
		yield return null;
		FileInfo songFileInfo = new FileInfo(song.fileInfo.Directory.FullName + "/song.ogg");
		if (songFileInfo.Exists)
		{
			using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(songFileInfo.FullName, AudioType.OGGVORBIS))
			{
				yield return uwr.SendWebRequest();
				if (uwr.isNetworkError || uwr.isHttpError)
				{
					Debug.LogError(uwr.error);
					yield break;
				}
				yield return null;
				audio.song = DownloadHandlerAudioClip.GetContent(uwr);
			}
		}
		Debug.Log("Loading rhythm");
		yield return null;
		FileInfo rhythmFileInfo = new FileInfo(song.fileInfo.Directory.FullName + "/rhythm.ogg");
		if (rhythmFileInfo.Exists)
		{
			using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(rhythmFileInfo.FullName, AudioType.OGGVORBIS))
			{
				yield return uwr.SendWebRequest();
				if (uwr.isNetworkError || uwr.isHttpError)
				{
					Debug.LogError(uwr.error);
					yield break;
				}
				yield return null;
				audio.rhythm = DownloadHandlerAudioClip.GetContent(uwr);
			}
		}
		song.audio = audio;
		Debug.Log("Audio loaded");
		onPrepared();
	}

	private void Parse()
	{
		string gotError = null;
		try
		{
			string fullFileName;
			lock (lockObject)
			{
				fullFileName = song.fileInfo.FullName;
			}
			string[] chart = File.ReadAllLines(fullFileName);
			Song.Notes notes = new Song.Notes();
			notes.easy = new List<Song.Note>();
			notes.medium = new List<Song.Note>();
			notes.hard = new List<Song.Note>();
			notes.expert = new List<Song.Note>();
			List<Song.SyncTrack> syncTrack = new List<Song.SyncTrack>();
			List<Song.SongEvent> events = new List<Song.SongEvent>();
			Song.Info info = new Song.Info();
			//Debug.Log(chart.Length);
			for (int i = 0; i < chart.Length; ++i)
			{
				if (chart[i].Contains("[Song]")) { i = LoadChartSong(info, chart, i); continue; }
				if (chart[i].Contains("[SyncTrack]")) { i = LoadChartSyncTrack(syncTrack, chart, i); continue; }
				if (chart[i].Contains("[Events]")) { i = LoadChartEvents(events, chart, i); continue; }
				if (chart[i].Contains("[ExpertSingle]")) { i = LoadChartNotes(chart, i, notes.expert,info.resolution); continue; }
				if (chart[i].Contains("[HardSingle]")) { i = LoadChartNotes(chart, i, notes.hard, info.resolution); continue; }
				if (chart[i].Contains("[MediumSingle]")) { i = LoadChartNotes(chart, i, notes.medium, info.resolution); continue; }
				if (chart[i].Contains("[EasySingle]")) { i = LoadChartNotes(chart, i, notes.easy, info.resolution); continue; }
			}
			Song.Data data = new Song.Data();
			data.syncTrack = syncTrack;
			data.info = info;
			data.events = events;
			data.notes = notes;
			song.data = data;
		}
		catch (System.Exception e)
		{
			gotError = e.Message + " - " + e.StackTrace;
		}
		lock (lockObject)
		{
			error = gotError;
			song.ready = true;
		}
	}

	private int LoadChartSong(Song.Info info, string[] chart, int i)
	{
		int timeout = 100000;
		while (i < timeout)
		{
			if (chart[i].Contains("{"))
			{
				//Debug.Log("Start reading song info");
				i++;
				break;
			}
			i++;
		}
		while (i < timeout)
		{
			if (chart[i].Contains("}"))
			{
				//Debug.Log("End reading song info");
				break;
			}
			if (chart[i].Contains("Resolution"))
			{
				info.resolution = uint.Parse(chart[i].Split(new string[] { " = " }, System.StringSplitOptions.None)[1]);
			}
			i++;
		}
		return i;
	}
	private int LoadChartSyncTrack(List<Song.SyncTrack> syncTrack, string[] chart, int i)
	{
		int timeout = 100000;
		while (i < timeout)
		{
			if (chart[i].Contains("{"))
			{
				//Debug.Log("Start reading SyncTrack");
				i++;
				break;
			}
			i++;
		}
		
		while (i < timeout)
		{
			if (chart[i].Contains("}")) break;
			string line = chart[i];
			if (line.Contains(" = "))
			{
				string[] splitted = line.Split(new string[] { " = " }, System.StringSplitOptions.None);
				string[] commandValue = splitted[1].Split(" "[0]);
				syncTrack.Add(new Song.SyncTrack(uint.Parse(splitted[0]),commandValue[0], uint.Parse(commandValue[1])));
			}
			i++;
		}
		return i;
	}

	private int LoadChartEvents(List<Song.SongEvent> events, string[] chart, int i)
	{
		int timeout = 100000;
		while (i < timeout)
		{
			if (chart[i].Contains("{"))
			{
				//Debug.Log("Start reading Events");
				i++;
				break;
			}
			i++;
		}
		
		while (i < timeout)
		{
			if (chart[i].Contains("}"))
			{
				//Debug.Log("End reading Events");
				break;
			}
			string line = chart[i];
			if (line.Contains(" = E "))
			{
				string[] splitted = line.Split(new string[] { " = E " }, System.StringSplitOptions.None);
				events.Add(new Song.SongEvent(uint.Parse(splitted[0]), splitted[1]));
			}
			i++;
		}
		return i;
	}
	private int LoadChartNotes(string[] chart, int i, List<Song.Note> list, uint resolution)
	{
		int timeout = 100000;
		while (i < timeout)
		{
			if (chart[i].Contains("{"))
			{
				//Debug.Log("Start reading Notes");
				i++;
				break;
			}
			i++;
		}
		uint starPowerEndsAt=0;
		while (i < timeout)
		{
			if (chart[i].Contains("}"))
			{
				//Debug.Log("End reading Notes");
				break;
			}
			string line = chart[i];
			if (line.Contains(" = "))
			{
				string[] splitted = line.Split(new string[] { " = " }, System.StringSplitOptions.None);
				string[] noteSplitted = splitted[1].Split(" "[0]);
				uint timestamp = uint.Parse(splitted[0]);
				if (noteSplitted[0] == "N")
				{
					bool hammeron = false;
					uint fred = uint.Parse(noteSplitted[1]);
					Song.Note previousNote=null;
					if (list.Count > 0)
					{
						previousNote = list[list.Count - 1];
						if (previousNote.timestamp == timestamp)//double notes no hammeron
						{
							previousNote.hammerOn = false;
						}
						else
						{
							hammeron = (timestamp < previousNote.timestamp + (resolution / 2))&&(previousNote.fred!=fred) && (previousNote.timestamp != timestamp);
						}
					}
					if (uint.Parse(noteSplitted[1]) < 5)
					{
						list.Add(new Song.Note(timestamp,fred , uint.Parse(noteSplitted[2]),timestamp<=starPowerEndsAt,hammeron));
					}
				}
				if (noteSplitted[0] == "S")
				{

					starPowerEndsAt = timestamp + uint.Parse(noteSplitted[2]);
					//also set previous note to star
					int traceBack = 1;
					while (traceBack < 5)
					{
						if (list[list.Count - traceBack].timestamp == timestamp)
						{
							list[list.Count - traceBack].star = true;
							traceBack++;
							continue;
						}
						break;
					}
				}
			}
			i++;
		}
		return i;
	}
}