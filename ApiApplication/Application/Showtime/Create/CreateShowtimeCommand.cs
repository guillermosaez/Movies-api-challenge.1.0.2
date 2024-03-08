using System;
using MediatR;

namespace ApiApplication.Application.Showtime.Create;

public class CreateShowtimeCommand : IRequest<CreateShowtimeResponse>
{
    public int AuditoriumId { get; init; }
    public DateTime SessionDate { get; init; }
    public string MovieId { get; init; }
}