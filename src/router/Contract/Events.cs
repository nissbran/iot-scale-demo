namespace MessageRouter.Contract;


public record FaultyBooking(string CreditId, int Value, string Date, int Month)
{
    public string Type => nameof(FaultyBooking);
}

//public record Temperature(string DeviceId, )