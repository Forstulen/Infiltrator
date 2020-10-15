using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(RectTransform))]
public class RadarScript : MonoBehaviour {

    // Public Variables
    public GameObject EnemyPoint;
    public float RadarRange = 10.0f;
    public float TimeRepeat = 3.0f;
    public int EnemyPointDepth = 5; 
    public float SmoothRadarRotation = 10.0f;

    public Color Normal;
    public Color Alert;
    public Color Danger;

    // Private Variables
    private GameObject _player;
    private ThirdPersonControllerScript _thirdPersonControllerScript;
    [SerializeField]
    private List<RectTransform> _enemies;
    private RectTransform _radarRectTransform;
    private Vector3 _radarDirection;
    private float _timer;

	// Use this for initialization
	void Start () {
        _player = GameObject.Find("Player");
        _enemies = new List<RectTransform>();
        _radarRectTransform = gameObject.GetComponent<RectTransform>();
        _thirdPersonControllerScript = _player.GetComponentInChildren<ThirdPersonControllerScript>();
        _thirdPersonControllerScript.StopSliding += StopSliding;
        _thirdPersonControllerScript.UpdateWallNormal += UpdateWallNormal;
        _radarDirection = Vector3.back;
        _timer = 0.0f;
	}

    private void UpdateWallNormal(ThirdPersonControllerScript thirdPlayer, ThirdPersonControllerScript.SlidingArgs args)
    {
        _radarDirection = args.wallNormal.normalized;
    }

    private void StopSliding(ThirdPersonControllerScript thirdPlayer, ThirdPersonControllerScript.SlidingArgs args)
    {
        _radarDirection = Vector3.back;
    }

	// Update is called once per frame
	void Update () {
        _timer += Time.deltaTime;

        if (_timer >= TimeRepeat)
        {
            _timer = 0.0f;
            CheckEnemies();
        }
        RotateRadar();
	}

    void RotateRadar()
    {
        float value;

        if (_radarDirection == Vector3.forward)
            value = 180;
        else if (_radarDirection == Vector3.back)
            value = 0;
        else if (_radarDirection == Vector3.right)
            value = -90;
        else
            value = 90;

        Quaternion euler = Quaternion.Euler(0, 0, value);

        _radarRectTransform.rotation = Quaternion.Lerp(gameObject.transform.rotation, euler, SmoothRadarRotation * Time.deltaTime);
    }

    void PlaceEnemyPoints(Collider collider, Color color)
    {
        RectTransform enemyPoint = Instantiate(EnemyPoint, gameObject.transform).GetComponent<RectTransform>();
        Vector3 relativePosition = (collider.transform.position - _player.transform.position);

        _enemies.Add(enemyPoint);
        //// * 0.2f ==> Without the border of the radar
        ///

        enemyPoint.localPosition = new Vector2((relativePosition.x / RadarRange) * ((_radarRectTransform.rect.width - (_radarRectTransform.rect.width * 0.2f)) / 2),
                                            (relativePosition.z / RadarRange) * ((_radarRectTransform.rect.width - (_radarRectTransform.rect.width * 0.2f)) / 2));
        Image enemyPointImage = enemyPoint.GetComponent<Image>();
        //enemyPointImage.depth = EnemyPointDepth;
        enemyPointImage.color = color;
    }

    void CheckEnemies()
    {
        Collider[] hitColliders = Physics.OverlapSphere(_player.transform.position, RadarRange);

        foreach (RectTransform obj in _enemies)
            Destroy(obj.gameObject);
        _enemies.Clear();

        for (int i = 0; i < hitColliders.Length; ++i)
        {
            if (hitColliders[i].gameObject.tag == "Enemy")
            {
                PlaceEnemyPoints(hitColliders[i], Normal);
            }
            else if (hitColliders[i].gameObject.tag == "RedEnemy")
            {
                PlaceEnemyPoints(hitColliders[i], Danger);
            }
            else if ((hitColliders[i].gameObject.tag == "YellowEnemy"))
            {
                PlaceEnemyPoints(hitColliders[i], Alert);
            }
        }
    }
}
