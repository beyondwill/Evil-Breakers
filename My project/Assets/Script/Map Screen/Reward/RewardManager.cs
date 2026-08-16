using System.Collections.Generic;
using UnityEngine;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance { get; private set; }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }



    [Header("UI")]
    [SerializeField] private RewardWindow rewardWindow;
    [SerializeField] private InventoryUI inventoryUI;



    private List<InventoryItem> CurrentRewards =>
        DataManager.Instance.GetBattleData.leftRewards;


    private void Start()
    {
        if (DataManager.Instance.GetAllData.current_state == CurrentState.BattleEnd)
        {
            if (CurrentRewards.Count == 0)
            {
                ResetReward();
            }
            else
            {
                ShowReward();
            }
        }
        else
        {
            ResetReward();
        }
    }





    public void ShowReward()
    {
        rewardWindow.gameObject.SetActive(true);
        rewardWindow.ShowItems(CurrentRewards);
    }






    public void SetReward(List<InventoryItem> rewards)
    {
        CurrentRewards.Clear();


        foreach (InventoryItem item in rewards)
        {
            CurrentRewards.Add(item);
        }


        DataManager.Instance.GetAllData.SetCurrentState(CurrentState.BattleMap);
        DataManager.Instance.SaveData();
    }






    public void TakeItem(int index)
    {
        if(index < 0 || index >= CurrentRewards.Count)
            return;


        InventoryItem reward = CurrentRewards[index];


        int remain =
            DataManager.Instance.GetBattleData.AddItem(
                reward.item,
                reward.amount
            );


        if(remain == 0)
        {
            CurrentRewards.RemoveAt(index);
        }
        else
        {
            reward.amount = remain;
        }


        rewardWindow.Refresh(CurrentRewards);

        inventoryUI.Refresh();

        DataManager.Instance.SaveData();
    }


    public void ResetReward()
    {
        CurrentRewards.Clear();


        rewardWindow.Refresh(CurrentRewards);

        rewardWindow.gameObject.SetActive(false);


        DataManager.Instance.GetAllData.SetCurrentState(CurrentState.BattleMap);

        DataManager.Instance.SaveData();
    }

    public void CreateReward(List<InventoryItem> rewards)
    {
        SetReward(rewards);

        ShowReward();
    }

    public List<InventoryItem> goza;

    public void MakeReward()
    {
        if (DataManager.Instance.GetAllData.current_state
            == CurrentState.BattleEnd)
        {
            DataManager.Instance.GetAllData.current_state
                = CurrentState.BattleMap;


            CreateReward(goza);
        }
    }
}