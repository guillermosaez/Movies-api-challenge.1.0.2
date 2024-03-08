using System;
using System.Collections.Generic;

namespace ApiApplication.Application.Tickets.CreateReservation;

public record struct BookSeatsResponse
{
    public Guid TicketId { get; init; }
    public IEnumerable<short> BookedSeatNumbers { get; init; }
    public int AuditoriumId { get; init; }
    public int MovieId { get; init; }
}