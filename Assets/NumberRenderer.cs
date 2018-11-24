using UnityEngine;
public class NumberRenderer : MonoBehaviour
{
	public SpriteRenderer mySpriteRenderer;
	public Sprite[] number;
	public float time = 0;
	public float startMotion, endMotion;
	public void UpdateNumber()
	{
		if (time < 1) time = Mathf.Min(time + Time.deltaTime, 1);
		Vector3 localPos = transform.localPosition;
		localPos.y = Mathf.LerpUnclamped(startMotion, endMotion, elastic(time));
		transform.localPosition = localPos;
	}
	private float elastic(float p)
	{
		return 0.5f * (Mathf.Sin(-13 * (Mathf.PI*0.5f) * ((2 * p - 1) + 1)) * Mathf.Pow(2, -10 * (2 * p - 1)) + 2);
	}
}
