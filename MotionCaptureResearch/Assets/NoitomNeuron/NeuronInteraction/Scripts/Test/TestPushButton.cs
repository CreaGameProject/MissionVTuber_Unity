using UnityEngine;
using System.Collections;

public class TestPushButton : MonoBehaviour {

    public PushButton Button;

    public TextMesh Text;
    public Renderer Renderer;

    protected int clickTimes = 0;

    void Awake()
    {
        Reset();
    }

    void OnEnable()
    {
        Button.OnButtonPushed += OnButtonPushed;
        Button.OnButtonReleased += OnButtonReleased;
    }

    void OnDisable()
    {
        Button.OnButtonPushed -= OnButtonPushed;
        Button.OnButtonReleased -= OnButtonReleased;
    }

    void OnButtonPushed()
    {
        clickTimes = clickTimes + 1;
        Text.text = string.Format("clicked: {0}", clickTimes);

        Renderer.material.color = Color.red;
    }

    void OnButtonReleased()
    {
        Reset();
    }

    void Reset()
    {
        Renderer.material.color = Color.white;
    }
}
