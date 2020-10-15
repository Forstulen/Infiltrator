using UnityEngine;
using System.Collections;

public class SpawnScript : MonoBehaviour {

    // Private Variables
    private GameObject _startingPoint;
    private GameObject _player;

	// Use this for initialization
	void Start () {
        this._startingPoint = GameObject.Find("StartingPoint");
        this._player = GameObject.Find("Player");

        this._player.transform.position = this._startingPoint.transform.position;
        this._player.transform.rotation = this._startingPoint.transform.rotation;
	}
}
