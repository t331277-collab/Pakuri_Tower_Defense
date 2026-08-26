/*
 * 역할: 공용 UI Button 클릭음 전달.
 * 책임: 활성 Button의 pointer click과 submit 입력을 SoundManager에 전달한다.
 */

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class UiButtonClickSound : MonoBehaviour, IPointerClickHandler, ISubmitHandler
{
    private SoundManager soundManager;
    private Button button;

    public void Initialize(SoundManager manager, Button targetButton)
    {
        soundManager = manager;
        button = targetButton;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            PlayIfInteractable();
        }
    }

    public void OnSubmit(BaseEventData eventData)
    {
        PlayIfInteractable();
    }

    private void PlayIfInteractable()
    {
        if (button != null && button.isActiveAndEnabled && button.interactable)
        {
            soundManager?.PlayUiButtonClick();
        }
    }
}
