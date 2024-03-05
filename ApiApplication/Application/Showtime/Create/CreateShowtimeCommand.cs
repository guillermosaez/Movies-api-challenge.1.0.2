using System;
using MediatR;

namespace ApiApplication.Application.Showtime.Create;

public class CreateShowtimeCommand : IRequest<CreateShowtimeResponse>
{
    public int AuditoriumId { get; set; }
    public DateTime SessionDate { get; set; }
    public string MovieId { get; set; }
}