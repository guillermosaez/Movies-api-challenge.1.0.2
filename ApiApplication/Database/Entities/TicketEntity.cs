using System;
using System.Collections.Generic;

namespace ApiApplication.Database.Entities;

public class TicketEntity
{
    public Guid Id { get; set; }
    public int ShowtimeId { get; set; }
    public ICollection<TicketSeatEntity> Seats { get; set; }
    public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
    public bool Paid { get; set; }
    public ShowtimeEntity Showtime { get; set; }

    public bool IsExpired => DateTime.UtcNow.Subtract(CreatedTime) > TimeSpan.FromMinutes(10);
    public bool IsPendingToBePaid => !Paid && !IsExpired;
}