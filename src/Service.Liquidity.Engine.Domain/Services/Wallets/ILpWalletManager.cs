namespace Service.Liquidity.Engine.Domain.Services.Wallets
{
    public interface ILpWalletManager
    {
        ILpWallet GetWalletByLiquidityProvider(string lpName);
    }
}