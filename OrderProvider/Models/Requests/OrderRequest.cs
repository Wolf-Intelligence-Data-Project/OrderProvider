using System;

namespace OrderProvider.Models.Requests;

public class OrderRequest
{
    public Guid CustomerId { get; set; }

    public Guid ReservationId { get; set; }

}
