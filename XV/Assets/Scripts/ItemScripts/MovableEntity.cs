using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

// This class impl. generic animation movement

// --- TODO ---

// some object are not correctly on the ground (ex: ConvoyeurPalettes)
// on drag & drop model, some model are shifted in position (ex: gerbeur electrique)

public sealed class MovableEntity : AInteraction
{
    private enum EditionMode
    {
        NONE,
        MOVE,
        ROTATE,
    }

    private const float LIMIT = 0.5F;

    private const float ROT_SPEED = 500F;

    private ObjectEntity mObjectEntity;

    private Image mMoveButtonColor;

    private Image mRotateButtonColor;

    private EditionMode mEditionMode;

    private NavMeshAgent mAgent;

    private NavMeshObstacle mEntityObstacle;

    private GameObject mUITarget;

    private Renderer mUITargetRenderer;

    private GameObject mUITargetTemplate;

    private GameObject mCenteredParent;

    private GameObject mOffsetRotationParent;

    private GameObject mGhostEntity;

    private float mAngle;

    private float mRotationPerformed;

    public MovableEntity SetParent(GameObject iTopParent, GameObject iOffsetRotationParent)
    {
        mCenteredParent = iTopParent;
        mOffsetRotationParent = iOffsetRotationParent;
        return this;
    }

    public MovableEntity SetObjectEntity(ObjectEntity iObjectEntity)
    {
        mObjectEntity = iObjectEntity;
        Debug.Log(mObjectEntity == null);

        // Add all this code to the PostPopping callback of ObjectEntity
        mObjectEntity.PostPoppingAction.Add(() => {

            // Execute this at the next frame
            StartCoroutine(Utils.WaitNextFrameAsync<ObjectEntity>((iObj) => {

                // Add Move button & Keep track of the button image to edit color
                Button lButton;
                Debug.Log(iObj == null);
                lButton = iObj.CreateBubleInfoButton(new UIBubbleInfoButton {
                    Text = "Move",
                    ClickAction = (iObject) => {
                        Debug.LogWarning("Deplacer: " + iObject.name + " has been clicked");
                        OnMoveClick();
                    }
                });
                mMoveButtonColor = lButton.GetComponent<Image>();

                // Add Rotate Button & Keep track of the button image to edit color
                lButton = iObj.CreateBubleInfoButton(new UIBubbleInfoButton {
                    Text = "Rotate",
                    ClickAction = (iObject) => {
                        Debug.LogWarning("Orienter: " + iObject.name + " has been clicked");
                        OnRotateClick();
                    }
                });
                mRotateButtonColor = lButton.GetComponent<Image>();


                // Add NavMeshAgent to move the object
                if ((mAgent = mCenteredParent.AddComponent<NavMeshAgent>()) != null) {
                    // Agent radius is the biggest size of the bounding box
                    mAgent.radius = (iObj.Size.x > iObj.Size.z) ? (iObj.Size.x / 2) : (iObj.Size.z / 2);
                    // Increase a little the radius to avoid limit of a mesh
                    mAgent.radius += 0.1F;
                    // Adjust the cylinder with the height position
                    mAgent.baseOffset = -transform.position.y;
                    // Add a limit to the target destination
                    mAgent.stoppingDistance = LIMIT;
                    // Disable it until is not use
                    mAgent.enabled = false;
                }

                // Get the NavMeshObstacle to perform mutual exclusion with NavMeshAgent
                mEntityObstacle = GetComponent<NavMeshObstacle>();

                // ------ TMP CODE PLAY BUTTON TO PLAY ANIMATION CLICKED -----
                // Debug buttton to execute each actions
                iObj.CreateBubleInfoButton(new UIBubbleInfoButton {
                    Text = "Play",
                    ClickAction = (iObject) => {
                        if (!TimeLineIsBusy) {
                            Debug.LogWarning("Play clicked");
                            PlayTimeline();
                        }
                    }
                });
                // ------ END TMP CODE ---
            }, mObjectEntity));
        });
        return this;
    }

    private void Start()
    {
        mEditionMode = EditionMode.NONE;
        mUITargetTemplate = Resources.Load<GameObject>(GameManager.UI_TEMPLATE_PATH + "UITarget");
    }

    private void Update()
    {
        if (mEditionMode == EditionMode.NONE)
            return;

        else if (mEditionMode == EditionMode.MOVE) {

            // Cancel Movement on press escape
            if (Input.GetKeyDown(KeyCode.Escape)) {
                ResetMode();
                return;
            }

            // -- Movement Preview --
            // Raycast on MousePosition to put UITarget on destination
            RaycastHit lHit;
            Ray lRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(lRay, out lHit, 1000, LayerMask.GetMask("dropable"))) {

                // Make UI follow mouse
                mUITarget.transform.position = new Vector3(lHit.point.x, lHit.point.y + 0.5F, lHit.point.z);

                // Change UI color according to the object Hit
                if (mUITargetRenderer != null && lHit.collider.tag == "scene")
                    mUITargetRenderer.material.color = Color.green;
                else if (mUITargetRenderer != null)
                    mUITargetRenderer.material.color = Color.red;
            }
            else if (mUITargetRenderer != null)
                mUITargetRenderer.material.color = Color.red;

            // On click leave this mode and continue animation movement adding process
            if (Input.GetMouseButtonDown(0))
                Move();

        } else if (mEditionMode == EditionMode.ROTATE) {

            /*  Explication temporaire: comment ajouter une animation de rotation. (Un peu tire par les cheveux je vous l'accorde mais les click souris etait deja pris pour translation / rotation dans ObjectEntity)
            **  Click on rotate button, the button change to red 
            **  Hold left alt key, and just move your mouse (WITHOUT CLICK) to rotate object, release left alt key when your done, (the ghost object rotation will be the final rotation of the object)
            **  press 'r' key to validate the rotation, your rotation has been saved into the timeline - you can click on 'Play'
            **  If you press escape, or if you mouse click during this process, the rotation process is stopped and nothing is add to timeline
            */

            // Cancel Rotation on press escape
            if (Input.GetKeyDown(KeyCode.Escape)) {
                ResetMode();
                return;
            }

            // Orientation preview - When LeftAlt is  pressed a rotation is performed on the ghost.
            if (Input.GetKey(KeyCode.LeftAlt)) {

                // Get increment angle from mouse axis
                float lAngle = Input.GetAxis("Mouse X") * ROT_SPEED * Mathf.Deg2Rad;
                // Compute final angle & clamp it between 0 & 360
                mAngle += lAngle;
                if (mAngle > 360F)
                    mAngle = mAngle % 360F;
                // Rotate the ghost around the center of the object, which is the offset rotation parent
                mGhostEntity.transform.RotateAround(mOffsetRotationParent.transform.position, Vector3.up, lAngle);
            }

            // When 'r' is pressed leave this mode and continue animation orientation adding process
            if (Input.GetKeyDown(KeyCode.R)) {
                Quaternion lStart = mCenteredParent.transform.rotation;
                // Send rotation destination to the function
                Rotate(mCenteredParent.transform.rotation * Quaternion.Euler(lStart.x, lStart.y + mAngle, lStart.z));
            }

            if (Input.GetMouseButtonDown(0))
                ResetMode();
        }
    }

    private void Move()
    {
        // Reset Button & Mode
        ResetMode();

        Vector3 lHitPoint = Vector3.zero;
        float lActionDuration = 0F;

        // Check the hit.point clicked is the ground
        if ((GetHitPointFromMouseClic(ref lHitPoint, "scene"))) {

            // TODO Calcul movement duration ...

            // Add the code that do the animation in the Action timeline
            AddToTimeline((iSpeed) => {
                
                // Check NavMesh component are present
                if (mEntityObstacle == null || mAgent == null) {
                    Debug.LogError("NavMeshAgent or NavMeshObstacle are missing.");
                    return true;
                }

                // Disable Obstacle
                if (mEntityObstacle.enabled) {
                    mEntityObstacle.enabled = false;
                    return false;
                }

                // Active Agent
                mAgent.enabled = true;
                // Update speed
                mAgent.ResetPath();
                mAgent.speed *= iSpeed;
                mAgent.acceleration *= iSpeed;
                mAgent.SetDestination(lHitPoint);

                // Check if we have reached the destination
                if (!mAgent.pathPending) {
                    if (mAgent.remainingDistance <= mAgent.stoppingDistance) {
                        if (!mAgent.hasPath || mAgent.velocity.sqrMagnitude <= float.Epsilon) {
                            // Switch into obstacle mode
                            mAgent.enabled = false;
                            mEntityObstacle.enabled = true;
                            // End of this Action
                            return true;
                        }
                    }
                }

                return false;
            } , lActionDuration);

        }
    }

    private void Rotate(Quaternion iTarget)
    {
        // Reset Button & Mode
        ResetMode();

        // The duration is 2s by default for now
        float lActionDuration = 2F;

        // Add the code that do the animation in the following Action
        AddToTimeline((iSpeed) => {

            // Update rotation performed according to speed
            mRotationPerformed += Time.deltaTime * iSpeed;

            // Rotate to the correct amount
            mCenteredParent.transform.rotation = Quaternion.Slerp(mCenteredParent.transform.rotation, iTarget, mRotationPerformed / lActionDuration);

            // When the counter reach the duration, the rotation is finished
            if (mRotationPerformed > lActionDuration) {
                // End of this Action
                mRotationPerformed = 0F;
                return true;
            }
            return false;

        }, lActionDuration);
    }

    //  Deplacement animation for all movable object
    //  UIBubleInfo `Move` is bind with this func.
    private bool OnMoveClick()
    {
        if (mEditionMode == EditionMode.NONE) {
            // Enter in move edition mode
            mEditionMode = EditionMode.MOVE;
            mMoveButtonColor.color = Color.red;
            mUITarget = Instantiate(mUITargetTemplate);

            // Get the renderer of the UI object
            if (mUITarget != null)
                mUITargetRenderer = mUITarget.GetComponent<Renderer>();

            // Warn the user to click somewhere to get a destination
            XV_UI.Instance.Notify(1F, "Click on a destination !");
        }
        else
            ResetMode();
        return true;
    }

    //  Rotation animation for all movable
    //  UIBubleInfo `Rotate` is bind with this func.
    private bool OnRotateClick()
    {
        if (mEditionMode == EditionMode.NONE) {
            // Enter in rotate edition mode
            mEditionMode = EditionMode.ROTATE;
            mRotateButtonColor.color = Color.red;

            // Create a Ghost clone to preview rotation
            mGhostEntity = mObjectEntity.CreateGhostObject();

            // Cancel rotation if error
            if (mGhostEntity == null) {
                XV_UI.Instance.Notify(1.5F, "Something went wrong !");
                Debug.LogError("Error on Instantiate GameObject");
                ResetMode();
                return true;
            }

            // Warn the user to click somewhere to get an orientation
            XV_UI.Instance.Notify(2.5F, "Choose an orientation (LeftAlt + MouseMotion) Then press 'r' to validate.");
        }
        else
            ResetMode();
        return true;
    }

    // Reset all variable to retrieve neutral edition mode
    private void ResetMode()
    {
        // Reset Button & Mode
        mMoveButtonColor.color = Color.white;
        mRotateButtonColor.color = Color.white;
        mEditionMode = EditionMode.NONE;
        mAngle = 0F;
        mRotationPerformed = 0F;
        if (mUITarget != null)
            Destroy(mUITarget);
        if (mGhostEntity != null)
            Destroy(mGhostEntity);
    }

    // Trace ray from camera according to mouse position, and give the hit if it's 'walkable area'
    private bool GetHitPointFromMouseClic(ref Vector3 iHitPoint, string iTag)
    {
        RaycastHit lHit;
        Ray lRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(lRay, out lHit, 1000, LayerMask.GetMask("dropable"))) {
            Debug.DrawRay(lRay.origin, lRay.direction * lHit.distance, Color.red, 1);
            if (lHit.collider.tag != iTag)
                return false;
            iHitPoint = lHit.point;
            return true;
        }
        return false;
    }
}