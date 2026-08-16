using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// 전투 변수
[System.Serializable]
public class BattleResultVariable
{
    public int win_battle_count = 0;
    public int slained_player_count = 0;
    public int slained_enemy_count = 0;
    public int played_card_count = 0;
    public int used_item_count = 0;
}

public class BattleResult : MonoBehaviour
{
    // ==========================================
    // 싱글톤
    // ==========================================

    public static BattleResult Instance;


    // ==========================================
    // 외부 요소
    // ==========================================

    [Header("결과 텍스트")]
    [SerializeField] private TextMeshProUGUI win_battle_count_text;
    [SerializeField] private TextMeshProUGUI slained_player_count_text;
    [SerializeField] private TextMeshProUGUI slained_enemy_count_text;
    [SerializeField] private TextMeshProUGUI played_card_count_text;
    [SerializeField] private TextMeshProUGUI used_item_count_text;

    [SerializeField] private TextMeshProUGUI misson_text;

    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("사운드")]
    [SerializeField] private AudioClip a;
    [SerializeField] private AudioClip defeat;

    [Header("보상 UI")]
    [SerializeField] private Transform ClearRewardGroup;
    [SerializeField] private Transform itemRewardGroup;

    [SerializeField] private IconButton iconPrefab;

    // 돈 아이콘
    [SerializeField] private Sprite moneyIcon;


    // ==========================================
    // 변수
    // ==========================================

    private List<IconButton> itemIcons = new();


    // ==========================================
    // Unity
    // ==========================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void Start()
    {
        // 시작할 때 결과창 숨김
        gameObject.SetActive(false);
    }


    // ==========================================
    // 전투 결과 보여주기
    // ==========================================

    public void ShowBattleResult(bool win)
    {
        DataManager.Instance.GetAllData.SetCurrentState(
            CurrentState.MainScreen
        );


        // ==========================================
        // 전투 결과
        // ==========================================

        BattleResultVariable BRV =
            DataManager.Instance.GetBattleData.battle_result_variables;


        win_battle_count_text.text =
            BRV.win_battle_count.ToString() + "회";

        slained_player_count_text.text =
            BRV.slained_player_count.ToString() + "명";

        slained_enemy_count_text.text =
            BRV.slained_enemy_count.ToString() + "개체";

        played_card_count_text.text =
            BRV.played_card_count.ToString() + "장";

        used_item_count_text.text =
            BRV.used_item_count.ToString() + "개";


        // ==========================================
        // 결과창 표시
        // ==========================================

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        canvasGroup.DOKill();

        canvasGroup.DOFade(1f, 1f);


        // ==========================================
        // BGM
        // ==========================================

        if (win)
        {
            AudioManager.Instance.PlayBGM(
                a,
                0.5f
            );
        }
        else
        {
            AudioManager.Instance.FadeOutBGM(0.1f);

            DOVirtual.DelayedCall(0.11f, () =>
            {
                AudioManager.Instance.PlaySoundOnce(
                    AudioSort.BGM,
                    defeat
                );
            });
        }


        // ==========================================
        // 날짜 증가
        // ==========================================

        DataManager.Instance.GetAllData.main_data.day++;


        // ==========================================
        // 보상 표시
        // ==========================================

        ShowRewardItems();


        // ==========================================
        // 캐릭터 데이터 저장
        // ==========================================

        UpdateCharacterData();


        // ==========================================
        // 인벤토리 저장
        // ==========================================

        SaveInventoryToMainData();
    }


    // ==========================================
    // 보상 아이템 보여주기
    // ==========================================

    private void ShowRewardItems()
    {
        // ==========================================
        // 기존 아이템 보상 제거
        // ==========================================

        foreach (var icon in itemIcons)
        {
            if (icon != null)
            {
                Destroy(icon.gameObject);
            }
        }

        itemIcons.Clear();


        // ==========================================
        // 기존 클리어 보상 제거
        // ==========================================

        foreach (Transform child in ClearRewardGroup)
        {
            Destroy(child.gameObject);
        }


        // ==========================================
        // 전투 인벤토리
        // ==========================================

        List<InventoryItem> items =
            DataManager.Instance.GetBattleData.slots;


        Debug.Log(
            $"전투 보상 아이템 수 : {items.Count}"
        );


        for (int i = 0; i < items.Count; i++)
        {
            InventoryItem inventoryItem = items[i];


            // 빈 슬롯
            if (inventoryItem == null ||
                inventoryItem.item == null)
            {
                continue;
            }


            // ==========================================
            // 아이콘 생성
            // ==========================================

            IconButton icon =
                Instantiate(
                    iconPrefab,
                    itemRewardGroup
                );


            icon.ButtonInit(
                i,
                Color.white,
                inventoryItem.item.icon,
                false
            );


            icon.SetAmount(
                inventoryItem.amount
            );


            itemIcons.Add(icon);
        }


        // ==========================================
        // 모든 미션 클리어 보상
        // ==========================================

        if (DataManager.Instance
            .GetBattleData
            .map_data
            .IsAllMissionComplete)
        {
            Debug.Log(
                "모든 미션 클리어! 보상 1000"
            );

            misson_text.text = "모든 임무를 완료했습니다.";

            IconButton moneyIconButton =
                Instantiate(
                    iconPrefab,
                    ClearRewardGroup
                );


            moneyIconButton.ButtonInit(
                0,
                Color.white,
                moneyIcon,
                false
            );


            moneyIconButton.SetAmount(1000);


            // MainData에 돈 추가
            DataManager.Instance
                .GetAllData
                .main_data
                .money += 1000;
        }

        else
        {
            misson_text.text = "임무를 완수하지 못했습니다.";
        }
    }


    // ==========================================
    // 전투 끝내기
    // ==========================================

    public void EndBattle()
    {
        Destroy(
            DataManager.Instance.gameObject
        );

        SceneManager.LoadScene(
            "Main Scene"
        );
    }


    // ==========================================
    // 캐릭터 데이터 업데이트
    // ==========================================

    public void UpdateCharacterData()
    {
        foreach (
            PlayerCharacterData PCD
            in DataManager.Instance
                .GetBattleData
                .characters_in_battle_data_list)
        {
            Debug.Log(
                "캐릭터 스탯 변경!"
            );


            PlayerCharacterData mainCharacter =
                DataManager.Instance
                    .GetAllData
                    .main_data
                    .player_character_data_list
                    .Find(
                        x =>
                            x.player_character_info ==
                            PCD.player_character_info
                    );


            // ==========================================
            // 원본 데이터에 없는 캐릭터
            // ==========================================

            if (mainCharacter == null)
            {
                continue;
            }


            mainCharacter.current_health =
                PCD.current_health;
        }


        DataManager.Instance.SaveData();
    }


    // ==========================================
    // 인벤토리 저장
    // ==========================================

    public void SaveInventoryToMainData()
    {
        MainData mainData =
            DataManager.Instance
                .GetAllData
                .main_data;


        List<InventoryItem> saveList =
            mainData.inventoryItemList;


        Debug.Log(
            $"saveList : {saveList}"
        );


        // ==========================================
        // 전투 인벤토리 순회
        // ==========================================

        foreach (
            InventoryItem item
            in DataManager.Instance
                .GetBattleData
                .slots)
        {
            // 빈 슬롯
            if (item == null ||
                item.IsEmpty)
            {
                continue;
            }


            // ==========================================
            // Money인 경우
            // ==========================================

            if (item.item.itemName == "돈")
            {
                mainData.money += item.amount;


                Debug.Log(
                    $"Money 획득 : {item.amount}"
                );

                Debug.Log(
                    $"현재 보유 돈 : {mainData.money}"
                );


                // 전투 인벤토리에서 제거
                item.Clear();


                continue;
            }


            // ==========================================
            // 일반 아이템
            // ==========================================

            InventoryItem exist =
                saveList.Find(
                    x =>
                        x.item == item.item
                );


            // ==========================================
            // 기존 아이템이 존재
            // ==========================================

            if (exist != null)
            {
                exist.amount += item.amount;
            }


            // ==========================================
            // 기존 아이템이 없음
            // ==========================================

            else
            {
                saveList.Add(
                    new InventoryItem(
                        item.item,
                        item.amount
                    )
                );
            }


            // ==========================================
            // 전투 인벤토리 비우기
            // ==========================================

            item.Clear();
        }


        // ==========================================
        // 저장
        // ==========================================

        DataManager.Instance.SaveData();
    }
}