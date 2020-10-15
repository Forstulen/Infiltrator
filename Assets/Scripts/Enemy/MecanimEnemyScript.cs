using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AttackBaseScript))]
[RequireComponent(typeof(Animator))]
public class MecanimEnemyScript : MonoBehaviour {


    // Public Variables
    [System.NonSerialized]
    public float LookWeight;
    public float LookSmoother = 5.0f;
    public float AnimationSpeed = 1.0f;
    public float MaxWalkingSpeed = 1.0f;

    // Private Variables
    private Animator _animatorScript;
    private AttackBaseScript    _attackScript;
    private AnimatorStateInfo _currentBaseState;
    private float _direction;
    private Vector3 _lookDirection;
    private float _currentAnimationSpeed;
    private float _catchObjectAnimationSpeed = 0.5f;
    private float _standingAimAnimationSpeed = 0.5f;

    static int idleState = Animator.StringToHash("Base Layer.Idle");
    static int standingAimState = Animator.StringToHash("Base Layer.StandingAim");
    static int walkState = Animator.StringToHash("Base Layer.Walk");
    static int strafeWalkLeftState = Animator.StringToHash("Base Layer.StrafeWalkLeft");
    static int strafeWalkRightState = Animator.StringToHash("Base Layer.StrafeWalkRight");
    static int aimState = Animator.StringToHash("Base Layer.WalkAim");
    static int runState = Animator.StringToHash("Base Layer.Run");
    static int catchState = Animator.StringToHash("Base Layer.Crouch");
    static int crouchWalkState = Animator.StringToHash("Base Layer.CrouchWalk");

    // Use this for initialization
    void Start()
    {
        this._animatorScript = this.gameObject.GetComponent<Animator>();
        this._attackScript = this.gameObject.GetComponent<AttackBaseScript>();
        this._currentAnimationSpeed = this.AnimationSpeed;
    }

    void FixedUpdate()
    {
        this._animatorScript.speed = this._currentAnimationSpeed;
        this._currentBaseState = this._animatorScript.GetCurrentAnimatorStateInfo(0);

        if (this._attackScript.IsWandering)
        {
            this._animatorScript.SetBool("Walk", false);
            this._animatorScript.SetBool("Run", false);
            this._animatorScript.SetBool("Aim", false);
        }
        else
        {
            this._animatorScript.SetBool("Walk", this._attackScript.IsWalking);
            this._animatorScript.SetBool("Run", this._attackScript.IsRunning);
            this._animatorScript.SetBool("Aim", this._attackScript.IsAttacking);
        }
    }
}
