using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraHandler : MonoBehaviour {

	public enum Mode { FREE, SUBJECTIVE, LOCKED }

	[SerializeField] private float StandardSpeed;
	[SerializeField] private float FastSpeedMultiplier;
    [SerializeField] private float MouseSensitivity;
	[SerializeField] private Text ViewModeText;

	private bool mIsRepositioning;

	private Mode mViewMode;
	public Mode ViewMode
	{
		get
		{
			return mViewMode;
		}
		set
		{
			if (value == Mode.FREE) {
				mViewMode = Mode.FREE;
				ViewModeText.text = "View mode: FREE";
			}
			else if (value == Mode.SUBJECTIVE) {
				mViewMode = Mode.SUBJECTIVE;
				ViewModeText.text = "View mode: SUBJECTIVE";
				StartCoroutine(SetSubjectivePosition(new Vector3(transform.position.x, 1f, transform.position.z)));
			}
		}
	}

	private Mode mCurrentMode;
	public Mode CurrentMode
	{
		get
		{
			return mCurrentMode;
		}
		set
		{
			if (value != mCurrentMode) {
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
			}
		}
	}

	private void Start()
	{
		ViewMode = Mode.FREE;
		CurrentMode = Mode.LOCKED;
		mIsRepositioning = false;
	}

    void Update()
    {
		// Hold right click to go in 'view mode' (free or subjective), otherwise stay in locked mode
		if (Input.GetMouseButtonDown(1)) {
			CurrentMode = ViewMode;
		}
		else if (Input.GetMouseButtonUp(1)) {
			CurrentMode = Mode.LOCKED;
		}
		// Use tab to switch between 'view modes': free or subjective
		if (Input.GetKeyDown(KeyCode.Tab)) {
			ViewMode = (ViewMode == Mode.FREE) ? Mode.SUBJECTIVE : Mode.FREE;
		}

		if (!mIsRepositioning) {
			ApplyMovement();
		}
		if (CurrentMode != Mode.LOCKED) {
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
		if (ViewMode == Mode.SUBJECTIVE) {
			zAxisMovement.y = 0f;
		}
		// Move up / down if in free mode
		else if (ViewMode == Mode.FREE) {
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
		mCurrentMode = Mode.LOCKED;
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

	private void SetFreeMode()
	{
		mCurrentMode = Mode.FREE;
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	private void SetSubjectiveMode()
	{
		mCurrentMode = Mode.SUBJECTIVE;
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
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
