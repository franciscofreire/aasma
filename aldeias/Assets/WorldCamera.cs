using UnityEngine;
using System.Collections;

public class WorldCamera : MonoBehaviour {
	public float smooth = 2.0f;
	public float moveSpeed = 10f;
	public float turnSpeed = 50f;

	void Start () {
		logStatus ();
	}

	void Update () {
		if (Input.GetKey("1")) {
			transform.rotation = Quaternion.Euler(new Vector3(75, 0, 0));
			transform.position = new Vector3(25, 50, 10);
		}
		if (Input.GetKey("2")) {
			transform.rotation = Quaternion.Euler(new Vector3(15, 0, 0));
			transform.position = new Vector3(25, 5, -10);
		}

		if (Input.GetKey("a"))
			transform.Rotate(Vector3.left, turnSpeed * Time.deltaTime);
		if (Input.GetKey("s"))
			transform.Rotate(Vector3.right, turnSpeed * Time.deltaTime);
		if (Input.GetKey("z"))
			transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
		if (Input.GetKey("x"))
			transform.Translate(Vector3.back * moveSpeed * Time.deltaTime);

		if(Input.GetKey(KeyCode.UpArrow))
			transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.World);
		if(Input.GetKey(KeyCode.DownArrow))
			transform.Translate(Vector3.back * moveSpeed * Time.deltaTime, Space.World);
		if(Input.GetKey(KeyCode.LeftArrow))
			transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
		if(Input.GetKey(KeyCode.RightArrow))
			transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
	}
	
	void logStatus() {
		Debug.Log("position: " + transform.position +
		          "rotation: " + transform.rotation);
	}
}
