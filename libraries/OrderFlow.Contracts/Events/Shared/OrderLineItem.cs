namespace OrderFlow.Contracts.Events.Shared;

public record OrderLineItem(
    string Sku,
    int Quantity);