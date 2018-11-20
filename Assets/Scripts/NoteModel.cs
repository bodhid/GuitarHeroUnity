using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteModel : MonoBehaviour
{
	public Transform myTransform;
	public SpriteRenderer spriteRenderer;
	public MeshRenderer line;
	public Material materialInstance;
	public void SetLengt(float meters)
	{
		Vector3 localPos = line.transform.localPosition;
		localPos.z = meters * 0.5f;
		line.transform.localPosition = localPos;
		Vector3 localScale = line.transform.localScale;
		localScale.y = meters ;
		line.transform.localScale = localScale;
		materialInstance.mainTextureScale = new Vector2(1, localScale.y / localScale.x);
		materialInstance.mainTextureOffset =new Vector2(0, -localScale.y / localScale.x + 1);
	}
}
