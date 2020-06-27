using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Main purpose of this input module is to disable mouse input taking priority over controller.
// Based on StandaloneInputModule default implementation in their respective methods.
public class EventSystemInputOverride : StandaloneInputModule
{
    public override bool IsModuleSupported()
    {
        return forceModuleActive;
    }

    // In addition override idea to exclude input.mousePresnet line and ProcessMouse/ProcessTouchEvents
    // was inspired by post "Make Standalone Input Module ignore mouse input?" on Unity Forums.
    public override void Process()
    {
        if (!eventSystem.isFocused) // && ShouldIgnoreEventsOnNoFocus() not in scope
            return;
        
        bool usedEvent = SendUpdateEventToSelectedObject();

        if (eventSystem.sendNavigationEvents)
        {
            if (!usedEvent)
                usedEvent |= SendMoveEventToSelectedObject();
            
            if (!usedEvent)
                SendSubmitEventToSelectedObject();
        }
    }

    public override bool ShouldActivateModule()
    {
        if (!base.ShouldActivateModule())
            return false;

        bool shouldActivate = forceModuleActive;

        shouldActivate &=
            input.GetButtonDown("Submit");
        shouldActivate &=
            input.GetButtonDown("Cancel");
        shouldActivate &=
            !Mathf.Approximately(input.GetAxisRaw("Left Joystick Horizontal"), 0.0f);
        shouldActivate &=
            !Mathf.Approximately(input.GetAxisRaw("Left Joystick Vertical"), 0.0f);

        return shouldActivate;
    }
}
