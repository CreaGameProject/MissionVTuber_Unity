using UnityEngine;
using System.Collections;

public class TestSwitchButton : MonoBehaviour {

    public ButtonSwitch switchBtn;

    private Renderer render;

    void Awake()
    {
        if (switchBtn == null)
        {
            switchBtn = GetComponent<ButtonSwitch>();
        }
        render = GetComponent<MeshRenderer>();

        OnSwitchToggle(switchBtn, switchBtn.State);
    }
	
    void OnEnable()
    {
        switchBtn.OnStateChangedTo += OnSwitchToggle;
    }

    void OnDisable()
    {
        switchBtn.OnStateChangedTo -= OnSwitchToggle;
    }

    void OnSwitchToggle(ButtonSwitch btn, ButtonSwitch.ButtonSwitchState newState)
    {
        if (newState == ButtonSwitch.ButtonSwitchState.ON)
        {
            render.material.color = Color.red;
        }
        else
        {
            render.material.color = Color.blue;
        }
    }
}
