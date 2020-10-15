using UnityEngine;
using System.Collections;

public class FixedCameraScript : MonoBehaviour {

    // Public Variables
    public float    SmoothTranslate = 1.5f;
    public float    SmoothRotation = 1.0f;
    public float    SmoothTranslateSlide = 5.0f;
    public float    SmoothRotationSlide = 2.0f;
    public float    BackWardDistance = 10.0f;
    public float    UpWardDistance = 10.0f;
    public float    BackWardWallSlideDistance = 7.0f;
    public float    UpWardWallSlideDistance = 4.0f;
    public float    ShakeIntensity = 10.0f;
    public float    ShakeDuration = 1.0f;

    // Private Variables
    private GameObject  _player;
    private Vector3     _offset;
    private Vector3     _cameraPosition;
    private Vector3     _cameraSight;
    private GameObject  _lastOccluder;
    private bool _isSliding;
    private bool _isJittering;
    private Vector3 _closestPoint;
    private float _lastSlidingDirection;
    private ThirdPersonControllerScript.SlidingState _lastSlidingState;
    private ThirdPersonControllerScript _thirdPersonControllerScript;

    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private float _shakeTimer = 0.0f;
    private float _shakeIntensity = 0.0f;


    void Start() {
        this._player = GameObject.Find("Player");
        this._offset = new Vector3(0, this.UpWardDistance, -this.BackWardDistance);
        this._thirdPersonControllerScript = this._player.GetComponentInChildren<ThirdPersonControllerScript>();
        this._thirdPersonControllerScript.StartSliding += this.StartSliding;
        this._thirdPersonControllerScript.StopSliding += this.StopSliding;
        this._thirdPersonControllerScript.UpdateWallNormal += this.UpdateWallNormal;
    }

    private void UpdateWallNormal(ThirdPersonControllerScript thirdPlayer, ThirdPersonControllerScript.SlidingArgs args)
    {
        this._lastSlidingState = args.direction;
        this._lastSlidingDirection = args.directionValue;
        this._closestPoint = args.normal;
    }

    private void StartSliding(ThirdPersonControllerScript thirdPlayer, ThirdPersonControllerScript.SlidingArgs args)
    {
        this._isSliding = true;
    }

    private void StopSliding(ThirdPersonControllerScript thirdPlayer, ThirdPersonControllerScript.SlidingArgs args)
    {
        this._isSliding = false;
    }

    public void ActivateSliding()
    {
        this._isSliding = true;
    }

    public void DeactivateSliding()
    {
        this._isSliding = false;
    }

    public void ActivateShaking() {
        this._originalPosition = this.gameObject.transform.position;
        this._originalRotation = this.gameObject.transform.rotation;
        this._isJittering = true;
        this._shakeTimer = 0.0f;
        this._shakeIntensity = this.ShakeIntensity;
    }

    public void DeactivateShaking()
    {
        this._isJittering = false;
    }

    void FixedUpdate() {
        if (_isSliding)
        {
            this.SlidingCamera();
        }
        else
        {
            this.FollowCamera();
        }
    }

    void SlidingCamera()
    {
        if (this._lastOccluder != null)
        {
            this._lastOccluder.GetComponentInChildren<TransparentObjectScript>().FadeOut();
            this._lastOccluder = null;
        }

        this._cameraPosition = ((this._player.transform.position - this._closestPoint) *
                                this.BackWardWallSlideDistance + this._player.transform.position) +
                                Vector3.up * this.UpWardWallSlideDistance;
        this._cameraSight = (this._player.transform.position + Vector3.up * this.UpWardWallSlideDistance) - this.gameObject.transform.position;

        if (this._lastSlidingState == ThirdPersonControllerScript.SlidingState.HORIZONTAL)
        {
            this._cameraPosition += (Vector3.left * -this._lastSlidingDirection);
            //this._cameraSight += (Vector3.left * -this._lastSlidingDirection);
        }
        else
        {
            this._cameraPosition += Vector3.forward * -this._lastSlidingDirection;
            //this._cameraSight += Vector3.forward * -this._lastSlidingDirection;
        }

        Quaternion lookAtRotation = Quaternion.LookRotation(this._cameraSight);

        this.gameObject.transform.position = Vector3.Lerp(this.gameObject.transform.position, this._cameraPosition, this.SmoothTranslateSlide * Time.deltaTime);
        this.gameObject.transform.rotation = Quaternion.Lerp(this.gameObject.transform.rotation, lookAtRotation, this.SmoothRotationSlide * Time.deltaTime);


        if (_isJittering)
        {
            this._originalPosition = this.gameObject.transform.position;
            this._originalRotation = this.gameObject.transform.rotation;
            this.Jittering();
        }
    }

    void FollowCamera()
    {
        Vector3 standardPos = this._player.transform.position + this._offset;
        Vector3 abovePos = this._player.transform.position + Vector3.up * this.UpWardDistance;

        Vector3[] checkPoints = new Vector3[6];

        checkPoints[0] = standardPos;
        checkPoints[1] = Vector3.Lerp(standardPos, abovePos, 0.25f);
        checkPoints[2] = Vector3.Lerp(standardPos, abovePos, 0.5f);
        checkPoints[3] = Vector3.Lerp(standardPos, abovePos, 0.7f);
        checkPoints[4] = Vector3.Lerp(standardPos, abovePos, 0.9f);
        checkPoints[5] = Vector3.Lerp(standardPos, abovePos, 1.2f);

        for (int i = 0; i < checkPoints.Length; i++)
        {
            if (ViewingPosCheck(checkPoints[i]))
                break;
            else
                this._cameraPosition = checkPoints[i];
        }
        this.gameObject.transform.position = Vector3.Lerp(this.gameObject.transform.position, this._cameraPosition, this.SmoothTranslate * Time.deltaTime);
        this.SmoothLookAt();
        
        if (_isJittering)
        {
            this._originalPosition = this.gameObject.transform.position;
            this._originalRotation = this.gameObject.transform.rotation;
            this.Jittering();
        }
    }

     bool ViewingPosCheck (Vector3 position)
    {
        RaycastHit hit;
        TransparentObjectScript transparentScript = null;
        if (Physics.Raycast(position, this._player.transform.position - position, out hit, this._offset.magnitude))
            if (hit.transform != this._player.transform && hit.collider.gameObject.tag != "Enemy")
            {
                if ((transparentScript = hit.collider.gameObject.GetComponentInChildren<TransparentObjectScript>()))
                {
                    this._lastOccluder = hit.collider.gameObject;
                    transparentScript.FadeIn();
                }
                return false;
            }
        if (this._lastOccluder != null  ) {
            this._lastOccluder.GetComponentInChildren<TransparentObjectScript>().FadeOut();
            this._lastOccluder = null;
        }
        this._cameraPosition = position;
        return true;
    }

     void Jittering()
     {
         this._shakeTimer += Time.deltaTime;
         if (this._shakeIntensity > 0)
         {
             this.gameObject.transform.position = this._originalPosition + Random.insideUnitSphere * this._shakeIntensity;
             this.gameObject.transform.rotation = new Quaternion(this._originalRotation.x + Random.Range(-this._shakeIntensity, this._shakeIntensity),
                                       this._originalRotation.y + Random.Range(-this._shakeIntensity, this._shakeIntensity),
                                       this._originalRotation.z + Random.Range(-this._shakeIntensity, this._shakeIntensity),
                                       this._originalRotation.w + Random.Range(-this._shakeIntensity, this._shakeIntensity));

             this._shakeIntensity = Mathf.Lerp(this._shakeIntensity, 0, Mathf.Clamp01(this._shakeTimer / this.ShakeDuration));
         }
         else
         {
            this._shakeTimer = 0;
            this._shakeIntensity = this.ShakeIntensity;
            this.gameObject.transform.position = this._originalPosition;
            this.gameObject.transform.rotation = this._originalRotation;
            this._isJittering = false;
         }
     }

     void SmoothLookAt()
     {
         Vector3 relPlayerPosition = this._player.transform.position - this.gameObject.transform.position;
         Quaternion lookAtRotation = Quaternion.LookRotation(relPlayerPosition, Vector3.up);

         transform.rotation = Quaternion.Lerp(this.gameObject.transform.rotation, lookAtRotation, this.SmoothRotation * Time.deltaTime);
     }
}