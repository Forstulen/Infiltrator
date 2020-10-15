using UnityEngine;
using System.Collections;

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
public class EnemySoundScript : MonoBehaviour {

    // Public Variables
    public AudioClip RotatingSound;
    public AudioClip WalkingSound;
    public AudioClip RunningSound;
    public AudioClip AttackingSound;
    public AudioClip IdleSound;
    public float Volume = 0.3f;

    // Private Variables
    private UnityEngine.AI.NavMeshAgent _navMeshAgent;
    private AudioSource _currentSource;

	// Use this for initialization
	void Start () {
        this._navMeshAgent = this.gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
        this._currentSource = null;
	}
	
	// Update is called once per frame
	void Update () {
        if (this._navMeshAgent.velocity.magnitude <= 0.1f)
        {
            if (this._currentSource == null || this._currentSource.clip != this.IdleSound)
            {
                AudioManagerScript.Instance.StopSound(this._currentSource);
                this._currentSource = AudioManagerScript.Instance.PlayLoop(this.IdleSound, this.gameObject.transform, this.Volume);
            }
        }
        else if (this._navMeshAgent.velocity.magnitude < 2f)
        {
            if (this._currentSource == null || this._currentSource.clip != this.WalkingSound)
            {
                AudioManagerScript.Instance.StopSound(this._currentSource);
                this._currentSource = AudioManagerScript.Instance.PlayLoop(this.WalkingSound, this.gameObject.transform, this.Volume);
            }
        }
        else if (this._navMeshAgent.velocity.magnitude >= 2f)
        {
            if (this._currentSource == null || this._currentSource.clip != this.RunningSound)
            {
                AudioManagerScript.Instance.StopSound(this._currentSource);
                this._currentSource = AudioManagerScript.Instance.PlayLoop(this.RunningSound, this.gameObject.transform, this.Volume);
            }
        }
	}
}
