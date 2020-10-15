using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshRenderer))]
public class TransparentObjectScript : MonoBehaviour {

    // Public Variables
    public float        TimeAmount = 0.2f;
    public float        Percentage = 0.75f;

    // Private Variables
    private Material    _material;
    private float       _timer = 0.0f;
    private bool        _isTransparent = false;

	// Use this for initialization
	void Start () {
	    this._material = this.gameObject.GetComponent<Renderer>().material;
	}
	
	// Update is called once per frame
	void Update () {
        Color newColor = this._material.color;

        if (this._isTransparent) {
            newColor.a = Mathf.Lerp(1.0f, this.Percentage, this._timer);
        } else {
            newColor.a = Mathf.Lerp(this.Percentage, 1.0f, this._timer);
        }
        this._material.color = newColor;
        this._timer += this.TimeAmount * Time.deltaTime;
	}

    public void FadeIn()
    {
        if (!this._isTransparent)
        {
            this._timer = 0;
        }
        this._isTransparent = true;
    }

    public void FadeOut()
    {
        if (this._isTransparent)
        {
            this._timer = 0;
        }
        this._isTransparent = false;
    }
}
