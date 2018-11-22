using UnityEngine;
public class NoteRenderer : MonoBehaviour
{
	public SpriteData spriteData;
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
}
