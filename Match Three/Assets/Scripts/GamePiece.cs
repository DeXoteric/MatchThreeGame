using System.Collections;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    [SerializeField] private int xIndex;
    [SerializeField] private int yIndex;
    [SerializeField] private InterpolationType interpolationType = InterpolationType.SmootherStep;
    [SerializeField] public MatchValue matchValue;

    private Board board;

    private bool isMoving = false;

    public enum InterpolationType
    {
        Linear,
        EaseOut,
        EaseIn,
        SmoothStep,
        SmootherStep
    };

    public enum MatchValue
    {
        Yellow,
        Orange,
        Red,
        Blue,
        Green,
        Purple,
        Cyan,
        Wild
    }

    
    private void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Move((int)transform.position.x + 1, (int)transform.position.y, 0.5f);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move((int)transform.position.x - 1, (int)transform.position.y, 0.5f);
        }
        */
    }

    public void Init(Board board)
    {
        this.board = board;
    }

    public void SetCoord(int x, int y)
    {
        xIndex = x;
        yIndex = y;
    }

    public void Move(int destX, int destY, float timeToMove)
    {
        if (!isMoving)
        {
            StartCoroutine(MoveCoroutine(new Vector3(destX, destY, 0), timeToMove));
        }
    }

    private IEnumerator MoveCoroutine(Vector3 destination, float timeToMove)
    {
        Vector3 startPosition = transform.position;
        bool reachedDestination = false;
        float elsapsedTime = 0f;

        isMoving = true;

        while (!reachedDestination)
        {
            if (Vector3.Distance(transform.position, destination) < 0.01f)
            {
                reachedDestination = true;
                if (board != null)
                {
                    board.PlaceGamePiece(this, (int)destination.x, (int)destination.y);
                }
                break;
            }

            elsapsedTime += Time.deltaTime;

            float t = Mathf.Clamp(elsapsedTime / timeToMove, 0f, 1f);

            switch (interpolationType)
            {
                case InterpolationType.Linear:
                    break;

                case InterpolationType.EaseOut:
                    t = Mathf.Sin(t * Mathf.PI * 0.5f);
                    break;

                case InterpolationType.EaseIn:
                    t = 1 - Mathf.Cos(t * Mathf.PI * 0.5f);
                    break;

                case InterpolationType.SmoothStep:
                    t = t * t * (3 - 2 * t);
                    break;

                case InterpolationType.SmootherStep:
                    t = t * t * t * (t * (t * 6 - 15) + 10);
                    break;
            }

            transform.position = Vector3.Lerp(startPosition, destination, t);

            yield return null; //wait until next frame
        }

        isMoving = false;
    }
}