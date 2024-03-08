using System.Collections.Generic;
using MediatR;

namespace ApiApplication.Application.Tickets.CreateReservation;

public class BookSeatsCommand : IRequest<BookSeatsResponse>
{
    public int ShowtimeId { get; init; }
    public IEnumerable<SeatDto> Seats { get; init; }
}