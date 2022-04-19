using JetBrains.Annotations;
using src.museum.quiz.script;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class MyHoverButton : HoverButton
{
    public Vector3 localMoveDistance = new Vector3(0, -0.1f, 0);

    [Range(0, 1)] public float engageAtPercent = 0.95f;

    [Range(0, 1)] public float disengageAtPercent = 0.9f;

    public bool engaged = false;
    public bool buttonDown = false;
    public bool buttonUp = false;

    private Vector3 startPosition;
    private Vector3 endPosition;

    private Vector3 handEnteredPosition;

    private bool hovering;

    [Range(0, 30)] public float buttonDownDelay;

    private void Start()
    {
        if (movingPart == null && this.transform.childCount > 0)
            movingPart = this.transform.GetChild(0);

        startPosition = movingPart.localPosition;
        endPosition = startPosition + localMoveDistance;
        handEnteredPosition = endPosition;
    }

    [UsedImplicitly]
    private void HandHoverUpdate(Hand hand)
    {
        hovering = true;

        bool wasEngaged = engaged;

        float currentDistance =
            Vector3.Distance(movingPart.parent.InverseTransformPoint(hand.transform.position), endPosition);
        float enteredDistance = Vector3.Distance(handEnteredPosition, endPosition);

        if (currentDistance > enteredDistance)
        {
            enteredDistance = currentDistance;
            handEnteredPosition = movingPart.parent.InverseTransformPoint(hand.transform.position);
        }

        float distanceDifference = enteredDistance - currentDistance;

        float lerp = Mathf.InverseLerp(0, localMoveDistance.magnitude, distanceDifference);

        if (lerp > engageAtPercent)
            engaged = true;
        else if (lerp < disengageAtPercent)
            engaged = false;

        movingPart.localPosition = Vector3.Lerp(startPosition, endPosition, lerp);

        InvokeEvents(wasEngaged, engaged);
    }

    private void LateUpdate()
    {
        if (hovering == false)
        {
            movingPart.localPosition = startPosition;
            handEnteredPosition = endPosition;
            engaged = false;
        }
        else
        {
            gameObject.GetComponent<QuizComposer>().Check();
        }

        hovering = false;
    }

    private void InvokeEvents(bool wasEngaged, bool isEngaged)
    {

    }
}
