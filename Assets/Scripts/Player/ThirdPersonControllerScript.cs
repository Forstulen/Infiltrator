using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonControllerScript : MonoBehaviour {

    // Public Variables
    public float WalkingSpeed = 2.0f;
    public float RunningSpeed = 5.0f;
    public float SlidingSpeed = 2.0f;
    public float RotationSpeed = 20.0f;
    public float RadiusCoeff = 1.5f;

    public AudioClip RunningSound;
    public AudioClip WalkingSound;
    public AudioClip SlidingSound;
    public AudioClip KnockSound;

    static float MaxAngle = 5.0f;

    public enum CharacterState
    {
        IDLE = 0,
        WALKING,
        RUNNING,
        SLIDING,
        INTERACTING
    }

    public enum SlidingState
    {
        HORIZONTAL = 0,
        VERTICAL
    };

    // Private Variables
    private CharacterController _characterController;
    private CharacterState  _characterState;
    private CharacterState _lastCharacterState;
    private Vector3 _forwardDirection;
    private Vector3 _rightDirection;
    private float   _movingSpeed;
    private float _lastSlidingDirection;
    private SlidingState _lastSlidingState;
    private AudioSource _currentSource;

    private bool _isSliding = false;
    private bool _isInteracting = false;
    private Vector3 _wallSlideNormal = Vector3.zero;
    private Vector3 _closestPoint = Vector3.zero;
    private Collider _wallCollider;

    private float _interactingTime = 1f;
    private float _interactingTimer = 0.0f;

     // Events
    public event SlidingEvent HorizontalSlide;
    public event SlidingEvent VerticalSlide;
    public event SlidingEvent IdleSlide;
    public event SlidingEvent StartSliding;
    public event SlidingEvent StopSliding;
    public event SlidingEvent UpdateWallNormal;
    public delegate void SlidingEvent(ThirdPersonControllerScript script, SlidingArgs arg);

    public event InteractingEvent CatchObject;
    public event InteractingEvent KnockWall;
    public event InteractingEvent OpenDoor;
    public delegate void InteractingEvent(ThirdPersonControllerScript script, InteractingArgs e);


    public event NoiseEvent Running;
    public event NoiseEvent Knock;
    public delegate void NoiseEvent();


    public class SlidingArgs : System.EventArgs
    {
        public float directionValue { get; set; }
        public SlidingState direction { get; set; }
        public Vector3 normal { get; set; }
        public Vector3 wallNormal { get; set; }
    }

    public class InteractingArgs : System.EventArgs
    {
        public int ID { get; set; }
    }

    void Awake()
    {
        _characterState = CharacterState.IDLE;
    }

	// Use this for initialization
	void Start () {
        _characterController = gameObject.GetComponent<CharacterController>();
        _forwardDirection = gameObject.transform.forward;
        _rightDirection = gameObject.transform.right;
	}

    bool IsMoving()
    {
        return (Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f || Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f);
    }

    void SetCharacterState()
    {
        if (Input.GetButtonDown("Jump")) {
            _isSliding = !_isSliding;

            if (_isSliding && Slide())
            {
                if (StartSliding != null) StartSliding(this, null);
            }
            else
            {
                if (StopSliding != null) StopSliding(this, null);
            }
        }

        _lastCharacterState = _characterState;
        if (_isSliding)
        {
            if (Input.GetButtonDown("Fire1")) {
                KnockWall?.Invoke(this, null);
            }
            _movingSpeed = WalkingSpeed;
            _characterState = CharacterState.SLIDING;
            if (_lastCharacterState != _characterState)
            {
                AudioManagerScript.Instance.StopSound(_currentSource);
                _currentSource = AudioManagerScript.Instance.PlayLoop(SlidingSound, gameObject.transform, .05f);
            }
            Slide();
        }
        else if (Input.GetButtonDown("Fire2"))
        {
            _movingSpeed = 0;
            _characterState = CharacterState.INTERACTING;
            AudioManagerScript.Instance.StopSound(_currentSource);
        }
        else if (Input.GetKey(KeyCode.LeftShift) | Input.GetKey(KeyCode.RightShift))
        {
            _movingSpeed = RunningSpeed;
            _characterState = CharacterState.RUNNING;
            if (_lastCharacterState != _characterState)
            {
                AudioManagerScript.Instance.StopSound(_currentSource);
                _currentSource = AudioManagerScript.Instance.PlayLoop(RunningSound, gameObject.transform, .1f);
            }

            Running?.Invoke();
        }
        else if (IsMoving())
        {
            _movingSpeed = WalkingSpeed;
            _characterState = CharacterState.WALKING;
            if (_lastCharacterState != _characterState)
            {
                AudioManagerScript.Instance.StopSound(_currentSource);
                _currentSource = AudioManagerScript.Instance.PlayLoop(WalkingSound, gameObject.transform, .1f);
            }
        }
        else
        {
            _movingSpeed = 0;
            AudioManagerScript.Instance.StopSound(_currentSource);
            _characterState = CharacterState.IDLE;
        }
    }

    SlidingArgs SetArgs(float direction)
    {
        SlidingArgs args = new SlidingArgs();

        args.directionValue = direction;
        args.direction = _lastSlidingState;
        args.normal = _closestPoint;
        args.wallNormal = _wallSlideNormal;

        return args;
    }

    bool BlockOnWall(float positionAxis, float min, float max, ref float value)
    {
        if ((positionAxis - _characterController.radius) <= min && value < 0)
        {
            value = 0;
            return true;
        }
        else if ((positionAxis + _characterController.radius) >= max && value > 0)
        {
            value = 0;
            return true;
        }
        else
        {
            return false;
        }
    }

    void Interact()
    {
        if (_characterState == CharacterState.INTERACTING)
        {
            Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, _characterController.radius * RadiusCoeff);

            for (int i = 0; i < hitColliders.Length; ++i)
            {
                if (hitColliders[i].gameObject.tag == "Object")
                {
                    InteractingArgs args = new InteractingArgs();

                    args.ID = hitColliders[i].gameObject.GetInstanceID();
                    _isInteracting = true;

                    if (CatchObject != null) CatchObject(this, args);
                }
                else if (hitColliders[i].gameObject.tag == "Interact")
                {
                    InteractingArgs args = new InteractingArgs();

                    args.ID = hitColliders[i].gameObject.GetInstanceID();
                    _isInteracting = true;
                    if (OpenDoor != null) OpenDoor(this, args);
                }
            }
        }
    }

    void Move()
    {
        float vertical = Input.GetAxisRaw("Vertical");
        float horizontal = Input.GetAxisRaw("Horizontal");

        if (_characterState == CharacterState.SLIDING && _wallCollider)
        {
            float forwardAngle = Vector3.Angle(_forwardDirection, _wallSlideNormal);
            float rightAngle = Vector3.Angle(_rightDirection, _wallSlideNormal);

            Vector3 minBounds = _wallCollider.bounds.min;
            Vector3 maxBounds = _wallCollider.bounds.max;

            // Block character movements during sliding action
            if (forwardAngle == 180 || forwardAngle == 0)
            {
                vertical = 0;
                if (forwardAngle == 0)
                {
                    horizontal = -horizontal;
                }
                _lastSlidingState = SlidingState.HORIZONTAL;
                if (!BlockOnWall(gameObject.transform.position.x, minBounds.x, maxBounds.x, ref horizontal) && HorizontalSlide != null)
                {
                    if (forwardAngle == 0)
                    {
                        _lastSlidingDirection = horizontal;
                        HorizontalSlide(this, SetArgs(-horizontal));
                    }
                    else
                    {
                        _lastSlidingDirection = horizontal;
                        HorizontalSlide(this, SetArgs(horizontal));
                    }

                }
                else if (IdleSlide != null)
                {
                    IdleSlide(this, null);
                }
            }
            else
            {
                // Allow the player to use left/right key to move even along a vertical collider
                if (rightAngle == 180)
                {
                    _lastSlidingDirection = horizontal;
                    vertical = -horizontal;
                } else if (rightAngle == 0) {
                    _lastSlidingDirection = -horizontal;
                    vertical = horizontal;
                }

                _lastSlidingState = SlidingState.VERTICAL;
                if (!BlockOnWall(gameObject.transform.position.z, minBounds.z, maxBounds.z, ref vertical) && VerticalSlide != null)
                    VerticalSlide(this, SetArgs(horizontal));
                else if (IdleSlide != null)
                {
                    IdleSlide(this, null);
                }
                horizontal = 0;
            }
        }

        Vector3 direction = (vertical * _forwardDirection + horizontal * _rightDirection).normalized;

        if (IsMoving())
        {
            Vector3 movement = (direction * _movingSpeed) * Time.deltaTime;

            _characterController.Move(movement);
            if (!_isSliding) {
                gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, Quaternion.LookRotation(movement), Time.deltaTime * RotationSpeed);
            }
        }
        else
        {
            _lastSlidingDirection = 0;
            if (IdleSlide != null) IdleSlide(this, null);
        }
    }

    bool Slide()
    {
            Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, _characterController.radius * RadiusCoeff);

            for (int i = 0; i < hitColliders.Length; ++i)
            {
                if (hitColliders[i].gameObject.tag == "Wall")
                {
                    _wallCollider = hitColliders[i];
                    _closestPoint = hitColliders[i].ClosestPointOnBounds(gameObject.transform.position);
                    // Don't slide when you're too close from the edges
                    if ((_closestPoint.x > (_wallCollider.bounds.max.x - _characterController.radius / 2.0f) || _closestPoint.x < (_wallCollider.bounds.min.x + _characterController.radius / 2.0f)) &&
                        (_closestPoint.z > (_wallCollider.bounds.max.z - _characterController.radius / 2.0f) || _closestPoint.z < (_wallCollider.bounds.min.z + _characterController.radius / 2.0f)))
                    {
                        CancelSliding();
                        return false;
                    }

                    // Don't slide on the forward face
                    //if (Vector3.Angle(_closestPoint - (Vector3.forward + gameObject.transform.position), _closestPoint - gameObject.transform.position) < MaxAngle)
                    //{
                    //    CancelSliding();
                    //    return false;
                    //}

                    _wallSlideNormal = (gameObject.transform.position - _closestPoint).normalized;
                    if (UpdateWallNormal != null) UpdateWallNormal(this, SetArgs(_lastSlidingDirection));
                    gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, Quaternion.LookRotation(_wallSlideNormal), Time.deltaTime * RotationSpeed);
                    return true;
                }
            }
            CancelSliding();
            return false;
    }

    void CancelSliding()
    {
        _wallCollider = null;
        _wallSlideNormal = Vector3.zero;
        _isSliding = !_isSliding;
        if (StopSliding != null) StopSliding(this, null);
    }

    bool Interacting()
    {
        if (_isInteracting && _interactingTimer < _interactingTime)
        {
            _interactingTimer += Time.deltaTime;
            return true;
        }
        else
        {
            _isInteracting = false;
            _interactingTimer = 0.0f;
            return false;
        }
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        if (!Interacting())
        {
            SetCharacterState();
            Move();
            Interact();
        }
	}

    public void PlayKnockSound() {
        Knock?.Invoke();
        AudioManagerScript.Instance.Play(KnockSound, gameObject.transform, .25f);
    }

}
