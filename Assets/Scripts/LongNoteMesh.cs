using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongNoteMesh : MonoBehaviour {

	// Use this for initialization
	void Start()
	{
		MeshFilter mf = GetComponent<MeshFilter>();
		Mesh m = mf.mesh;
		Vector3[] vertices = m.vertices;
		for (int i = 0; i < vertices.Length; ++i)
		{
			if (vertices[i].y > 0)
			{
				vertices[i].x = vertices[i].x * 0.1f;
			}
		}
		m.vertices = vertices;
		mf.mesh = m;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
