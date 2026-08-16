using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    [SerializeField] private EventWindow eventWindow;

    void Awake()
    {
        Instance = this;
    }

    public void ShowEvent(EventInfo info)
    {
        Debug.Log("이벤트 보여주기!");
        eventWindow.gameObject.SetActive(true);
        eventWindow.InitEventWindow(info);
    }

    public void CloseEvent()
    {
        eventWindow.gameObject.SetActive(false);
    }
}