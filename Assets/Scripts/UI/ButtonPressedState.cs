using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ButtonPressedState : StateMachineBehaviour
{
    public static Dictionary<Button, System.Action> ButtonFunctionTable;

    void Awake()
    {
        ButtonFunctionTable = new Dictionary<Button, System.Action>();
    }

    private static void DisableOtherButton(Component button)
    {
        if (button.gameObject.name is "Button Cancel" or "Button Submit") return;
        foreach (var value in ButtonFunctionTable.Where(value => value.Key.gameObject.name != button.gameObject.name))
        {
            value.Key.interactable = false;
        }
    }

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        UIInput.Instance.DisableAllUIInput();
        
        // 禁用其他按钮
        DisableOtherButton(animator.gameObject.GetComponent<Button>());
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    // public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    // {
    // }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 当按钮动画播放结束时，才执行按钮对应的功能
        ButtonFunctionTable[animator.gameObject.GetComponent<Button>()].Invoke();
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
