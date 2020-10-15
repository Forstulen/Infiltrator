using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SearchPlayerScript))]
[RequireComponent(typeof(PatrolScript))]
public class AttackScript : AttackBaseScript {

    // Public Variables
    public enum State
    {
        ATTACK = 0,
        WANDER,
        RESUME,
        ALERT,
        NONE
    };

    public enum Damage
    {
        LIGHT = 0,
        MODERATE,
        DEADLY
    }

    public Damage DamageAttack = Damage.LIGHT;
    public float SmoothRotation = 5.0f;

    // Private Variables
    private SearchPlayerScript _searchScript;
    private UnityEngine.AI.NavMeshAgent _navMeshAgent;
    private PatrolScript _patrolAgent;
    private State _agentState;
    private GameObject _player;
    private FixedCameraScript _cameraScript;

    private int _wanderingNumber = 0;
    private int _wanderingMinNumber = 2;
    private int _wanderingMaxNumber = 5;
    private float _wanderingTime = 0;
    private float _wanderingMinTime = 4.0f;
    private float _wanderingMaxTime = 8.0f;
    private float _recoveryDelay = 1.5f;
    private float _recoveryTime = 0.0f;
    private float _navMeshAgentAttackSpeed = 3.0f;
    private float _navMeshAgentWalkSpeed = 1.0f;


	// Use this for initialization
	void Start () {
        this._agentState = State.NONE;
        this._searchScript = this.gameObject.GetComponent<SearchPlayerScript>();
        this._navMeshAgent = this.gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
        this._patrolAgent = this.gameObject.GetComponent<PatrolScript>();
        this._player = GameObject.Find("Player");
        this._cameraScript = Camera.main.GetComponent<FixedCameraScript>();
        this._searchScript.PlayerFound += this.Attack;
        this._searchScript.PlayerAlmostFound += this.Research;
        this._searchScript.PlayerNotFound += this.Wander;
	}
	
	// Update is called once per frame
	void Update () {
        switch (this._agentState)
        {
            case State.ATTACK:
                this.Attack();
                break;
            case State.WANDER:
                this.Wander();
                break;
            case State.RESUME:
                break;
            default:
                break;
        }
	}

    void SmoothLookAt()
    {
        Vector3 relPlayerPosition = this._player.transform.position - this.gameObject.transform.position;
        relPlayerPosition.y = this.gameObject.transform.position.y;
        Quaternion lookAtRotation = Quaternion.LookRotation(relPlayerPosition, Vector3.up);

        transform.rotation = Quaternion.Lerp(this.gameObject.transform.rotation, lookAtRotation, this.SmoothRotation * Time.deltaTime);
    }

    private void Attack()
    {
        this.SmoothLookAt();
        this._recoveryTime += Time.deltaTime;
        if (Vector3.Distance(this._player.transform.position, this.gameObject.transform.position) <= 2.0f && 
                this._recoveryTime >= this._recoveryDelay) {
            this._recoveryTime = 0;
            this.IsAttacking = true;
            this._cameraScript.ActivateShaking();
        }
        IsWalking = true;
        IsRunning = true;
        this._patrolAgent.Pursuit();
        this._navMeshAgent.SetDestination(this._searchScript.LastKnownPosition);
    }

    private void Wander()
    {
        this._wanderingTime += Time.deltaTime;
        if (this.gameObject.transform.position.x != this._searchScript.LastKnownPosition.x &&
            this.gameObject.transform.position.z != this._searchScript.LastKnownPosition.z) {
            this.Attack();
        } else if (this._wanderingNumber >= Random.Range(this._wanderingMinNumber, this._wanderingMaxNumber)) {
            this._agentState = State.RESUME;
            this._patrolAgent.ResumePath();
            this._wanderingNumber = 0;
        } else if (this._wanderingTime >= Random.Range(this._wanderingMinTime, this._wanderingMaxTime)) {
            this._patrolAgent.RandomLookAround();
            ++this._wanderingNumber;
            this._wanderingTime = 0;
        }
    }

    private void Alert()
    {
        this.IsWalking = false;
        this._wanderingTime += Time.deltaTime;
        if (this._wanderingNumber >= 3)
        {
            this._agentState = AttackScript.State.RESUME;
            this._patrolAgent.ResumePath();
            this._wanderingNumber = 0;
        }
        else if (this._wanderingTime >= 1)
        {
            this._patrolAgent.LookAround(120);
            ++this._wanderingNumber;
            this._wanderingTime = 0;
        }
    }

    private void Attack(SearchPlayerScript searchPlayer, System.EventArgs arg)
    {
        this.AttackTag();
        this._navMeshAgent.speed = this._navMeshAgentAttackSpeed;
        this._agentState = State.ATTACK;
        this.IsAttacking = true;
    }

    private void Wander(SearchPlayerScript searchPlayer, System.EventArgs arg)
    {
        this.IsAttacking = false;
        this._navMeshAgent.speed = this._navMeshAgentWalkSpeed;
        if (this._agentState == State.ATTACK || this._agentState == State.WANDER)
        {
            this.AlertTag();
            this._agentState = State.WANDER;
        }
        else if (this._agentState != AttackScript.State.ALERT)
        {
            this.NonAttackTag();
            this._agentState = State.RESUME;
        }
    }

    private void Research(SearchPlayerScript searchPlayer, System.EventArgs arg)
    {
        this.IsAttacking = false;
        this._navMeshAgent.speed = this._navMeshAgentWalkSpeed;
        this.AlertTag();
        this._patrolAgent.Alert();
        this._agentState = AttackScript.State.WANDER;
    }
}
