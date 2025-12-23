using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class OptionsUIController : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    void OnEnabled()
    {
        playerInput.EnableGameOverInput();
    }
    
}
