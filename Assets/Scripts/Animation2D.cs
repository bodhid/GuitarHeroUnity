using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animation2D : MonoBehaviour
{
	public SpriteRenderer spriteRenderer;
	public Sprite[] frames;
	public bool loop;
	public bool flip;
	public int fps = 60;
	public int index = 0;
	public float seconds = 0;

	void OnEnable()
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

	void Update()
	{
		seconds += Time.deltaTime;
		index = Mathf.FloorToInt(seconds * fps);
		if (index >= frames.Length)
		{
			if (!loop) gameObject.SetActive(false);
		}
		index = index % frames.Length;
		spriteRenderer.sprite = frames[index];
	}
}
