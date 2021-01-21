using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomButton : Button
{
    private bool isButtonDown;
    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);

        OnInteractableChanged(state == SelectionState.Disabled || state == SelectionState.Pressed);        
    }

    public void OnInteractableChanged(bool buttonDown)
    {
        if (isButtonDown == buttonDown) return;

        isButtonDown = buttonDown;
        Text text = GetComponentInChildren<Text>();

        if (buttonDown)
        {
            text.rectTransform.offsetMax = new Vector2(text.rectTransform.offsetMax.x, -31); 
            text.rectTransform.offsetMin = new Vector2(text.rectTransform.offsetMin.x, 14); 
        }
        else
        {
            text.rectTransform.offsetMax = new Vector2(text.rectTransform.offsetMax.x, -12);
            text.rectTransform.offsetMin = new Vector2(text.rectTransform.offsetMin.x, 32);
        }


    }
}
