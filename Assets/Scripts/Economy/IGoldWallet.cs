public interface IGoldWallet
{
    bool AddGold(int amount);
    bool TakeGold(int amount);
    bool HasEnoughGold(int amount);

}