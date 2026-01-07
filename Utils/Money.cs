namespace StoreManagement.Utils
{
    public struct Money
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency = "USD")
        {
            Amount = amount;
            Currency = currency;
        }

        public override string ToString()
        {
            return $"{Amount:C} ({Currency})";
        }

        public static Money operator +(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new System.ArgumentException("Currencies must match");
            
            return new Money(a.Amount + b.Amount, a.Currency);
        }

        public static Money operator -(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new System.ArgumentException("Currencies must match");
            
            return new Money(a.Amount - b.Amount, a.Currency);
        }
    }
}
