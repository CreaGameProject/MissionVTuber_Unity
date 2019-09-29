using UnityEngine;
using System.Collections;

public class TestSlider : MonoBehaviour {

    public SliderButton slider;
    public Vector3 startp = new Vector3(0, 1, 0);
    public Vector3 endp = new Vector3(4, 5, 4);
    public Color st = Color.black;
    public Color ed = Color.white;
    public TextMesh Text;

    public TextMesh EventText;

    private float oldVal = -1f;
    // Use this for initialization
    void Start () {
        //Vector3 pos = Vector3.Lerp(startp, endp, slider.CurrentValue);
        //transform.position = pos;
    }

    void OnEnable()
    {
        slider.OnAdjustComplete += OnAdjustComplete;
    }

    void OnDisable()
    {
        slider.OnAdjustComplete -= OnAdjustComplete;
    }

    void OnAdjustComplete()
    {
        EventText.text = "slider (event): " + slider.CurrentValue;
        print("fff " + slider.CurrentValue);
    }

    // Update is called once per frame
    void Update () {
        if (oldVal != slider.CurrentValue)
        {
            oldVal = slider.CurrentValue;

            //Vector3 pos = Vector3.Lerp(startp, endp, slider.CurrentValue);
            //transform.position = pos;

            Color clr = Color.Lerp(st, ed, slider.CurrentValue);
            Renderer r = GetComponent<Renderer>();
            r.material.color = clr;
            //print("Adjust pos " + pos + " " + oldVal);

            Text.text = "slider: " + slider.CurrentValue;
        }
    }
}
