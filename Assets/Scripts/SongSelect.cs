using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
public class SongSelect : MonoBehaviour
{
	public string path;
	public Session session;
	public List<SongInfo> songs;
	public SongBlock songblockPrefab;
	public GameObject selectScreen;
	public RawImage fade;
	[System.Serializable]
	public class SongInfo
	{
		public FileInfo fileInfo;
		public string artist, name, displayArtist, displayName;
	}

	public void Start()
	{
		selectScreen.SetActive(true);
		songs = ScanForSongsRecursively(new DirectoryInfo(Application.dataPath).Parent);
		for (int i = 0; i < songs.Count; ++i)
		{
			SongBlock newBlock = Instantiate(songblockPrefab.gameObject).GetComponent<SongBlock>();
			newBlock.transform.SetParent(songblockPrefab.transform.parent);
			newBlock.transform.localPosition = Vector3.zero;
			newBlock.transform.localScale = Vector3.one;
			newBlock.text.text = songs[i].displayArtist;
			newBlock.fileInfo = songs[i].fileInfo;
		}
		if (songs.Count == 0)
		{
			songblockPrefab.text.text = "No Songs found";
			songblockPrefab.GetComponent<Button>().enabled = false;
		}
		else
		{
			songblockPrefab.gameObject.SetActive(false);
		}
	}

	public List<SongInfo> ScanForSongsRecursively(DirectoryInfo folder)
	{
		List<SongInfo> list = new List<SongInfo>();
		List<DirectoryInfo> foldersToScan = new List<DirectoryInfo>();
		foldersToScan.Add(folder);
		while (foldersToScan.Count > 0)
		{
			DirectoryInfo[] currentScan = foldersToScan.ToArray();
			foldersToScan.Clear();
			for (int i = 0; i < currentScan.Length; ++i)
			{
				//Debug.Log("Scanning " + currentScan[i].FullName);
				foreach (FileInfo f in currentScan[i].GetFiles())
				{
					if (f.Name == "song.ini")
					{
						list.Add(CreateSongInfo(currentScan[i]));
						break;
					}
				}
				foreach (DirectoryInfo d in currentScan[i].GetDirectories())
				{
					foldersToScan.Add(d);
				}
			}
		}
		return Sort(list);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (session.playing)
			{
				session.playing = false;
				StartCoroutine(EndingSong());
			
			}
		}
	}

	public List<SongInfo> Sort(List<SongInfo> songs)
	{
		Dictionary<string, SongInfo> songByArtists = new Dictionary<string, SongInfo>();
		List<string> artists = new List<string>();
		for (int i = 0; i < songs.Count; ++i)
		{
			if (!songByArtists.ContainsKey(songs[i].displayArtist))
			{
				artists.Add(songs[i].displayArtist);
				songByArtists.Add(songs[i].displayArtist, songs[i]);
			}
		}
		artists.Sort();
		List<SongInfo> sortedList = new List<SongInfo>();
		for (int i = 0; i < artists.Count; ++i)
		{
			sortedList.Add(songByArtists[artists[i]]);
		}
		return sortedList;
	}

	public SongInfo CreateSongInfo(DirectoryInfo folder)
	{
		SongInfo songInfo = new SongInfo();
		foreach (FileInfo f in folder.GetFiles())
		{
			if (f.Name == "notes.chart")
			{
				songInfo.fileInfo = f;
				break;
			}
		}
		FileInfo ini = null;
		foreach (FileInfo f in folder.GetFiles())
		{
			if (f.Name == "song.ini")
			{
				ini = f;
				break;
			}
		}
		string[] lines = File.ReadAllLines(ini.FullName);
		for (int i = 0; i < lines.Length; ++i)
		{
			if (lines[i].StartsWith("name")) songInfo.name = lines[i].Split("="[0])[1].Trim();
			if (lines[i].StartsWith("artist")) songInfo.artist = lines[i].Split("="[0])[1].Trim();
		}
		songInfo.displayArtist = songInfo.artist + " - " + songInfo.name;
		songInfo.displayName = songInfo.name + " - " + songInfo.artist;
		return songInfo;
	}

	public void LoadSong(FileInfo chartFile)
	{
		StartCoroutine(StartingSong(chartFile));
	}
	private IEnumerator StartingSong(FileInfo chartFile)
	{
		Debug.Log("Loading " + chartFile.FullName);
		Song song = null;
		SongLoader.Instance.Load(chartFile.FullName, delegate (Song _song)
		{
			song = _song;
		});
		fade.color = new Color(0, 0, 0, 0);
		fade.gameObject.SetActive(true);
		while (fade.color.a < 1)
		{
			fade.color += new Color(0, 0, 0, Time.deltaTime);
			yield return null;
		}
		while (song == null) yield return null;
		Debug.Log("Loading audio");
		bool prepared = false;
		SongLoader.Instance.PrepareAudio(song, delegate ()
		 {
			 prepared = true;
		 });

		while (!prepared) yield return null;
		Debug.Log("Initializing session");
		yield return null;
		session.Initialize(song);

		selectScreen.SetActive(false);
		yield return null;
		Debug.Log("Ready to play");
		while (fade.color.a > 0)
		{
			fade.color -= new Color(0, 0, 0, Time.deltaTime);
			yield return null;
		}
		fade.gameObject.SetActive(false);
		session.StartPlaying();
	}

	private IEnumerator EndingSong()
	{
		fade.color = new Color(0, 0, 0, 0);
		fade.gameObject.SetActive(true);
		while (fade.color.a < 1)
		{
			fade.color += new Color(0, 0, 0, Time.deltaTime);
			yield return null;
		}
		session.EndSession();
		selectScreen.SetActive(true);
		while (fade.color.a > 0)
		{
			fade.color -= new Color(0, 0, 0, Time.deltaTime);
			yield return null;
		}
		fade.gameObject.SetActive(false);
	}
}
	
	
