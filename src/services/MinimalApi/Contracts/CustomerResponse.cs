﻿namespace MinimalApi.Contracts;

public class CustomerResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedOnUtc { get; set; }

}
