using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteRenderer : MonoBehaviour
{
	public Camera camera2D, camera3D;
	public Session session;
	public SpriteData spriteData;
	public Dictionary<Session.NoteInstance, SpriteRenderer> mapping;
	public List<Session.NoteInstance> activeList;
	public GameObject[] prefabs;
	public SpriteRenderer[][] pool;
	public Transform[] top, bottom;
	public int[] poolIndex;
	public float lerpPow = 1;
	public int poolSize = 128;
	public int frameIndex;

	[System.Serializable]
	public class SpriteData
	{
		public FredSpriteData[] fred;
	}
	[System.Serializable]
	public class FredSpriteData
	{
		public Sprite normal, hammerOn;
		public Sprite[] star, starHammerOn;
	}
	public void Initialize()
	{
		mapping = new Dictionary<Session.NoteInstance, SpriteRenderer>();
		pool = new SpriteRenderer[prefabs.Length][];
		poolIndex = new int[prefabs.Length];
		activeList = new List<Session.NoteInstance>();
		for (int i = 0; i < prefabs.Length; ++i)
		{
			pool[i] = MakePool(prefabs[i]);
		//	Debug.Log(pool[i]);
		}
	}
	public void AddMap(Session.NoteInstance noteInstance)
	{
		uint fred = noteInstance.fred;
		SpriteRenderer spriteRenderer = (pool[fred])[poolIndex[fred] % poolSize];
		activeList.Add(noteInstance);
		mapping.Add(noteInstance, spriteRenderer);

		spriteRenderer.gameObject.SetActive(true);
		poolIndex[fred]++;
	}
	public void RemoveMap(Session.NoteInstance noteInstance)
	{
		activeList.Remove(noteInstance);
		mapping[noteInstance].gameObject.SetActive(false);
		mapping.Remove(noteInstance);

	}
	public SpriteRenderer[] MakePool(GameObject prefab)
	{
		SpriteRenderer[] newPool = new SpriteRenderer[poolSize];
		GameObject poolObject = new GameObject(prefab.name + " pool");
		poolObject.transform.SetParent(prefab.transform.parent);
		for (int i = 0; i < poolSize; ++i)
		{
			GameObject g = Instantiate(prefab);
			g.SetActive(false);
			newPool[i] = g.GetComponent<SpriteRenderer>();
			g.transform.SetParent(poolObject.transform);
		}
		return newPool;
	}
	public void NewFrame()
	{
		frameIndex++;
	}
	public void RenderNote(Session.NoteInstance noteInstance, float meters)
	{
		//position
		float lerp = meters / session.boardLength;
		Transform topTransform = top[noteInstance.fred];
		Transform bottomTransform = bottom[noteInstance.fred];
		SpriteRenderer spriteRenderer = mapping[noteInstance];
		spriteRenderer.transform.position = Vector3.Lerp(bottomTransform.position, topTransform.position, lerp);
		Vector3 screenPoint = camera3D.WorldToScreenPoint(noteInstance.noteModel.transform.position);
		Vector3 point2D = camera2D.ScreenToWorldPoint(screenPoint);
		point2D.z = noteInstance.noteModel.transform.position.z;
		spriteRenderer.transform.position = point2D;
		float powResult = Mathf.Pow(lerp, lerpPow);
		spriteRenderer.transform.localScale = Vector3.Lerp(bottomTransform.localScale, topTransform.localScale, float.IsNaN(powResult) ? 0 : powResult);

		//correctSprite
		FredSpriteData fredSpriteData = spriteData.fred[noteInstance.fred];
		if (noteInstance.star)
		{
			spriteRenderer.sprite = (noteInstance.hammeron) ? fredSpriteData.starHammerOn[frameIndex % 16] : fredSpriteData.star[frameIndex % 16];
		}
		else
		{
			spriteRenderer.sprite = (noteInstance.hammeron) ? fredSpriteData.hammerOn : fredSpriteData.normal;
		}
	}
}
