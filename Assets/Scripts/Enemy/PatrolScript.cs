using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AttackBaseScript))]
[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
[RequireComponent(typeof(EnemySoundScript))]
public class PatrolScript : MonoBehaviour {

    // Public Variables
    public Transform[] WayPoints;
    public PatrolType PatrolBehavior;
    public float Delay = 3.0f;
    public float SpeedRotation = 2.0f;

    // Private Variables
    private int _waypointIndex = 0;
    private float _delay = 0;
    private UnityEngine.AI.NavMeshAgent _navMeshAgent;
    private EnemySoundScript _enemySound;
    private bool _isGoingBack = false;
    private PatrolType _savedBehavior;
    private AttackBaseScript _attackScript;
    private float _minAngle = 45;
    private float _maxAngle = 180;
    private Vector3 _savedPosition;
    private Quaternion _savedRotation;
    private float _waypointPrecision = 0.2f;

    public enum PatrolType
    {
        LOOP = 0,
        PINGPONG,
        STATIC,
        LOOKAROUND,
        PURSUIT,
        ALERT
    }

	// Use this for initialization
	void Start () {
        iTween.Init(gameObject);
        _navMeshAgent = gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
        _enemySound = gameObject.GetComponent<EnemySoundScript>();
        _attackScript = gameObject.GetComponent<AttackBaseScript>();
        _savedBehavior = PatrolBehavior;
        _savedPosition = new Vector3(gameObject.transform.position.x, 
                                            gameObject.transform.position.y, 
                                            gameObject.transform.position.z);
        _savedRotation = gameObject.transform.rotation;
	}

    private void Loop() {
        _navMeshAgent.isStopped = false;
        _navMeshAgent.SetDestination(WayPoints[_waypointIndex].position);
        _attackScript.IsWalking = true;
        if (_delay >= Delay){           
            ++_waypointIndex;
            _delay = 0;
        }
        else if (Mathf.Abs(gameObject.transform.position.x - WayPoints[_waypointIndex].position.x) <= _waypointPrecision &&
            Mathf.Abs(gameObject.transform.position.z - WayPoints[_waypointIndex].position.z) <= _waypointPrecision)
        {
            _delay += Time.deltaTime;
            _attackScript.IsWalking = false;
        }

        if (_waypointIndex == WayPoints.Length)
        {
            _waypointIndex = 0;
        }
    }

    private void PingPong() {
        _navMeshAgent.isStopped = false;
        _navMeshAgent.SetDestination(WayPoints[_waypointIndex].position);
        _attackScript.IsWalking = true;
        if (Mathf.Abs(gameObject.transform.position.x - WayPoints[_waypointIndex].position.x) <= _waypointPrecision &&
            Mathf.Abs(gameObject.transform.position.z - WayPoints[_waypointIndex].position.z) <= _waypointPrecision)
        {
            if (_waypointIndex == WayPoints.Length - 1)
            {
                _isGoingBack = true;
            }
            else if (_waypointIndex == 0)
            {
                _isGoingBack = false;
            }

            if (_delay >= Delay)
            {
                if (_isGoingBack)
                {
                    --_waypointIndex;
                }
                else
                {
                    ++_waypointIndex;
                }
                _delay = 0;
            }
            else
            {
                _attackScript.IsWalking = false;
                _delay += Time.deltaTime;
            }
        }
    }

    public bool LookAround(float angle) {
        _delay += Time.deltaTime;
        _attackScript.IsWalking = false;
        _attackScript.IsRunning = false;
        if (_delay <= SpeedRotation / 2)
        {
            _attackScript.IsWalking = true;
        }
        if (_delay >= Delay)
        {
            AudioManagerScript.Instance.Play(_enemySound.RotatingSound, gameObject.transform.position, 0.3f);
            LocalLookAround(angle);
            _delay = 0.0f;
            return true;
        }
        return false;
    }

    private void LocalLookAround(float angle)
    {
        iTween.RotateAdd(gameObject, iTween.Hash("y", angle,
                                                      "easetype", iTween.EaseType.easeInOutQuad,
                                                      "time", SpeedRotation));
    }

    public bool RandomLookAround() {
        return LookAround(Random.Range(_minAngle, _maxAngle) * (Random.value > 0.5f ? 1 : -1));
    }

    public void Pursuit() {
        PatrolBehavior = PatrolType.PURSUIT;
    }

    public void Alert()
    {
        _navMeshAgent.isStopped = true;
        PatrolBehavior = PatrolType.ALERT;
    }

    public void ResumePath() {
        float   minDistance = Mathf.Infinity;
        int     index = 0;

        foreach (Transform t in WayPoints) {
            if (Mathf.Abs(Vector3.Distance(t.position, gameObject.transform.position)) < minDistance) {
                minDistance = Mathf.Abs(Vector3.Distance(t.position, gameObject.transform.position));
                _waypointIndex = index;
            }
            ++index;
        }
        PatrolBehavior = _savedBehavior;
    }
	
	// Update is called once per frame
	void Update () {
        switch (PatrolBehavior)
        {
            case PatrolType.LOOP:
                Loop();
                break;
            case PatrolType.PINGPONG:
                PingPong();
                break;
            case PatrolType.LOOKAROUND:
                if (gameObject.transform.position.x != _savedPosition.x &&
                    gameObject.transform.position.z != _savedPosition.z) {
                        _attackScript.IsWalking = true;
                    _navMeshAgent.isStopped = false;
                    _navMeshAgent.SetDestination(_savedPosition);
                } else {
                    RandomLookAround();
                }
                break;
            case PatrolType.ALERT:
                break;
            case PatrolType.PURSUIT:
                break;
            case PatrolType.STATIC:
                if (Mathf.Abs(Vector3.Distance(gameObject.transform.position, _savedPosition)) > 0.25f)
                {
                    _attackScript.IsWalking = true;
                    _navMeshAgent.isStopped = false;
                    _navMeshAgent.SetDestination(_savedPosition);
                }
                else
                {
                    _attackScript.IsWalking = false;
                    gameObject.transform.rotation = Quaternion.Lerp(gameObject.transform.rotation, _savedRotation, 2.0f * Time.deltaTime);
                }
                break;
            default:
                break;
        }
	}
}
