using UnityEngine;
using System.Collections;

public class PlaySoundScript : MonoBehaviour {

    // Public Variables
    public AudioClip Sound;
    public Transform Emitter;
    public float Volume = 1.0f;

    // Private Variables

    void Start()
    {
        AudioManagerScript.Instance.PlayLoop(this.Sound, this.Emitter, this.Volume);
    }
}
