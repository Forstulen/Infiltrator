using UnityEngine;
using System;
using System.Collections;

public class SearchPlayerScript : MonoBehaviour {

    // Public Variables
    public float    DistanceMax = 10.0f;
    public float    HearDistance = 9.0f;
    public float    FeelingRadius = 4.0f;
    public int      FieldOfView = 90;
    public int      MaxFieldOfView = 360;
    public float    AwarenessDelay = 10.0f;
    public float    AlertRecovery = 15.0f;
    [System.NonSerialized]
    public Vector3 LastKnownPosition;

    // Private Variables
    private int _currentFieldOfView;
    private float _awarenessTimer = 0.0f;
    private float _alertTimer;

    // Events
    public event SearchPlayerHandler PlayerFound;
    public event SearchPlayerHandler PlayerAlmostFound;
    public event SearchPlayerHandler PlayerNotFound;
    public event SearchPlayerHandler PlayerHeard;
    public delegate void SearchPlayerHandler(SearchPlayerScript script, EventArgs e);

    // Private Variables
    private ThirdPersonControllerScript _player;

	// Use this for initialization
	void Start () {
        _player = GameObject.Find("Player").GetComponent<ThirdPersonControllerScript>();
        _currentFieldOfView = FieldOfView;
        _alertTimer = AlertRecovery;

        _player.Running += PlayerIsHeard;
        _player.Knock += PlayerIsHeard;
	}

    void PlayerIsFound()
    {
        LastKnownPosition = _player.transform.position;
        _currentFieldOfView = MaxFieldOfView;
        _awarenessTimer = 0.0f;
        if (PlayerFound != null) PlayerFound(this, null);
    }

    void PlayerIsAlmostFound()
    {
        _currentFieldOfView = MaxFieldOfView;
        _awarenessTimer = 0.0f;
        _alertTimer = 0;
        if (PlayerAlmostFound != null) PlayerAlmostFound(this, null);
    }

    void PlayerIsHeard() {
        if (Mathf.Abs(Vector3.Distance(gameObject.transform.position, _player.transform.position)) < HearDistance) {
            _alertTimer = 0;
            _awarenessTimer = 0.0f;
            LastKnownPosition = _player.transform.position;
            PlayerHeard?.Invoke(this, null);
        }
    }

	// Update is called once per frame
	void Update () {
        _awarenessTimer += Time.deltaTime;
        _alertTimer += Time.deltaTime;
        if (_awarenessTimer >= AwarenessDelay)
        {
            _currentFieldOfView = FieldOfView;
            _awarenessTimer = 0.0f;
        }

        RaycastHit hit;
        Vector3 rayDirection = _player.transform.position - gameObject.transform.position;

        Debug.DrawRay(transform.position, rayDirection, Color.red);

        if (Physics.Raycast(gameObject.transform.position, rayDirection, out hit, DistanceMax)) {
            if (hit.transform.tag == "Player" && FeelingRadius > 0)
            {
                Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, FeelingRadius);

                for (int i = 0; i < hitColliders.Length; ++i)
                {
                    if (hitColliders[i].gameObject.tag == "Player")
                    {
                        if (Vector3.Distance(gameObject.transform.position, hitColliders[i].transform.position) < (FeelingRadius / 2.0f))
                        {
                            PlayerIsFound();
                        }
                        else if (_alertTimer >= AlertRecovery)
                        {
                            PlayerIsAlmostFound();
                        }
                    }
                }
            }
            
            if (hit.transform.tag == "Player" && (Mathf.Abs(Vector3.Angle(rayDirection, gameObject.transform.forward)) <= (_currentFieldOfView >> 1))) {
                 PlayerIsFound();
                return;
             }
             else if (hit.transform.tag == "Player" && rayDirection.magnitude <= DistanceMax / 2.0f && Input.GetButton("Fire3"))
             {
                 PlayerIsAlmostFound();
                 return;
             }
        }
        if (PlayerNotFound != null) PlayerNotFound(this, null);
	}
}
