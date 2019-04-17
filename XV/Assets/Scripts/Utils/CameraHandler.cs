using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraHandler : MonoBehaviour {

	public enum Mode { FREE, SUBJECTIVE, LOCKED }

	[SerializeField] private float StandardSpeed;
	[SerializeField] private float FastSpeedMultiplier;
    [SerializeField] private float MouseSensitivity;
	[SerializeField] private Text CameraModeText;

	private bool mIsRepositioning;

	private Mode mMode;
	public Mode CameraMode
	{
		get
		{
			return mMode;
		}
		set
		{
			if (value != CameraMode) {
				switch (value)
				{
					case Mode.LOCKED:
						SetLockedMode();
						break;
					case Mode.FREE:
						SetFreeMode();
						break;
					case Mode.SUBJECTIVE:
						SetSubjectiveMode();
						break;
				}
				CameraModeText.text = "Camera: " + value.ToString();
			}
		}
	}

	private void Start()
	{
		CameraMode = Mode.LOCKED;
		mIsRepositioning = false;
	}

    void Update()
    {
		// Set camera modes using 1 / 2 / 3 keys
		if (Input.GetKeyDown(KeyCode.Alpha1)) {
			CameraMode = Mode.LOCKED;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2)) {
			CameraMode = Mode.FREE;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3)) {
			CameraMode = Mode.SUBJECTIVE;
		}
		// No movement if in locked mode
		if (CameraMode != Mode.LOCKED)
		{
			if (!mIsRepositioning) {
				ApplyMovement();
			}
			ApplyRotation();
		}
    }

	private void ApplyMovement()
	{
		bool fastMode = Input.GetKey(KeyCode.LeftShift);
		float baseSpeed = fastMode ? StandardSpeed * FastSpeedMultiplier : StandardSpeed;

		float xSpeed = Input.GetAxis("Horizontal") * baseSpeed * Time.deltaTime;
		float zSpeed = Input.GetAxis("Vertical") * baseSpeed * Time.deltaTime;

		Vector3 xAxisMovement = transform.right * xSpeed;
		Vector3 zAxisMovement = transform.forward * zSpeed;

		// Freeze Y axis movement if in subjective mode
		if (CameraMode == Mode.SUBJECTIVE) {
			zAxisMovement.y = 0f;
		}
		// Move up / down if in free mode
		else {
			float ySpeed = Input.GetAxis("Jump") * baseSpeed * Time.deltaTime;
			transform.position += Vector3.up * ySpeed;
		}

		transform.position += xAxisMovement;
		transform.position += zAxisMovement;
	}

	private void ApplyRotation()
	{
		float newRotationX = transform.eulerAngles.y + Input.GetAxis("Mouse X") * MouseSensitivity;
		float newRotationY = transform.eulerAngles.x - Input.GetAxis("Mouse Y") * MouseSensitivity;
		if (newRotationY > 90f && newRotationY < 270f) {
			if (newRotationY < 180f) {
				newRotationY = 90f;
			}
			else {
				newRotationY = 270f;
			}
		}
		transform.eulerAngles = new Vector3(newRotationY, newRotationX, 0f);
	}

    private void SetLockedMode()
	{
		mMode = Mode.LOCKED;
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

	private void SetFreeMode()
	{
		mMode = Mode.FREE;
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	private void SetSubjectiveMode()
	{
		mMode = Mode.SUBJECTIVE;
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
		StartCoroutine(SetSubjectivePosition(new Vector3(transform.position.x, 1f, transform.position.z)));
	}

	private IEnumerator SetSubjectivePosition(Vector3 iTarget)
	{
		Vector3 velocity = Vector3.zero;
		mIsRepositioning = true;
		while (Vector3.Distance(transform.position, iTarget) > 0.1f)
		{
			transform.position = Vector3.SmoothDamp(transform.position, iTarget, ref velocity, 0.3f);
			yield return new WaitForEndOfFrame();
		}
		mIsRepositioning = false;
		yield break;
	}
}
