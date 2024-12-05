using UnityEngine;

public class PlatformMover : MonoBehaviour
{
    public Vector3 localTargetPosition;
    public float moveDuration = 2f;
    public float waitDuration = 1f;
    public bool loop = true;

    private Vector3 _startPosition;
    private Vector3 _globalTargetPosition;
    private float _tweenTime;
    private bool _isMovingToTarget;
    private bool _isWaiting;
    private float _waitTimer;

    private void Start()
    {
        _startPosition = transform.position;
        _globalTargetPosition = transform.TransformPoint(localTargetPosition);
        _isMovingToTarget = true;
        _isWaiting = false;
        _tweenTime = 0;
        _waitTimer = 0;
    }

    private void FixedUpdate()
    {
        if (_isWaiting)
        {
            _waitTimer += Time.fixedDeltaTime;

            if (_waitTimer >= waitDuration)
            {
                _waitTimer = 0;
                _isWaiting = false;
            }
        }
        else if (loop || (!_isMovingToTarget && _tweenTime < 1f) || (_isMovingToTarget && _tweenTime > 0f))
        {
            _tweenTime += (_isMovingToTarget ? 1 : -1) * Time.fixedDeltaTime / moveDuration;

            if (_tweenTime >= 1f || _tweenTime <= 0f)
            {
                _tweenTime = Mathf.Clamp01(_tweenTime);
                _isMovingToTarget = !_isMovingToTarget;
                _isWaiting = true;
            }

            transform.position = Vector3.Lerp(_startPosition, _globalTargetPosition, Mathf.SmoothStep(0f, 1f, _tweenTime));
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.TransformPoint(localTargetPosition), 0.25f);
        Gizmos.DrawLine(transform.position, transform.TransformPoint(localTargetPosition));
    }
#endif
}
