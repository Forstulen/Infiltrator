using UnityEngine;
using System.Collections;

public class BulletScript : MonoBehaviour {

    // Public Variables
    public float LifeSpan = 2.0f;
    public AudioClip StartingBulletSound;
    public AudioClip CollidingPlayerBulletSound;
    public AudioClip CollidingLDBulletSound;
    public AttackScript.Damage AttackDamage = AttackScript.Damage.LIGHT;

    // Private Variables
    private float _timer;

	// Use this for initialization
	void Start () {
        this._timer = 0.0f;
        AudioManagerScript.Instance.Play(this.StartingBulletSound, this.gameObject.transform.position, 1.0f);
	}
	
	// Update is called once per frame
	void Update () {
        this._timer += Time.deltaTime;

        if (this._timer >= this.LifeSpan)
        {
            Destroy(this.gameObject);
        }
	}

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.tag == "Player")
        {
            if (!AudioManagerScript.Instance.IsPlaying(this.CollidingPlayerBulletSound))
                AudioManagerScript.Instance.Play(this.CollidingPlayerBulletSound, this.gameObject.transform.position, 1.0f);
            //LifeBarScript lifeBarScript = GameObject.Find("LifeBar").GetComponent<LifeBarScript>();
            FixedCameraScript cameraScript = Camera.main.GetComponent<FixedCameraScript>();

            cameraScript.ActivateShaking();
            //switch (this.AttackDamage)
            //{
            //    case AttackScript.Damage.LIGHT:
            //        //lifeBarScript.ChangeState(1);
            //        break;
            //    case AttackScript.Damage.MODERATE:
            //        lifeBarScript.ChangeState(2);
            //        break;
            //    case AttackScript.Damage.DEADLY:
            //        lifeBarScript.ChangeState(10);
            //        break;
            //    default:
            //        lifeBarScript.ChangeState(1);
            //        break;
            //}
        }
        else
        {
            AudioManagerScript.Instance.Play(this.CollidingLDBulletSound, this.gameObject.transform.position, 1.0f);
        }
        Destroy(this.gameObject);
    }
}
