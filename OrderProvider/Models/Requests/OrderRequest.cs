using System;

namespace OrderProvider.Models.Requests;

public class OrderRequest
{
    public Guid CustomerId { get; set; }

    public Guid ReservationId { get; set; }
    // this will be changed with klarna fetch
    public bool IsPayed { get; set; }
}
