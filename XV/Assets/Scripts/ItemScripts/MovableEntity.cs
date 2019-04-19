using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// This class impl. generic animation movement

public sealed class MovableEntity : AInteraction
{
    private enum EditionMode {
        NONE,
        MOVE,
        ROTATE,
    }

    private UIBubbleInfo mUIBuble;

    private Image mMoveButtonColor;

    private Image mRotateButtonColor;

    private EditionMode mEditionMode;

    private void Start()
	{
        mEditionMode = EditionMode.NONE;
        mUIBuble = null;
        mMoveButtonColor = null;
        mRotateButtonColor = null;
        StartCoroutine(PostPoppingAsync());
    }

    private IEnumerator PostPoppingAsync()
    {
        // Waiting the end of the GameManager initialization of this class
        yield return new WaitForEndOfFrame();

        if ((mUIBuble = GetComponentInChildren<UIBubbleInfo>()) == null)
            yield break;

        // bouton deplacer
        Button lButton;
        lButton = mUIBuble.CreateButton(new UIBubbleInfoButton {
            Text = "Move",
            ClickAction = (iObjectEntity) => {
                Debug.LogWarning("Deplacer: " + iObjectEntity.name + " has been clicked");
                GoTo();
            }
        });
        // Keep track of the button image to edit color
        mMoveButtonColor = lButton.GetComponent<Image>();

        // bouton orienter vers
        lButton = mUIBuble.CreateButton(new UIBubbleInfoButton {
            Text = "Rotate",
            ClickAction = (iObjectEntity) => {
                Debug.LogWarning("Orienter: " + iObjectEntity.name + " has been clicked");
                Rotate();
            }
        });
        // Keep track of the button image to edit color
        mRotateButtonColor = lButton.GetComponent<Image>();



        // TMP
        // bouton debug qui execute les actions de la timeline tempporaire
        // Wait for TimelineManager
        mUIBuble.CreateButton(new UIBubbleInfoButton {
            Text = "Play",
            ClickAction = (iObjectEntity) => {
                if (!TimeLineIsBusy) {
                    Debug.LogWarning("Play clicked");
                    PlayTimeline();
                }
            }
        });
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

                float lDuration = 0F;
                // Check the hit.point clicked is the ground
                // Calcul the distance between position & destination
                // Calcul the Duration of the movement
                // Add the code that do the animation in the Action
                AddToTimeline(() => { Debug.LogWarning("-- MOVE ANIM --"); }, lDuration);
            }
        }
        else if (mEditionMode == EditionMode.ROTATE) {
            // On click leave this mode and continue animation adding process
            if (Input.GetMouseButtonDown(0)) {
                mRotateButtonColor.color = Color.white;
                mEditionMode = EditionMode.NONE;
                // reset cursor

                float lDuration = 0F;
                // Calcul the distance between position & destination
                // Calcul the Duration of the movement
                // Add the code that do the animation in the following Action
                AddToTimeline(() => { Debug.LogWarning("-- ROTATE ANIM --");}, lDuration);
            }
        }
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

/* Code temporaire de CreateButton de UIBubleInfo, a remplacer par une func, dans ObjectEntity => GetComponent<ObjectEntity>().CreateBubbleButton(UIBubbleInfo Button iInfoButton)
    public void CreateButton(UIBubbleInfoButton iInfoButton)
    {
        if (iInfoButton == null)
            return;
        
        GameObject lNewButton = Instantiate(SampleButton, GridContainer.transform);

        Button lButtonComponant = lNewButton.GetComponent<Button>();
        lButtonComponant.onClick.AddListener(() => {
            if (iInfoButton.ClickAction != null)
                iInfoButton.ClickAction(Parent);
        });
        mButtons.Add(lButtonComponant);

        lNewButton.GetComponentInChildren<Text>().text = iInfoButton.Text;
        lNewButton.name = iInfoButton.Text;
        lNewButton.SetActive(true);
        Canvas.ForceUpdateCanvases();
    }

    public void SetInteractable(bool iInteractable) {
        foreach (Button lButton in mButtons) {
            lButton.interactable = iInteractable;
        }
        ModelName.interactable = iInteractable;
    }
*/