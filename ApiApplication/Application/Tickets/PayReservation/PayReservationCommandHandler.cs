using System;
using System.Threading;
using System.Threading.Tasks;
using ApiApplication.Database.Entities;
using ApiApplication.Database.Repositories.Abstractions;
using MediatR;

namespace ApiApplication.Application.Tickets.PayReservation;

public class PayReservationCommandHandler : IRequestHandler<PayReservationCommand, Unit>
{
    private readonly ITicketsRepository _ticketsRepository;

    public PayReservationCommandHandler(ITicketsRepository ticketsRepository)
    {
        _ticketsRepository = ticketsRepository;
    }
    
    public async Task<Unit> Handle(PayReservationCommand request, CancellationToken cancellationToken)
    {
        var ticket = await GetTicketAsync(request.TicketId, cancellationToken);
        ValidateTicketIsPendingToBePaid(ticket);
        ValidateTicketIsExpired(ticket);

        await _ticketsRepository.ConfirmPaymentAsync(ticket, cancellationToken);
        return Unit.Value;
    }

    private async Task<TicketEntity> GetTicketAsync(Guid ticketId, CancellationToken cancellationToken)
    {
        var ticket = await _ticketsRepository.GetAsync(ticketId, cancellationToken);
        if (ticket is null)
        {
            throw new TicketNotFoundException();
        }
        return ticket;
    }

    private static void ValidateTicketIsPendingToBePaid(TicketEntity ticket)
    {
        if (ticket.Paid)
        {
            throw new TicketAlreadyPaidException();
        }
    }

    private static void ValidateTicketIsExpired(TicketEntity ticket)
    {
        if (ticket.IsExpired)
        {
            throw new TicketExpiredException();
        }
    }
}