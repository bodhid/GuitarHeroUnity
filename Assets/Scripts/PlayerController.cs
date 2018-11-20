using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : MonoBehaviour {

    Rigidbody rig;
    public GameObject cam;

    public int currentHealth = 100;

    public float walkSpeed; 
    public float runSpeed;
    public float mouseSensitivity;

    RaycastHit hit;
    Ray ray;


    Vector3 targetRotCam;
    Vector3 targetRotBody;

    Vector3 view;

    // Use this for initialization
    void Start ()
    {

            rig = GetComponent<Rigidbody>();
            cam = GameObject.Find("Main Camera");
            cam.SetActive(true);
            cam.transform.parent = this.transform;
            cam.transform.localPosition = new Vector3(0, 0.6f, 0);
        }	
	// Update is called once per frame
	void Update () 
	{
        Movement();
		}

	private Vector3 euler;
	void Movement()
	{
		euler.y += Input.GetAxis("Mouse X") * mouseSensitivity;
		euler.x += Input.GetAxis("Mouse Y") * mouseSensitivity;
		cam.transform.eulerAngles = euler;

		Vector2 movement = Vector2.zero;
		movement.x -= Input.GetKey(KeyCode.A) ? 1 : 0;
		movement.x += Input.GetKey(KeyCode.D) ? 1 : 0;
		movement.y -= Input.GetKey(KeyCode.S) ? 1 : 0;
		movement.y += Input.GetKey(KeyCode.W) ? 1 : 0;
		Vector3 velocity = Vector3.zero;
		velocity += cam.transform.forward * movement.y;
		velocity += cam.transform.right * movement.x;
		velocity.y = 0;

		rig.velocity = velocity.normalized * Time.deltaTime * walkSpeed;
	}
}