using UnityEngine;

[RequireComponent(typeof(Camera))]
public class GhostFreeRoamCamera : MonoBehaviour
{
	public float initialSpeed = 10f;
	public float increaseSpeed = 1.25f;
	
	public bool allowMovement = true;
	public bool allowRotation = true;
	
	public KeyCode forwardButton = KeyCode.W;
	public KeyCode backwardButton = KeyCode.S;
	public KeyCode rightButton = KeyCode.D;
	public KeyCode leftButton = KeyCode.A;
	
	public float cursorSensitivity = 0.025f;
	public bool cursorToggleAllowed = true;
	public KeyCode cursorToggleButton = KeyCode.Escape;
	
	private float currentSpeed = 0f;
	private bool moving = false;
	private bool togglePressed = false;
	
	private Rigidbody rb;

	private void OnEnable()
	{
		if (cursorToggleAllowed)
		{
			Screen.lockCursor = true;
			Cursor.visible = false;
		}
	}
	
	private void Update()
	{
			if (allowMovement)
		{
			bool lastMoving = moving;
			Vector3 deltaPosition = Vector3.zero;
			
			if (moving)
				currentSpeed += increaseSpeed * Time.deltaTime;
			
			moving = false;
			
			CheckMove(forwardButton, ref deltaPosition, transform.forward);
			CheckMove(backwardButton, ref deltaPosition, -transform.forward);
			CheckMove(rightButton, ref deltaPosition, transform.right);
			CheckMove(leftButton, ref deltaPosition, -transform.right);
			
			if (moving)
			{
				if (moving != lastMoving)
					currentSpeed = initialSpeed;
				
				transform.position += deltaPosition * currentSpeed * Time.deltaTime;
			}
			else currentSpeed = 0f;            
		}
		
		if (allowRotation)
		{
			Vector3 eulerAngles = transform.eulerAngles;
			eulerAngles.x += -Input.GetAxis("Mouse Y") * 359f * cursorSensitivity;
			eulerAngles.y += Input.GetAxis("Mouse X") * 359f * cursorSensitivity;
			transform.eulerAngles = eulerAngles;
		}
		
		if (cursorToggleAllowed)
		{
			if (Input.GetKey(cursorToggleButton))
			{
				if (!togglePressed)
				{
					togglePressed = true;
					Screen.lockCursor = !Screen.lockCursor;
					Cursor.visible = !Cursor.visible;
				}
			}
			else togglePressed = false;
		}
		else
		{
			togglePressed = false;
			Cursor.visible = false;
		}
		
	}

	private void Start() {	
		rb = GetComponent<Rigidbody> ();
	}

	private void CheckMove(KeyCode keyCode, ref Vector3 deltaPosition, Vector3 directionVector)
	{
		if (Input.GetKey(keyCode))
		{
			moving = true;
			deltaPosition += directionVector;
		}
	}
	void OnTriggerEnter(Collider other) {
		if (other.tag == "Boundary") {
			currentSpeed = 0;
//			Debug.Log(other.gameObject.name);
//			if (transform.position.x == other.transform.position.x) {
//				Debug.Log("x " + other.gameObject.name);
//			}
//			if (transform.position.y == other.transform.position.y) {
//				Debug.Log ("y " + other.gameObject.name);
//			}
//			if (transform.position.z == other.transform.position.z) {
//				Debug.Log("z " + other.gameObject.name);
//			}
//			rb.AddForce(other.transform.up);
		}
	}
	
	void OnTriggerStay(Collider other) {
		if (other.tag == "Boundary") {
			currentSpeed = 0;
//			Debug.Log(other.gameObject.name);
//			if (transform.position.x == other.transform.position.x) {
//				Debug.Log("x " + other.gameObject.name);
//			}
//			if (transform.position.y == other.transform.position.y) {
//				Debug.Log ("y " + other.gameObject.name);
//			}
//			if (transform.position.z == other.transform.position.z) {
//				Debug.Log("z " + other.gameObject.name);
//			}
		}
//		rb.AddForce(other.transform.up);
//		Debug.Log(other.gameObject.name);
	}


// NOTE:* Set the Euler angles above to define the relative orientation of the capsule (it depends on the cube axes).
}
