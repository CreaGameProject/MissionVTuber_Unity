using UnityEngine;
using System.Collections;

public class TestLever : MonoBehaviour {

    public Lever lever;
    public TextMesh ModeText;
    public Material TriggerMaterial;
    public TextMesh ValueText;
    public TextMesh EventText;
    public Renderer clrRenderer;

    protected Material OldMaterial;
    
    protected void Awake()
    {
        ModeText.text = "Mode: " + lever.Mode;
    }

    protected void OnEnable()
    {
        if (lever != null)
        {
            lever.OnEngaged += OnEngaged;
            lever.OnAdjustComplete += OnAdjustComplete;
        }
    }

    protected void OnDisable()
    {
        if (lever != null)
        {
            lever.OnEngaged -= OnEngaged;
            lever.OnAdjustComplete -= OnAdjustComplete;
        }
    }

    protected void OnAdjustComplete()
    {
        EventText.text = "Event Val: " + lever.CurrentValue + " pos: " + lever.CurrentLeverPosition;
    }

    protected void OnEngaged()
    {
        Renderer r = GetComponent<Renderer>();
        OldMaterial = r.material;
        r.material = TriggerMaterial;

        Invoke("Reset", 2);
    }

    protected void Reset()
    {
        Renderer r = GetComponent<Renderer>();
        r.material = OldMaterial;
    }

    void Update()
    {
        Color clr = Color.Lerp(Color.red, Color.blue, lever.CurrentValue);
        clrRenderer.material.color = clr;

        ValueText.text = "Val: " + lever.CurrentValue + " pos: "+ lever.CurrentLeverPosition;
    }
}
