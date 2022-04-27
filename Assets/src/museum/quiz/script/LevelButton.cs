using JetBrains.Annotations;
using src.museum.quiz.script;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class LevelButton : HoverButton
{
    public new Vector3 localMoveDistance = new Vector3(0, -0.1f, 0);

    private Vector3 _startPosition;
    private Vector3 _endPosition;

    private Vector3 _handEnteredPosition;

    private bool _hovering;

    private void Start()
    {
        if (movingPart == null && transform.childCount > 0)
            movingPart = transform.GetChild(0);

        _startPosition = movingPart.localPosition;
        _endPosition = _startPosition + localMoveDistance;
        _handEnteredPosition = _endPosition;
    }

    [UsedImplicitly]
    private void HandHoverUpdate(Hand hand)
    {
        _hovering = true;

        float currentDistance =
            Vector3.Distance(movingPart.parent.InverseTransformPoint(hand.transform.position), _endPosition);
        float enteredDistance = Vector3.Distance(_handEnteredPosition, _endPosition);

        if (currentDistance > enteredDistance)
        {
            enteredDistance = currentDistance;
            _handEnteredPosition = movingPart.parent.InverseTransformPoint(hand.transform.position);
        }

        float distanceDifference = enteredDistance - currentDistance;

        float lerp = Mathf.InverseLerp(0, localMoveDistance.magnitude, distanceDifference);

        movingPart.localPosition = Vector3.Lerp(_startPosition, _endPosition, lerp);

    }

    private void LateUpdate()
    {
        if (_hovering == false)
        {
            movingPart.localPosition = _startPosition;
            _handEnteredPosition = _endPosition;
        }
        else
        {
            gameObject.GetComponent<QuizComposer>().Check();
        }

        _hovering = false;
    }
}
