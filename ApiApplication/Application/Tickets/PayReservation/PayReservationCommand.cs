using System;
using MediatR;

namespace ApiApplication.Application.Tickets.PayReservation;

public class PayReservationCommand : IRequest<Unit>
{
    public Guid TicketId { get; init; }
}