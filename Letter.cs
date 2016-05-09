using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class Letter : MonoBehaviour {

    private char _c;
    public TextMesh tMesh;
    public Renderer tRend;
    public bool big = false;
    public List<Vector3> pts = null;
    public float timeDuration = .5f;
    public float timeStart = -1;
    public string easingCuve = Easing.InOut;

    void Awake()
    {
        tMesh = GetComponentInChildren<TextMesh>();
        tRend = tMesh.GetComponent<Renderer>();
        visible = false;
    }

    public char c
    {
        get
        {
            return (_c);

        }
        set
        {
            _c = value;
            tMesh.text = _c.ToString();
        }
    }
    public string str
    {
        get
        {
            return (_c.ToString());
        }
        set
        {
            c = value[0];
        }
    }
    public bool visible
    {
        get
        {
            return (tRend.enabled);
        }
        set
        {
            tRend.enabled = value;
        }
    }

    public Color color
    {
        get
        {
            return (GetComponent<Renderer>().material.color);
        }
        set
        {
            GetComponent<Renderer>().material.color = value;

        }
    }

    public Vector3 pos
    {
        set
        {
            Vector3 mid = (transform.position + value) / 2f;
            float mag = (transform.position - value).magnitude;
            mid += Random.insideUnitSphere * mag * 0.25f;
            pts = new List<Vector3>() { transform.position, mid, value };
            if (timeStart == -1) timeStart = Time.time;
        }
    }

    public Vector3 position
    {
        set
        {
            transform.position = value;
        }
    }

    

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (timeStart == -1) return;
        float u = (Time.time - timeStart) / timeDuration;
        u = Mathf.Clamp01(u);
        float u1 = Easing.Ease(u, easingCuve);
        Vector3 v = Utils.Bezier(u1, pts);
        transform.position = v;

        if (u == 1) timeStart = -1;
	
	}
}
