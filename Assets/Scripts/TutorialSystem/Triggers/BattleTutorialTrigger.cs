using UnityEngine;

/// <summary>
/// Trigger de tutorial basado en eventos del sistema de batalla
/// </summary>
public class BattleTutorialTrigger : TutorialTrigger
{
    public enum BattleTriggerType
    {
        OnBattleStart,
        OnFirstPlayerTurn,
        OnFirstAttack,
        OnFirstQTEWindow,
        OnFirstEnemyTurn,
        OnFirstParryWindow,
        OnStaminaLow,
        OnFirstSkillUse,
        OnBattleVictory,
        OnBattleLoss
    }

    [Header("Configuración de Trigger de Batalla")]
    [SerializeField] private BattleTriggerType triggerType;
    [SerializeField] private BattleManagerV2 battleManager;

    [Header("Configuración Específica")]
    [Tooltip("Para OnStaminaLow: porcentaje de stamina (0-1)")]
    [SerializeField] private float staminaThreshold = 0.3f;

    private bool hasSeenPlayerTurn = false;
    private bool hasSeenQTEWindow = false;
    private bool hasSeenEnemyTurn = false;
    private bool hasSeenParryWindow = false;
    private bool hasSeenLowStamina = false;

    protected override void Start()
    {
        base.Start();

        // Buscar BattleManager si no está asignado
        if (battleManager == null)
        {
            battleManager = FindFirstObjectByType<BattleManagerV2>();
            if (battleManager == null)
            {
                Debug.LogWarning($"{gameObject.name}: No se encontró BattleManagerV2!");
                return;
            }
        }

        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        if (battleManager == null) return;

        switch (triggerType)
        {
            case BattleTriggerType.OnBattleStart:
                battleManager.OnBattleStateChanged += HandleBattleStateChanged;
                break;

            case BattleTriggerType.OnFirstPlayerTurn:
                battleManager.OnBattleStateChanged += HandleBattleStateChanged;
                break;

            case BattleTriggerType.OnFirstEnemyTurn:
                battleManager.OnBattleStateChanged += HandleBattleStateChanged;
                break;

            case BattleTriggerType.OnFirstQTEWindow:
                // Necesitamos acceder al QTEManager
                var qteManager = FindQTEManager();
                if (qteManager != null)
                {
                    qteManager.OnQTEWindowStart += HandleQTEWindowStart;
                }
                break;

            case BattleTriggerType.OnFirstParryWindow:
                // Necesitamos acceder al ParrySystem
                var parrySystem = FindParrySystem();
                if (parrySystem != null)
                {
                    parrySystem.OnParryWindowActive += HandleParryWindowActive;
                }
                break;

            case BattleTriggerType.OnStaminaLow:
                // Necesitamos acceder al PlayerBattleController para stamina
                var playerController = FindPlayerBattleController();
                if (playerController != null && playerController.Character != null)
                {
                    var staminaManager = playerController.Character.StaminaManager;
                    if (staminaManager != null)
                    {
                        staminaManager.OnStaminaChanged += HandleStaminaChanged;
                    }
                }
                break;

            case BattleTriggerType.OnBattleVictory:
            case BattleTriggerType.OnBattleLoss:
                battleManager.OnBattleEnded += HandleBattleEnded;
                break;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (battleManager == null) return;

        switch (triggerType)
        {
            case BattleTriggerType.OnBattleStart:
            case BattleTriggerType.OnFirstPlayerTurn:
            case BattleTriggerType.OnFirstEnemyTurn:
                battleManager.OnBattleStateChanged -= HandleBattleStateChanged;
                break;

            case BattleTriggerType.OnFirstQTEWindow:
                var qteManager = FindQTEManager();
                if (qteManager != null)
                {
                    qteManager.OnQTEWindowStart -= HandleQTEWindowStart;
                }
                break;

            case BattleTriggerType.OnFirstParryWindow:
                var parrySystem = FindParrySystem();
                if (parrySystem != null)
                {
                    parrySystem.OnParryWindowActive -= HandleParryWindowActive;
                }
                break;

            case BattleTriggerType.OnStaminaLow:
                var playerController = FindPlayerBattleController();
                if (playerController != null && playerController.Character != null)
                {
                    var staminaManager = playerController.Character.StaminaManager;
                    if (staminaManager != null)
                    {
                        staminaManager.OnStaminaChanged -= HandleStaminaChanged;
                    }
                }
                break;

            case BattleTriggerType.OnBattleVictory:
            case BattleTriggerType.OnBattleLoss:
                battleManager.OnBattleEnded -= HandleBattleEnded;
                break;
        }
    }

    #region Event Handlers

    private void HandleBattleStateChanged(BattleState state)
    {
        switch (triggerType)
        {
            case BattleTriggerType.OnBattleStart:
                if (state == BattleState.BattleStart || state == BattleState.PlayerTurn)
                {
                    FireTrigger();
                }
                break;

            case BattleTriggerType.OnFirstPlayerTurn:
                if (state == BattleState.PlayerTurn && !hasSeenPlayerTurn)
                {
                    hasSeenPlayerTurn = true;
                    FireTrigger();
                }
                break;

            case BattleTriggerType.OnFirstEnemyTurn:
                if (state == BattleState.EnemyTurn && !hasSeenEnemyTurn)
                {
                    hasSeenEnemyTurn = true;
                    FireTrigger();
                }
                break;
        }
    }

    private void HandleQTEWindowStart(bool isActive)
    {
        if (isActive && !hasSeenQTEWindow)
        {
            hasSeenQTEWindow = true;
            FireTrigger();
        }
    }

    private void HandleParryWindowActive(bool isActive)
    {
        if (isActive && !hasSeenParryWindow)
        {
            hasSeenParryWindow = true;
            FireTrigger();
        }
    }

    private void HandleStaminaChanged(float current, float max)
    {
        if (!hasSeenLowStamina)
        {
            float percentage = current / max;
            if (percentage <= staminaThreshold)
            {
                hasSeenLowStamina = true;
                FireTrigger();
            }
        }
    }

    private void HandleBattleEnded(BattleResult result)
    {
        switch (triggerType)
        {
            case BattleTriggerType.OnBattleVictory:
                if (result == BattleResult.PlayerVictory)
                {
                    FireTrigger();
                }
                break;

            case BattleTriggerType.OnBattleLoss:
                if (result == BattleResult.PlayerDefeated)
                {
                    FireTrigger();
                }
                break;
        }
    }

    #endregion

    #region Helper Methods

    private QTEManager FindQTEManager()
    {
        // Buscar en el BattleManager usando reflexión o como componente
        return FindFirstObjectByType<QTEManager>();
    }

    private ParrySystem FindParrySystem()
    {
        return FindFirstObjectByType<ParrySystem>();
    }

    private PlayerBattleController FindPlayerBattleController()
    {
        return FindFirstObjectByType<PlayerBattleController>();
    }

    #endregion
}

