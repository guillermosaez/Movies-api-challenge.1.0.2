namespace ApiApplication.Application.Tickets.CreateReservation;

public record struct SeatDto
{
    public short Row { get; init; }
    public short Number { get; init; }
}