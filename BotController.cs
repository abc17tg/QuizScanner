using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotController : BasePlayer
{
    [SerializeField, Range(0.1f, 3.0f)] private float m_timeFinishMovePaddle;
    [SerializeField, Range(0f, 50f)] private float m_distanceToReact;
    [SerializeField] private Transform m_ballDetector;

    private RaycastHit m_hit;
    private GameObject m_closestBall;
    private Tween m_tween;

    /*protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (BallManager.Instance.Balls.Count == 0)
            return;

        m_closestBall = BallManager.Instance.FindClosestBall(m_ballDetector);
        if (m_closestBall == null)
            return;

        bool isGoalHitted = Physics.Raycast(m_closestBall.transform.position,
            m_closestBall.GetComponent<Rigidbody>().velocity.normalized,
            out m_hit, m_distanceToReact, LayerMask.GetMask("PlayerGoal"));

        if (isGoalHitted && m_hit.transform.Equals(m_goalPlayerTransform))
            PaddleMove(AngleUtils.FindAngleBetween3Points(BallSpawnPosition.position, Shield.position, m_hit.point));
    }*/

    private void Start()
    {
        //StartCoroutine(BotMove()); //TODO zmienić na wykonanie na event rozpoczęcia gry
        StartCoroutine(BotMove2zero());
    }

    private IEnumerator BotMove()
    {
        while (BallManager.Instance.Balls.Count == 0)
            yield return null;

        while (true)
        {
            do
            {
                m_closestBall = BallManager.Instance.FindClosestBall(m_ballDetector);
                yield return null;
            } while (m_closestBall == null);

            bool isGoalHitted = Physics.Raycast(m_closestBall.transform.position,
                m_closestBall.GetComponent<Rigidbody>().velocity.normalized,
                out m_hit, m_distanceToReact, LayerMask.GetMask("PlayerGoal"));

            if (isGoalHitted && m_hit.transform.Equals(GoalPlayerTransform))
                break;

            yield return null;
        }

        Shield.transform.DOLookAt(2 * Shield.position - m_hit.point, m_timeFinishMovePaddle, AxisConstraint.Y)
            .SetEase(Ease.InOutBounce)
            .OnComplete(() => StartCoroutine(BotMove()))
            .SetAutoKill();
    }

    private IEnumerator BotMove2zero()
    {
        while (BallManager.Instance.Balls.Count == 0)
            yield return null;
        List<GameObject> m_closestBalls = new List<GameObject>();
        while (true)
        {
            do
            {
                m_closestBalls = BallManager.Instance.FindClosestBalls(m_ballDetector);
                yield return null;
            } while (m_closestBalls.Count == 0);

            bool isGoalHitted = false;

            foreach (var ball in m_closestBalls)
            {
                isGoalHitted = Physics.Raycast(ball.transform.position,
                ball.GetComponent<Rigidbody>().velocity.normalized,
                out m_hit, m_distanceToReact, LayerMask.GetMask("PlayerGoal"));

                if (isGoalHitted && m_hit.transform.Equals(GoalPlayerTransform))
                    break;
            }

            if (isGoalHitted)
                break;

            yield return null;
        }

        m_tween = Shield.transform.DOLookAt(2 * Shield.position - m_hit.point, m_timeFinishMovePaddle, AxisConstraint.Y)
            .SetEase(Ease.InOutBounce)
            .OnUpdate(() =>
            {
                if (Shield.transform.localRotation.eulerAngles.y > m_maxPaddleAngle ||
                Shield.transform.localRotation.eulerAngles.y < -m_maxPaddleAngle)
                    m_tween.Kill();
            })
            .OnKill(() => StartCoroutine(BotMove()))
            .SetAutoKill();
    }
}




using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotController : BasePlayer
{
    [SerializeField, Range(0.1f, 3.0f)] private float m_timeFinishMovePaddle;
    [SerializeField, Range(0f, 50f)] private float m_distanceToReact;
    [SerializeField, Range(0f, 50f)] private float m_paddleSpeed;
    [SerializeField] private Transform m_ballDetector;

    private RaycastHit m_hit;
    private GameObject m_closestBall;
    private List<GameObject> m_closestBalls;
    private Tween m_tween;

    private void Start()
    {
        //StartCoroutine(BotMove()); //TODO zmienić na wykonanie na event rozpoczęcia gry
        //StartCoroutine(BotMove2zero());
    }

    private IEnumerator BotMove()
    {
        while (BallManager.Instance.Balls.Count == 0)
            yield return null;

        while (true)
        {
            do
            {
                m_closestBall = BallManager.Instance.FindClosestBall(m_ballDetector);
                yield return null;
            } while (m_closestBall == null);

            bool isGoalHitted = Physics.Raycast(m_closestBall.transform.position,
                m_closestBall.GetComponent<Rigidbody>().velocity.normalized,
                out m_hit, m_distanceToReact, LayerMask.GetMask("PlayerGoal"));

            if (isGoalHitted && m_hit.transform.Equals(m_goalPlayerTransform))
                break;

            yield return null;
        }

        Shield.transform.DOLookAt(2 * m_shield.position - m_hit.point, m_timeFinishMovePaddle, AxisConstraint.Y)
            .SetEase(Ease.InOutBounce)
            .OnComplete(() => StartCoroutine(BotMove()))
            .SetAutoKill();
    }

    private IEnumerator BotMove2zero()
    {
        while (BallManager.Instance.Balls.Count == 0)
            yield return null;
        
        while (true)
        {
            do
            {
                m_closestBalls = BallManager.Instance.FindClosestBalls(m_ballDetector, 3);
                yield return null;
            } while (m_closestBalls.Count == 0);

            bool isGoalHitted = false;

            foreach (var ball in m_closestBalls)
            {
                /*isGoalHitted = Physics.Raycast(ball.transform.position,
                ball.GetComponent<Rigidbody>().velocity.normalized,
                out m_hit, m_distanceToReact, LayerMask.GetMask("PlayerGoal"));*/
                isGoalHitted = Physics.SphereCast(ball.transform.position, BallManager.Instance.BallRadius,
                    ball.GetComponent<Rigidbody>().velocity.normalized, out m_hit, m_distanceToReact, LayerMask.GetMask("PlayerGoal"));

                if (isGoalHitted && m_hit.transform.Equals(m_goalPlayerTransform))
                    break;
            }

            if (isGoalHitted)
                break;

            yield return null;
        }

        m_tween = Shield.transform.DOLookAt(2 * Shield.position - m_hit.point, m_timeFinishMovePaddle, AxisConstraint.Y)
            .SetEase(Ease.InOutSine)
            .OnUpdate(() =>
            {
                if (Shield.localRotation.eulerAngles.y > m_maxPaddleAngle ||
                    Shield.localRotation.eulerAngles.y < -m_maxPaddleAngle)
                    m_tween.Kill();
                Shield.localRotation = Quaternion.Euler(Shield.localRotation.eulerAngles.x,
                    AngleUtils.ClampEulerAngle(Shield.localRotation.eulerAngles.y, -m_maxPaddleAngle, m_maxPaddleAngle),
                    Shield.localRotation.eulerAngles.z);
            })
            .OnKill(() => StartCoroutine(BotMove()))
            .SetAutoKill();
    }

    /*private void Update()
    {
        *//*if (Shield.localRotation.eulerAngles.y > m_maxPaddleAngle ||
                Shield.localRotation.eulerAngles.y < -m_maxPaddleAngle)
            m_tween.Kill();*//*

        Shield.localRotation = Quaternion.Euler(Shield.localRotation.eulerAngles.x,
            AngleUtils.ClampEulerAngle(Shield.localRotation.eulerAngles.y, -m_maxPaddleAngle, m_maxPaddleAngle),
            Shield.localRotation.eulerAngles.z);
    }*/

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        BotMove3zero();
    }


    private void BotMove3zero()
    {
        if (BallManager.Instance.Balls.Count == 0)
            return;

        m_closestBalls = BallManager.Instance.FindClosestBalls(m_ballDetector, 4);
        if (m_closestBalls.Count == 0)
            return;

        bool isGoalHitted = false;

        foreach (var ball in m_closestBalls)
        {
            isGoalHitted = Physics.SphereCast(ball.transform.position, BallManager.Instance.BallRadius,
                ball.GetComponent<Rigidbody>().velocity.normalized, out m_hit, MapManager.Instance.BorderRadiusConstant / 2,
                LayerMask.GetMask("PlayerGoal"));

            if (isGoalHitted && m_hit.transform.Equals(m_goalPlayerTransform))
                break;
        }

        if (isGoalHitted)
        {
            float angle = Mathf.LerpAngle(Shield.localRotation.eulerAngles.y,
                AngleUtils.ClampEulerAngle(-AngleUtils.FindAngleBetween3Points(BallSpawnPosition.position, Shield.position, m_hit.point),
                -m_maxPaddleAngle, m_maxPaddleAngle), Time.deltaTime * m_paddleSpeed);
            Shield.localRotation = Quaternion.Euler(Shield.localRotation.eulerAngles.x, angle, Shield.localRotation.eulerAngles.z);
        }
    }

        

}


using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BotController : Player
{
    [SerializeField, Range(0.1f, 50.0f)] private float m_paddleSpeed;
    [SerializeField, Range(1f, 50f)] private float m_distanceToReact;

    private GameObject m_closestBall;

    private void Start()
    {
        MovePaddle(); //TODO przenieœæ mo¿e do listener'a na zaczêcie gry czy coœ
    }
    
    private void MovePaddle()
    {
        if (!(bool)BallManager.Instance.Balls?.Any())
            return;

        m_closestBall = FindClosestBall(BallManager.Instance.Balls);

        if (m_closestBall == null || Vector3.Distance(m_closestBall.transform.position, transform.position) > m_distanceToReact)
            return;

        float rotateAngle = Mathf.Clamp(FindAngleBetween(transform, m_closestBall.transform), -m_maxPaddleAngle, m_maxPaddleAngle);
        transform.DORotateQuaternion(Quaternion.Euler(0, rotateAngle, 0), m_paddleSpeed)
            .SetSpeedBased()
            .SetEase(Ease.InOutBack)
            .OnComplete(MovePaddle)
            .SetAutoKill();

        /*transform.DOLookAt(m_closestBall.transform.position, m_paddleSpeed, AxisConstraint.Y)
            .SetSpeedBased()
            .SetEase(Ease.InOutBack)
            .OnComplete(MovePaddle)
            .SetAutoKill();*/
    }

    private float FindAngleBetween(Transform relativeObjectT, Transform objectT)
    {
        Vector3 relativePos = objectT.position - relativeObjectT.position;
        return Vector3.Angle(relativePos, relativeObjectT.forward);
    }

    private void CmdPaddleMove(float rotationY)
    {
        transform.Rotate(0, -rotationY * Time.deltaTime * m_paddleSpeed, 0);

        Vector3 newRotation = transform.localEulerAngles;
        Debug.Log(newRotation.y);
        if (newRotation.y > m_maxPaddleAngle && newRotation.y < 360 - m_maxPaddleAngle)
        {
            newRotation.y = rotationY > 0 ? 360 - m_maxPaddleAngle : m_maxPaddleAngle;
            transform.localEulerAngles = newRotation;
        }
    }

    private GameObject FindClosestBall(List<GameObject> balls)
    {
        int minBallPosIndex = 0;
        float minBallPosMagnitude = float.PositiveInfinity;

        for (int i = 0; i < balls.Count; ++i)
        {
            float minBallPosMagnitudeTemp = new Vector2(balls[i].transform.position.x - transform.position.x,
                balls[i].transform.position.z - transform.position.z).sqrMagnitude;
            if (minBallPosMagnitude > minBallPosMagnitudeTemp && transform.InverseTransformPoint(balls[i].transform.position).z > 0.0f)
            {
                minBallPosMagnitude = minBallPosMagnitudeTemp;
                minBallPosIndex = i;
            }
        }
        return balls[minBallPosIndex];
    }
}



               
    private void Start()
    {
        //StartCoroutine(BotMove()); //TODO zmienić na wykonanie na event rozpoczęcia gry
    }


        /*float angle = AngleUtils.FindAngleBetween3Points(BallSpawnPosition.position, Shield.position, m_hit.point);
        Shield.DOLocalRotate(new Vector3(0, -angle, 0), m_timeFinishMovePaddle, RotateMode.Fast)
            .SetEase(Ease.InExpo)
            .OnComplete(() => StartCoroutine(BotMove()))
            .SetAutoKill();*/

                private void Update()
    {
        if (Shield.transform.localRotation.eulerAngles.y > m_maxPaddleAngle)
            Shield.transform.localRotation = new Quaternion(Shield.transform.localRotation.x, Mathf.Sin(m_maxPaddleAngle / 2),
                Shield.transform.localRotation.z, Shield.transform.localRotation.w);
        if (Shield.transform.localRotation.eulerAngles.y < -m_maxPaddleAngle)
            Shield.transform.localRotation = new Quaternion(Shield.transform.localRotation.x, Mathf.Sin(-m_maxPaddleAngle / 2),
                Shield.transform.localRotation.z, Shield.transform.localRotation.w);
    }

    /*float shieldWidth = Shield.GetComponentInChildren<MeshRenderer>().bounds.size.x;
        float goalWidth = GoalPlayerTransform.GetComponent<MeshRenderer>().bounds.size.x;
        Vector3 hitPoint = new Vector3(Mathf.Clamp(m_hit.point.x, -goalWidth / 2 + shieldWidth / 2,
            goalWidth / 2 - shieldWidth / 2), m_hit.point.y, m_hit.point.z);*/

    public static float FindAngleBetween3Points(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        return Mathf.Atan2(Vector3.Dot(p2, Vector3.Cross(p0, p1)), Vector3.Dot(p0, p1)) * Mathf.Rad2Deg;
    }


    /*    public void GameStartedInvoke()
    {
        OnGameStarted.Invoke();
    }*/