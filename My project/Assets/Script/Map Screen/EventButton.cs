using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI buttonText;

    public void Init(EventChoice choice)
    {
        buttonText.text = choice.choiceText;

        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() =>
        {
            foreach (EventEffect effect in choice.effects)
            {
                effect.Execute();
            }

            if (choice.nextEvent != null)
                EventManager.Instance.ShowEvent(choice.nextEvent);
            else
                EventManager.Instance.CloseEvent();
        });
    }

    public void InitCloseButton()
    {
        buttonText.text = "³¡³»±â";

        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() =>
        {
            EventManager.Instance.CloseEvent();
        });
    }
}