using UnityEngine;
using System.Collections;

public class PulseSoundScript : MonoBehaviour {

    // Public Variables
    public AudioClip PulseSound;

    // Private Variables
    private GameObject _player;

    void Start()
    {
        this._player = GameObject.Find("Player");
    }

    public void PlayPulseSound()
    {
        AudioManagerScript.Instance.Play(this.PulseSound, this._player.transform.position, .05f);
    }
}
