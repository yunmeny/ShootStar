using System.Collections;
using Core.UI;
using UnityEngine;

namespace Core.SystemModules
{
    // Created by the Gameplay wave manager, so direct scene entry and Retry use the same path.
    public class RunUpgradeController : Singleton<RunUpgradeController>
    {
        public RunUpgradeState Run { get; } = new RunUpgradeState();
        public CombatBuildState Combat { get; } = new CombatBuildState();
        public string CombatMessage { get; private set; }
        private float messageTime;
        public bool IsChoosing { get; private set; }
        public RunUpgradeDefinition[] Choices { get; private set; }
        private readonly System.Random random = new System.Random();
        private Player player;
        private RunUpgradeScreen screen;
        public const int MaxActiveFragments = 32;
        public int ActiveFragments { get; private set; }

        public bool TryReserveFragment()
        {
            if (ActiveFragments >= MaxActiveFragments) return false;
            ActiveFragments++;
            return true;
        }

        public void ReleaseFragment() => ActiveFragments = Mathf.Max(0, ActiveFragments - 1);

        private void Start() => gameObject.AddComponent<CombatBuildHUD>();

        private void Update()
        {
            if (GameManager.GameState != GameState.Play) return;
            Combat.Tick(Time.deltaTime);
            messageTime -= Time.deltaTime;
            if (messageTime <= 0f) CombatMessage = null;
        }

        public void ReportCombatEvent(string message)
        {
            CombatMessage = message;
            messageTime = 2.5f;
        }

        public IEnumerator ChooseAfterWave(int clearedWave)
        {
            if (GameManager.GameState != GameState.Play || IsChoosing) yield break;
            player = FindFirstObjectByType<Player>();
            if (!player) yield break;
            if (!screen)
            {
                screen = gameObject.AddComponent<RunUpgradeScreen>();
                screen.Initialize(this);
            }

            Choices = Run.RollChoices(random, GameManager.Instance.TestFocus);
            IsChoosing = true;
            TimeController.Instance.BeginUpgradeSelection();
            player.BeginUpgradeSelection();
            screen.Show(clearedWave, Choices, Run);
            yield return new WaitUntil(() => !IsChoosing);
        }

        public void Select(int index)
        {
            if (!IsChoosing || !screen || !screen.CanSelect || GameManager.GameState != GameState.UpgradeSelection ||
                index < 0 || index >= Choices.Length) return;
            var choice = Choices[index];
            if (!Run.TryApply(choice.Id)) return;
            // Close before restoring inputs: duplicate clicks / submits cannot apply twice.
            IsChoosing = false;
            screen.Hide();
            player.RefreshRunUpgrades();
            switch (choice.Id)
            {
                case RunUpgradeId.Repair: player.RestoreHealth(player.MaximumHealth * 0.5f); break;
                case RunUpgradeId.Missiles: player.GetComponent<MissileSystem>().AddMissile(5); break;
                case RunUpgradeId.Energy:
                    PlayerOverdrive.off.Invoke();
                    PlayerEnergy.Instance.AccumulateEnergy(50, false);
                    break;
            }
            if (Run.HasWideWarhead) player.GetComponent<MissileSystem>().AddMissile(2);
            if (UIInput.Instance) UIInput.Instance.DisableUIInput();
            TimeController.Instance.EndUpgradeSelection();
            player.EndUpgradeSelection();
        }

        private void OnDestroy()
        {
            if (!IsChoosing) return;
            IsChoosing = false;
            if (TimeController.Instance) TimeController.Instance.ResetTime();
        }
    }
}
