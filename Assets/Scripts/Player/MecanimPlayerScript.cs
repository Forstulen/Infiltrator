using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class MecanimPlayerScript : MonoBehaviour {

    // Public Variables
    [System.NonSerialized]
    public float LookWeight;
    public float LookSmoother = 5.0f;	
    public float AnimationSpeed = 1.0f;				

    // Private Variables
    private Animator    _animatorScript;
    private ThirdPersonControllerScript _thirdPersonControllerScript;
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
    static int knockState = Animator.StringToHash("Base Layer.Knock");

    // Use this for initialization
    void Start () {
        this._animatorScript = this.gameObject.GetComponent<Animator>();
        this._currentAnimationSpeed = this.AnimationSpeed;
        this._thirdPersonControllerScript = this.gameObject.GetComponent<ThirdPersonControllerScript>();
        this._thirdPersonControllerScript.HorizontalSlide += this.HorizontalSlide;
        this._thirdPersonControllerScript.VerticalSlide += this.VerticalSlide;
        this._thirdPersonControllerScript.IdleSlide += this.IdleSlide;
        this._thirdPersonControllerScript.StartSliding += this.StartSliding;
        this._thirdPersonControllerScript.StopSliding += this.StopSliding;
        this._thirdPersonControllerScript.CatchObject += this.CatchObject;
        this._thirdPersonControllerScript.KnockWall += KnockWall;

    }

    private void HorizontalSlide(ThirdPersonControllerScript thirdPlayer, ThirdPersonControllerScript.SlidingArgs arg)
    {
        this._animatorScript.SetFloat("Direction", arg.directionValue);
        this._animatorScript.SetBool("Run", false);
        if (arg.direction > 0) {
            this._lookDirection = Vector3.left;
        } else {
            this._lookDirection = Vector3.right;
        }
    }

    private void VerticalSlide(ThirdPersonControllerScript thirdPlayer, ThirdPersonControllerScript.SlidingArgs arg)
    {
        this._animatorScript.SetFloat("Direction", arg.directionValue);
        this._animatorScript.SetBool("Run", false);
        if (-arg.directionValue > 0)
        {
            this._lookDirection = Vector3.forward;
        }
        else
        {
            this._lookDirection = Vector3.back;
        }
    }

    private void IdleSlide(ThirdPersonControllerScript thirdPlayer, ThirdPersonControllerScript.SlidingArgs arg)
    {
        this._animatorScript.SetFloat("Direction", 0);
        this._animatorScript.SetBool("Run", false);
    }

    private void StartSliding(ThirdPersonControllerScript thirdPlayer, ThirdPersonControllerScript.SlidingArgs arg)
    {
        this._animatorScript.SetBool("Slide", true);
    }

    private void StopSliding(ThirdPersonControllerScript thirdPlayer, ThirdPersonControllerScript.SlidingArgs arg)
    {
        this._animatorScript.SetBool("Slide", false);
    }

    private void CatchObject(ThirdPersonControllerScript thirdPlayer, System.EventArgs e)
    {
        this._animatorScript.SetBool("Catch", true);
    }

    private void KnockWall(ThirdPersonControllerScript script, ThirdPersonControllerScript.InteractingArgs e) {
        this._animatorScript.SetTrigger("Knock");
    }


    void FixedUpdate()
    {
        this._animatorScript.speed = this._currentAnimationSpeed;
        this._currentBaseState = this._animatorScript.GetCurrentAnimatorStateInfo(0);

        if (this._currentBaseState.nameHash == idleState) {
            //this._animatorScript.SetBool("Aim", Input.GetButton("Fire1"));
            this._animatorScript.SetBool("Run", Input.GetButton("Fire3"));
            this._animatorScript.SetBool("Walk", Mathf.Abs(Input.GetAxisRaw("Vertical")) + Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f);
        }
        else if (this._currentBaseState.nameHash == standingAimState)
        {
            this._animatorScript.SetBool("Run", Input.GetButton("Fire3"));
            //this._animatorScript.SetBool("Aim", Input.GetButton("Fire1"));
            this._animatorScript.SetBool("Walk", Mathf.Abs(Input.GetAxisRaw("Vertical")) + Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f);
        }
        else if (this._currentBaseState.nameHash == walkState)
        {
            this._animatorScript.SetBool("Run", Input.GetButton("Fire3"));
            //this._animatorScript.SetBool("Aim", Input.GetButton("Fire1"));
            this._animatorScript.SetBool("Walk", Mathf.Abs(Input.GetAxisRaw("Vertical")) + Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f);
        }
        else if (this._currentBaseState.nameHash == aimState)
        {
            this._animatorScript.SetBool("Run", Input.GetButton("Fire3"));
            //this._animatorScript.SetBool("Aim", Input.GetButton("Fire1"));
            this._animatorScript.SetBool("Walk", Mathf.Abs(Input.GetAxisRaw("Vertical")) + Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f);
        }
        else if (this._currentBaseState.nameHash == runState)
        {
            //this._animatorScript.SetBool("Aim", false);
            this._animatorScript.SetBool("Run", Input.GetButton("Fire3"));
            this._animatorScript.SetBool("Walk", Mathf.Abs(Input.GetAxisRaw("Vertical")) + Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f);
        }
        else if (this._currentBaseState.nameHash == catchState)
        {
            if (!this._animatorScript.IsInTransition(0))
            {
                this._animatorScript.SetBool("Catch", false);
            }
        } 
        // Only works with a humanoid rigged body, doesn't work with generic avatar
        else if (this._currentBaseState.nameHash == strafeWalkLeftState || this._currentBaseState.nameHash == strafeWalkRightState)
        {
            this._animatorScript.SetLookAtPosition(this._lookDirection);
            this._animatorScript.SetLookAtWeight(this.LookWeight);
            this.LookWeight = Mathf.Lerp(this.LookWeight, 1.0f, Time.deltaTime * this.LookSmoother);
            return;
        }
        this.LookWeight = Mathf.Lerp(this.LookWeight, 0.0f, Time.deltaTime * this.LookSmoother);
    }
}
