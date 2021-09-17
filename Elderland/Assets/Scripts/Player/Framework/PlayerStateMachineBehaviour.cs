using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Class that all player state machine states (normal gameplay states such as movement but not
things like cutscenes) can inherit from for ease of use. In addition, all states that want to be.
considered as outside sources should inherit from this as well. Lastly, in order for this pattern to work
the states must set Exiting to true when making an internal transition to another state.
*/
public abstract class PlayerStateMachineBehaviour : StateMachineBehaviour 
{
    public bool Exiting { get; set; }
    // Field needed for states that do not have internal transitions via code. Must be set to true on states
    // that do not have internal transitions for pattern to work. These states must not have exit
    // code when "overriden" as they cannot detect when they are overriden. Often exit code
    // is done via OnStateExit method from StateMachineBehaviour definition.
    protected bool transitionless { get; set; } 
    // States that will not have an immediate exit function because they will never be overriden. 
    // An error is thrown if a non overriable state is overriden.
    protected bool unoverrideable { get; set; } 

    protected virtual void OnStateExitImmediate() {}

    /*
    The following goes at the top of the update method for the behaviour, afterwards the runnable
	code must only run if the behaviour is not exiting still.
    */
	protected void CheckForOutsideTransition(AnimatorStateInfo stateInfo)
	{
		if (PlayerInfo.AnimationManager.CurrentBehaviour != this &&
            !Exiting &&
            !transitionless)
		{
            if (unoverrideable)
            {
                throw new System.Exception("Un overrideable state was overrident: " + stateInfo);
            }
            else
            {
                OnStateExitImmediate();
			    Exiting = true;
            }
		}
	}

    // If adding logic in child classes that use enter method, make sure to call base method first.
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		Exiting = false;
        PlayerInfo.AnimationManager.CurrentBehaviour = this;
	}

    // If adding logic in child classes that use enter method, make sure to call base method first.
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
        CheckForOutsideTransition(stateInfo);
	}
}