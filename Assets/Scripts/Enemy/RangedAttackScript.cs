using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SearchPlayerScript))]
[RequireComponent(typeof(PatrolScript))]
public class RangedAttackScript : AttackBaseScript {


    // Public Variables
    public GameObject Bullet;
    public bool Static = true;
    public float MaxSpread = 10.0f;
    public float MaxSpreadDistance = 30.0f;
    public float Speed = 10000.0f;
    public float Delay = 0.5f;
    public float SmoothRotation = 5.0f;
    public float MaxShootingRange = 15.0f;

    // Private Variables

    private AttackScript.State _agentState;
    private SearchPlayerScript _searchScript;
    private UnityEngine.AI.NavMeshAgent _navMeshAgent;
    private PatrolScript _patrolAgent;
    private GameObject _player;
    private Transform _canon;
    private float _spread;

    private int _wanderingNumber = 0;
    private int _wanderingMinNumber = 4;
    private int _wanderingMaxNumber = 8;
    private float _wanderingTime = 0;
    private float _wanderingMinTime = 2.0f;
    private float _wanderingMaxTime = 3.0f;
    private float _recoveryDelay = 1.5f;
    private float _recoveryTime = 0.0f;
    private float _navMeshAgentRunningSpeed = 5.0f;
    private float _navMeshAgentWalkSpeed = 1.5f;

    private float _lostSightTimer = 0.0f;
    private float _lostSightMaxTime = 1.5f;


    // Use this for initialization
    void Start()
    {
        _agentState = AttackScript.State.NONE;
        _searchScript = gameObject.GetComponent<SearchPlayerScript>();
        _navMeshAgent = gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
        _patrolAgent = gameObject.GetComponent<PatrolScript>();
        _navMeshAgent.speed = _navMeshAgentWalkSpeed;
        _searchScript.PlayerFound += Attack;
        _searchScript.PlayerAlmostFound += Research;
        _searchScript.PlayerNotFound += Wander;
        _searchScript.PlayerHeard += PlayerHeard; ;
        _player = GameObject.Find("Player");
        _canon = gameObject.transform.Find("BulletSpawner");
    }

    private void PlayerHeard(SearchPlayerScript script, System.EventArgs e) {
        _patrolAgent.Pursuit();
        _agentState = AttackScript.State.WANDER;
    }

    // Update is called once per frame
    void Update()
    {
        switch (_agentState)
        {
            case AttackScript.State.ATTACK:
                Attack();
                break;
            case AttackScript.State.WANDER:
                Wander();
                break;
            case AttackScript.State.RESUME:
                break;
            case AttackScript.State.ALERT:
                Alert();
                break;
            default:
                break;
        }
    }

    void SmoothLookAt()
    {
        Vector3 relPlayerPosition = _player.transform.position - gameObject.transform.position;
        relPlayerPosition.y = gameObject.transform.position.y;
        Quaternion lookAtRotation = Quaternion.LookRotation(relPlayerPosition, Vector3.up);

        transform.rotation = Quaternion.Lerp(gameObject.transform.rotation, lookAtRotation, SmoothRotation * Time.deltaTime);
    }

    private void Attack()
    {
        iTween.Stop(gameObject);
        _patrolAgent.Pursuit();
        _recoveryTime += Time.deltaTime;
        if (_recoveryTime >= Delay && Vector3.Distance(gameObject.transform.position, _player.transform.position) <= MaxShootingRange)
        {
            GameObject bullet;
            //BulletScript bulletScript;

            _spread = MaxSpread * (Vector3.Distance(gameObject.transform.position, _player.transform.position) / MaxSpreadDistance);

            //float randomNumberX = Random.Range(-_spread, _spread);
            float randomNumberY = Random.Range(-_spread, _spread);
            //float randomNumberZ = Random.Range(-_spread, _spread);
            _recoveryTime = 0;
            if (_canon == null)
            {
                bullet = Instantiate(Bullet, gameObject.transform.position, gameObject.transform.rotation) as GameObject;
            }
            else
            {
                bullet = Instantiate(Bullet, _canon.position, gameObject.transform.rotation) as GameObject;
            }

            bullet.transform.LookAt(_player.transform.position);
            bullet.transform.Rotate(0, randomNumberY, 0);
            bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * Speed);
        }
        else if (Vector3.Distance(gameObject.transform.position, _player.transform.position) > MaxShootingRange && !Static)
        {
            IsWalking = true;
            IsRunning = true;
            _navMeshAgent.isStopped = false;
            _navMeshAgent.SetDestination(gameObject.transform.position + (_player.transform.position - gameObject.transform.position).normalized);
        }
        else
        {
            _navMeshAgent.isStopped = true;
            IsWalking = false;
            IsRunning = false;
        }
    }

    private void Wander()
    {
        IsWalking = false;
        IsRunning = false;
        _navMeshAgent.speed = _navMeshAgentWalkSpeed;
        if (gameObject.transform.position.x != _searchScript.LastKnownPosition.x &&
            gameObject.transform.position.z != _searchScript.LastKnownPosition.z && !Static)
        {
            if (_navMeshAgent.velocity.magnitude > 0.1f)
                IsWalking = true;
            IsRunning = false;
            _navMeshAgent.isStopped = false;
            _navMeshAgent.SetDestination(_searchScript.LastKnownPosition);
        }
        else if (_wanderingNumber >= Random.Range(_wanderingMinNumber, _wanderingMaxNumber))
        {
            _agentState = AttackScript.State.RESUME;
            _patrolAgent.ResumePath();
            _wanderingNumber = 0;
        }
        else
        {   
            if (_patrolAgent.RandomLookAround())
                ++_wanderingNumber;
        }
    }

    private void Alert()
    {
        IsWalking = false;
        if (_wanderingNumber >= 3)
        {
            _agentState = AttackScript.State.RESUME;
            _patrolAgent.ResumePath();
            _wanderingNumber = 0;
        }
        else 
        {
            if (_patrolAgent.LookAround(120))
                ++_wanderingNumber;
        }
    }

    private void Attack(SearchPlayerScript searchPlayer, System.EventArgs arg)
    {
        AttackTag();
        SmoothLookAt();
        IsAttacking = true;
        _navMeshAgent.speed = _navMeshAgentRunningSpeed;
        _agentState = AttackScript.State.ATTACK;
        _lostSightTimer = 0.0f;
    }

    private void Wander(SearchPlayerScript searchPlayer, System.EventArgs arg)
    {
        if (_lostSightTimer >= _lostSightMaxTime)
        {
            _lostSightTimer = 0.0f;
            IsAttacking = false;
            _navMeshAgent.speed = _navMeshAgentWalkSpeed;
            if (_agentState == AttackScript.State.ATTACK || _agentState == AttackScript.State.WANDER)
            {
                AlertTag();
                _agentState = AttackScript.State.WANDER;
            }
            else if (_agentState != AttackScript.State.ALERT)
            {
                NonAttackTag();
                _agentState = AttackScript.State.RESUME;
            }
        }
        _lostSightTimer += Time.deltaTime;
    }

    private void Research(SearchPlayerScript searchPlayer, System.EventArgs arg)
    {
        IsAttacking = false;
        _navMeshAgent.speed = _navMeshAgentWalkSpeed;
        AlertTag();
        _patrolAgent.Alert();
        _agentState = AttackScript.State.ALERT;
        _lostSightTimer = 0.0f;
    }


}

