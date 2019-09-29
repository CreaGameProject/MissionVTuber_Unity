using UnityEngine;
using System.Collections;

public class TestKnob : MonoBehaviour {

    public RotationKnob Knob;

    public TextMesh EventText;
    private TextMesh Text;

    private void Awake()
    {
        Text = this.GetComponent<TextMesh>();
    }

    void OnEnable()
    {
        Knob.OnAdjustComplete += OnAdjustComplete;
    }

    void OnDisable()
    {
        Knob.OnAdjustComplete -= OnAdjustComplete;
    }

    void OnAdjustComplete()
    {
        float v = Knob.CurrentValue;
        EventText.text = string.Format("event: {0}", v);
    }

    private void Update()
    {
        float v = Knob.CurrentValue;
        Text.text = string.Format("{0}", v);
    }
}
