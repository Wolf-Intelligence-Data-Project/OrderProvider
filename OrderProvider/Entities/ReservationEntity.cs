﻿using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderProvider.Entities;
[Table("Reservations", Schema = "dbo")]
public class ReservationEntity
{
    [Key]
    public Guid ReservationId { get; set; } 

    [Required]
    public Guid CustomerId { get; set; }

    public string? BusinessTypes { get; set; }
    public string? Regions { get; set; }
    public string? CitiesByRegion { get; set; }
    public string? Cities { get; set; }
    public string? PostalCodes { get; set; }

    public int? MinRevenue { get; set; }
    public int? MaxRevenue { get; set; }
    public int? MinNumberOfEmployees { get; set; }
    public int? MaxNumberOfEmployees { get; set; }

    public int Quantity { get; set; }
    public DateTime? ReservedFrom { get; set; }

    public DateTime? SoldFrom { get; set; }


}
