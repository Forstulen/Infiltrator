using UnityEngine;
using System.Collections;

public class SwitchScript : MonoBehaviour {

    // Public Variables
    public GameObject Door;
    public AudioClip SwitchSound;

    // Private Variables
    private ThirdPersonControllerScript _thirdPersonControllerScript;
    private Animation _animationScript;
    private bool _isOpened = false;

	// Use this for initialization
	void Start () {
        this._thirdPersonControllerScript = GameObject.Find("Player").GetComponentInChildren<ThirdPersonControllerScript>();
        this._animationScript = this.Door.GetComponentInChildren<Animation>();
        this._thirdPersonControllerScript.OpenDoor += this.OpenDoor;
	}

    private void OpenDoor(ThirdPersonControllerScript script, ThirdPersonControllerScript.InteractingArgs args)
    {
        if (this.gameObject.GetInstanceID() == args.ID && !this._isOpened)
        {
            AudioManagerScript.Instance.Play(this.SwitchSound, this.gameObject.transform.position, 1.0f);
            this._animationScript.Play();
            this._isOpened = true;
        }
    }
}
