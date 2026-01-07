using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIEventTrigger : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, ISelectHandler, ISubmitHandler
{
    [SerializeField] private AudioData selectSFX;
    [SerializeField] private AudioData submitSFX;

    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioManager.Instance.PlaySfx(selectSFX);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        AudioManager.Instance.PlaySfx(submitSFX);
    }

    public void OnSelect(BaseEventData eventData)
    {
        AudioManager.Instance.PlaySfx(selectSFX);

    }

    public void OnSubmit(BaseEventData eventData)
    {
        AudioManager.Instance.PlaySfx(submitSFX);

    }
}
