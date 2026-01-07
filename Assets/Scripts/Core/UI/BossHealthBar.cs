namespace Core.UI
{
    public class BossHealthBar : StateBar_HUD
    {
        protected override void SetPercentText()
        {
            // percentText.text = (targetFillAmount * 100f).ToString("f2") + "%";
            percentText.text = targetFillAmount.ToString("P2");
        }
    
    }
}
