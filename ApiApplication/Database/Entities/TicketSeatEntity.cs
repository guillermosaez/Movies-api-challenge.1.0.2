using System;

namespace ApiApplication.Database.Entities;

public class TicketSeatEntity
{
    public Guid TicketId { get; set; }
    public TicketEntity Ticket { get; set; }
    public int SeatId { get; set; }
    public SeatEntity Seat { get; set; }
}