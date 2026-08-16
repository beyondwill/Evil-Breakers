using UnityEngine;

public class GameRuleManager : MonoBehaviour
{
    public static GameRuleManager Instance { get; private set; }

    [SerializeField] private GameRuleSO gameRule;

    public GameRuleSO Rule => gameRule;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}