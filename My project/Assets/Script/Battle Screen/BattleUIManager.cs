using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;


public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager Instance;


    [Header("Text")]
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private TextMeshProUGUI deckCountText;
    [SerializeField] private TextMeshProUGUI graveyardCountText;


    [Header("Objects")]
    [SerializeField] private List<GameObject> LeftMoveObjects = new();
    [SerializeField] private List<GameObject> RightMoveObjects = new();


    [Header("Move")]
    [SerializeField] private float moveDistance = 2000f;


    private List<float> leftOriginalX = new();
    private List<float> rightOriginalX = new();



    private void Awake()
    {
        Instance = this;

        ShowDeckCount(0);
        ShowGraveyardCount(0);
        SaveOriginalPosition();
        MoveTurnObjects(0);
    }



    private void Start()
    {
        CardManager.Instance.OnDeckChanged += ShowDeckCount;
        CardManager.Instance.OnGraveyardChanged += ShowGraveyardCount;
    }



    private void SaveOriginalPosition()
    {
        leftOriginalX.Clear();
        rightOriginalX.Clear();


        foreach (GameObject obj in LeftMoveObjects)
        {
            if (obj == null)
                continue;


            leftOriginalX.Add(
                obj.transform.localPosition.x
            );
        }


        foreach (GameObject obj in RightMoveObjects)
        {
            if (obj == null)
                continue;


            rightOriginalX.Add(
                obj.transform.localPosition.x
            );
        }
    }



    public void ShowRound(int round)
    {
        roundText.text =
            round.ToString();
    }



    public void ShowEnergy(
        int currentEnergy,
        int maxEnergy)
    {
        energyText.text =
            currentEnergy +
            " / " +
            maxEnergy;
    }



    public void ShowDeckCount(int count)
    {
        deckCountText.text =
            count.ToString();
    }



    public void ShowGraveyardCount(int count)
    {
        graveyardCountText.text =
            count.ToString();
    }




    // 턴 종료 시 UI 이동
    public void MoveTurnObjects(float moveDuration = 0.5f)
    {
        for (int i = 0; i < LeftMoveObjects.Count; i++)
        {
            if (LeftMoveObjects[i] == null)
                continue;


            Transform tr =
                LeftMoveObjects[i].transform;


            tr.DOKill();


            tr.DOLocalMoveX(
                leftOriginalX[i] - moveDistance,
                moveDuration
            )
            .SetEase(Ease.OutCubic);
        }



        for (int i = 0; i < RightMoveObjects.Count; i++)
        {
            if (RightMoveObjects[i] == null)
                continue;


            Transform tr =
                RightMoveObjects[i].transform;


            tr.DOKill();


            tr.DOLocalMoveX(
                rightOriginalX[i] + moveDistance,
                moveDuration
            )
            .SetEase(Ease.OutCubic);
        }
    }





    // 플레이어 턴 시작 시 복귀
    public void ResetTurnObjects(float moveDuration = 0.5f)
    {
        for (int i = 0; i < LeftMoveObjects.Count; i++)
        {
            if (LeftMoveObjects[i] == null)
                continue;


            Transform tr =
                LeftMoveObjects[i].transform;


            tr.DOKill();


            tr.DOLocalMoveX(
                leftOriginalX[i],
                moveDuration
            )
            .SetEase(Ease.OutCubic);
        }



        for (int i = 0; i < RightMoveObjects.Count; i++)
        {
            if (RightMoveObjects[i] == null)
                continue;


            Transform tr =
                RightMoveObjects[i].transform;


            tr.DOKill();


            tr.DOLocalMoveX(
                rightOriginalX[i],
                moveDuration
            )
            .SetEase(Ease.OutCubic);
        }
    }
}