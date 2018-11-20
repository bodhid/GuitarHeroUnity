using UnityEngine;
public class Board : MonoBehaviour
{
	public Session session;
	void Update()
	{
		Vector3 pos = transform.localPosition;
		pos.z = (session.TickDistanceToMeters((float)session.smoothTick) % 2) * -1f+session.boardLength;
		if (float.IsNaN(pos.z)) return;
		transform.localPosition = pos;
	}
}
