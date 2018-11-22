using UnityEngine;
using UnityEngine.UI;
public class SessionRenderer : MonoBehaviour
{
	public RectTransform[] outputs;
	public RectTransform group;
	public void Initialize(RenderTexture[] textures)
	{
		for (int i = 0; i < outputs.Length; ++i)
		{
			outputs[i].gameObject.SetActive(i < textures.Length);
			
		}
		for (int i = 0; i < textures.Length; ++i)
		{
			outputs[i].GetComponent<RawImage>().texture = textures[i];
		}
		RectTransform myRect = (RectTransform)transform;
		Vector2 groupSizeDelta = group.sizeDelta;
		groupSizeDelta.x = myRect.sizeDelta.x;
		groupSizeDelta.y = myRect.sizeDelta.x / textures.Length;
		if (groupSizeDelta.y > myRect.sizeDelta.y)
		{
			groupSizeDelta *= myRect.sizeDelta.y / groupSizeDelta.y;
		}
		group.sizeDelta = groupSizeDelta;
	}
}