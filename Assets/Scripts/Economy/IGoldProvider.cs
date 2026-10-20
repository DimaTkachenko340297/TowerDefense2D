using System;

public interface IGoldProvider
{
    int CurrentGold { get; }
    event Action<int> OnGoldChanged;
}
