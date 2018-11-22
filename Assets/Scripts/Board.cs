using UnityEngine;
public class Board : MonoBehaviour
{
	public Transform boardMesh;
	public Session session;
	void Update()
	{
		Vector3 pos = boardMesh.localPosition;
		pos.z = (session.TickDistanceToMeters((float)session.smoothTick) % 2) * -1f+session.boardLength;
		if (float.IsNaN(pos.z)) return;
		boardMesh.localPosition = pos;
	}
}
