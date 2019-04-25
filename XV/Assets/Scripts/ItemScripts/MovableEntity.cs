using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

// This class impl. generic animation movement

// --- TODO ---

// Setting up offset rotation for each object that can move
// Find the value of the offset
// The final object hierarchy will be:
// OffsetPositionParent
// |-> OffsetRotationParent
//       |-> Chariot

// Put NavMeshObstacle generation into ObjectEntity

// Update this code

public sealed class MovableEntity : AInteraction
{
    private enum EditionMode
    {
        NONE,
        MOVE,
        ROTATE,
    }

    private const float SPEED = 1.5F;

    private const float LIMIT = 0.2F;

    private ObjectEntity mEntity;

    private Image mMoveButtonColor;

    private Image mRotateButtonColor;

    private EditionMode mEditionMode;

    private NavMeshAgent mAgent;

    private NavMeshObstacle mObstacle;

    [SerializeField]
    private Vector3 RotationOffset;

    private void Start()
    {
        mEditionMode = EditionMode.NONE;
        mEntity = null;
        mMoveButtonColor = null;
        mRotateButtonColor = null;
        StartCoroutine(PostPoppingAsync());
    }

    private IEnumerator PostPoppingAsync()
    {
        // Waiting the end of the GameManager initialization of this class
        yield return new WaitForEndOfFrame();

        if ((mEntity = GetComponentInChildren<ObjectEntity>()) == null)
            yield break;

        // bouton deplacer
        Button lButton;
        lButton = mEntity.CreateBubleInfoButton(new UIBubbleInfoButton {
            Text = "Move",
            ClickAction = (iObjectEntity) => {
                Debug.LogWarning("Deplacer: " + iObjectEntity.name + " has been clicked");
                GoTo();
            }
        });
        // Keep track of the button image to edit color
        mMoveButtonColor = lButton.GetComponent<Image>();

        // bouton orienter vers
        lButton = mEntity.CreateBubleInfoButton(new UIBubbleInfoButton {
            Text = "Rotate",
            ClickAction = (iObjectEntity) => {
                Debug.LogWarning("Orienter: " + iObjectEntity.name + " has been clicked");
                Rotate();
            }
        });
        // Keep track of the button image to edit color
        mRotateButtonColor = lButton.GetComponent<Image>();

        // TMP
        // bouton debug qui execute les actions de la timeline temporaire
        // Wait for TimelineManager
        mEntity.CreateBubleInfoButton(new UIBubbleInfoButton {
            Text = "Play",
            ClickAction = (iObjectEntity) => {
                if (!TimeLineIsBusy) {
                    Debug.LogWarning("Play clicked");
                    PlayTimeline();
                }
            }
        });

        if ((mAgent = transform.parent.gameObject.AddComponent<NavMeshAgent>()) != null) {
            mAgent.radius = (mEntity.Size.x > mEntity.Size.z) ? (mEntity.Size.x / 2) : (mEntity.Size.z / 2);
            mAgent.radius += 0.1F;
            mAgent.baseOffset = -transform.position.y;
            mAgent.stoppingDistance = LIMIT;
            mAgent.enabled = false;
        }

        if ((mObstacle = transform.gameObject.AddComponent<NavMeshObstacle>()) != null) {
            mObstacle.center = mEntity.Center;
            mObstacle.size = mEntity.Size;
            mObstacle.carving = true;
        }

        // Upgrade that
        GameObject parent = new GameObject();
        GameObject tmp;

        // save parent
        tmp = transform.parent.gameObject;

        // change parent
        transform.parent = parent.transform;

        // restore big parent
        parent.transform.parent = tmp.transform;

        transform.position = Vector3.zero;
        transform.localPosition = -mEntity.Center;

        transform.parent.transform.position = Vector3.zero;
        transform.parent.transform.localPosition = Vector3.zero;
        transform.parent.transform.rotation = Quaternion.Euler(RotationOffset);
    }

    private void Update()
    {
        if (mEditionMode == EditionMode.NONE)
            return;
        else if (mEditionMode == EditionMode.MOVE) {
            // On click leave this mode and continue animation adding process
            if (Input.GetMouseButtonDown(0)) {
                
                mMoveButtonColor.color = Color.white;
                mEditionMode = EditionMode.NONE;
                // reset cursors

                Vector3 lHitPoint = Vector3.zero;
                float lDuration = 0F;

                // Check the hit.point clicked is the ground
                if ((GetMouseClic(ref lHitPoint))) {

                    // Calcul the distance between position & destination
                    float lDist = Vector3.Distance(transform.position, lHitPoint);

                    // Calcul the Duration of the movement

                    // Add the code that do the animation in the Action
                    AddToTimeline((iBool) => {

                        // Switch into Agent mode
                        if (mObstacle.enabled) {
                            mObstacle.enabled = false;
                            mAgent.enabled = true;
							mAgent.SetDestination(lHitPoint);
                            return false;
                        }


                        if (mAgent.remainingDistance < LIMIT && mAgent.velocity.x == 0F && mAgent.velocity.z == 0F) {
                            // Switch into obstacle mode
							mAgent.enabled = false;
                            mObstacle.enabled = true;
                            return true;
                        }

                        return false;
                    }, lDuration);

                }
            }
        } else if (mEditionMode == EditionMode.ROTATE) {
            // On click leave this mode and continue animation adding process
            if (Input.GetMouseButtonDown(0)) {
                mRotateButtonColor.color = Color.white;
                mEditionMode = EditionMode.NONE;
                // reset cursor

                float lDuration = 0F;
                // Calcul the distance between position & destination
                // Calcul the Duration of the movement
                // Add the code that do the animation in the following Action
                AddToTimeline((iBool) => { Debug.LogWarning("-- ROTATE ANIM --"); return true; }, lDuration);
            }
        }
    }

    private bool GetMouseClic(ref Vector3 iHitPoint)
    {
        RaycastHit lHit;
        Ray lRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(lRay, out lHit, 1000, LayerMask.GetMask("dropable"))) {
            Debug.DrawRay(lRay.origin, lRay.direction * lHit.distance, Color.red, 1);
            iHitPoint = lHit.point;
            return true;
        }
        return false;
    }

    //  Deplacement animation for all VEHICLE
    //  UIBubleInfo `Move` bind with this func.
    private bool GoTo()
    {
        // Enter in move edition mode
        mEditionMode = EditionMode.MOVE;
        mMoveButtonColor.color = Color.red;

        // Change cursor
        // Warn the user to click somewhere to get a destination
        return true;
    }

    //  Rotation animation for all VEHICLE
    //  UIBubleInfo `Rotate` bind with this func.
    private bool Rotate()
    {
        // Enter in move edition mode
        mEditionMode = EditionMode.ROTATE;
        mRotateButtonColor.color = Color.red;

        // Change cursor
        // Warn the user to click somewhere to get a destination
        return true;
    }
}