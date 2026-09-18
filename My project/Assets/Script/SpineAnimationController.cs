using UnityEngine;
using TMPro;
using Spine;
using Spine.Unity;

public class SpineAnimationController : MonoBehaviour
{
    [Header("Spine")]
    [SerializeField] private SkeletonAnimation skeletonAnimation;

    [Header("UI")]
    [SerializeField] private TMP_Text animationText;

    [Header("Turn Image")]
    [SerializeField] private GameObject turnImage;

    [Header("Animation Names")]
    [SpineAnimation(dataField: "skeletonAnimation")]
    [SerializeField] private string idleAnimation;

    [SpineAnimation(dataField: "skeletonAnimation")]
    [SerializeField] private string turnAnimation;

    [SpineAnimation(dataField: "skeletonAnimation")]
    [SerializeField] private string attackAnimation;

    [SpineAnimation(dataField: "skeletonAnimation")]
    [SerializeField] private string hitAnimation;

    [SpineAnimation(dataField: "skeletonAnimation")]
    [SerializeField] private string deathAnimation;

    [Header("Display Names")]
    [SerializeField] private string idleDisplayName = "평상시";
    [SerializeField] private string turnDisplayName = "해당 캐릭터 턴";
    [SerializeField] private string attackDisplayName = "공격";
    [SerializeField] private string hitDisplayName = "피격됨";
    [SerializeField] private string deathDisplayName = "사망";

    private bool isAttacking;
    private bool isHitting;
    private bool isDead;

    // 공격/피격 전 상태
    private bool isTurn;

    private void Start()
    {
        if (skeletonAnimation == null)
            skeletonAnimation = GetComponent<SkeletonAnimation>();

        PlayIdle();
    }

    private void Update()
    {
        // 사망 상태에서는 다른 애니메이션 입력 무시
        if (isDead)
            return;

        // I : 평상시
        if (Input.GetKeyDown(KeyCode.I))
        {
            PlayIdle();
        }

        // T : 해당 캐릭터 턴
        if (Input.GetKeyDown(KeyCode.T))
        {
            PlayTurn();
        }

        // A : 공격
        if (Input.GetKeyDown(KeyCode.A))
        {
            PlayAttack();
        }

        // H : 피격됨
        if (Input.GetKeyDown(KeyCode.H))
        {
            PlayHit();
        }

        // D : 사망
        if (Input.GetKeyDown(KeyCode.D))
        {
            PlayDeath();
        }
    }

    private void PlayIdle()
    {
        isAttacking = false;
        isHitting = false;
        isTurn = false;

        // Turn 이미지 끄기
        if (turnImage != null)
            turnImage.SetActive(false);

        skeletonAnimation.AnimationState.SetAnimation(
            0,
            idleAnimation,
            true
        );

        UpdateAnimationText(idleDisplayName);
    }

    private void PlayTurn()
    {
        isAttacking = false;
        isHitting = false;
        isTurn = true;

        // Turn 이미지 켜기
        if (turnImage != null)
            turnImage.SetActive(true);

        skeletonAnimation.AnimationState.SetAnimation(
            0,
            turnAnimation,
            true
        );

        UpdateAnimationText(turnDisplayName);
    }

    private void PlayAttack()
    {
        if (isAttacking || isHitting)
            return;

        isAttacking = true;

        TrackEntry entry = skeletonAnimation.AnimationState.SetAnimation(
            0,
            attackAnimation,
            false
        );

        UpdateAnimationText(attackDisplayName);

        entry.Complete += OnAttackComplete;
    }

    private void OnAttackComplete(TrackEntry trackEntry)
    {
        isAttacking = false;

        if (isDead)
            return;

        // 공격 전에 Turn 상태였다면 Turn으로 복귀
        if (isTurn)
        {
            PlayTurn();
        }
        else
        {
            PlayIdle();
        }
    }

    private void PlayHit()
    {
        if (isAttacking || isHitting)
            return;

        isHitting = true;

        TrackEntry entry = skeletonAnimation.AnimationState.SetAnimation(
            0,
            hitAnimation,
            false
        );

        UpdateAnimationText(hitDisplayName);

        entry.Complete += OnHitComplete;
    }

    private void OnHitComplete(TrackEntry trackEntry)
    {
        isHitting = false;

        if (isDead)
            return;

        // 피격 전에 Turn 상태였다면 Turn으로 복귀
        if (isTurn)
        {
            PlayTurn();
        }
        else
        {
            PlayIdle();
        }
    }

    private void PlayDeath()
    {
        if (isDead)
            return;

        isDead = true;
        isAttacking = false;
        isHitting = false;

        // 사망하면 Turn 이미지도 끄기
        if (turnImage != null)
            turnImage.SetActive(false);

        skeletonAnimation.AnimationState.SetAnimation(
            0,
            deathAnimation,
            false
        );

        UpdateAnimationText(deathDisplayName);
    }

    private void UpdateAnimationText(string displayName)
    {
        if (animationText != null)
        {
            animationText.text = $"애니메이션 작동 : {displayName}";
        }
    }
}