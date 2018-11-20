using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SpriteAnimation : MonoBehaviour {
	public bool loop;
	public bool flip;
	public int fps=60;
	public int index = 0;
	public float seconds = 0;
	public int gridX = 4;
	public int gridY = 4;
	public float x;
	public float y;
	public RawImage rawImage;

	private void Awake()
	{
		Rect uvRect = rawImage.uvRect;
		uvRect.width = 1f / gridX;
		uvRect.height = 1f / gridY;
		rawImage.uvRect = uvRect;
	}

	void OnEnable ()
	{
		seconds = 0;
		index = 0;
		if (flip)
		{
			Vector3 scale = transform.localScale;
			scale.x = -scale.x;
			transform.localScale = scale;
		}
		}

	public void Reset()
	{
		OnEnable();
	}

	void Update ()
	{
		seconds += Time.deltaTime;
		index = Mathf.FloorToInt( seconds * fps);
		if (index == gridX * gridY)
		{
			index = 0;
			if (!loop) gameObject.SetActive(false);
		}
		Rect uvRect = rawImage.uvRect;
		int indexX = (index % gridX);
		x = uvRect.x = indexX * uvRect.width;
		y = uvRect.y = ((1f - uvRect.height) - ((index - indexX) / gridX * uvRect.height)) % 1;
		rawImage.uvRect = uvRect;
	}
}
