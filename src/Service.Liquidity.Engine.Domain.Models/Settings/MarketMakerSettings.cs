namespace Service.Liquidity.Engine.Domain.Models.Settings
{
    public class MarketMakerSettings
    {
        public EngineMode Mode { get; set; }


    }

    public enum EngineMode
    {
        Disabled,
        Idle,
        Active
    }
}