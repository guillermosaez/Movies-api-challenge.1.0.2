using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiApplication.Database.Entities;
using ApiApplication.Database.Repositories.Abstractions;
using MediatR;

namespace ApiApplication.Application.Tickets.CreateReservation;

public class BookSeatsCommandHandler : IRequestHandler<BookSeatsCommand, BookSeatsResponse>
{
    private readonly ITicketsRepository _ticketsRepository;
    private readonly IShowtimesRepository _showtimesRepository;
    private readonly IAuditoriumsRepository _auditoriumsRepository;

    public BookSeatsCommandHandler(ITicketsRepository ticketsRepository, IShowtimesRepository showtimesRepository, IAuditoriumsRepository auditoriumsRepository)
    {
        _ticketsRepository = ticketsRepository;
        _showtimesRepository = showtimesRepository;
        _auditoriumsRepository = auditoriumsRepository;
    }
    
    public async Task<BookSeatsResponse> Handle(BookSeatsCommand request, CancellationToken cancellationToken)
    {
        ValidateContiguousSeats(request.Seats);
        var showtimeTickets = await _ticketsRepository.GetEnrichedAsync(request.ShowtimeId, cancellationToken);
        ValidateSeatsAreNotBooked(showtimeTickets, request.Seats);
        ValidateAlreadyPaidSeats(showtimeTickets, request.Seats);

        var showtime = await _showtimesRepository.GetWithMoviesByIdAsync(request.ShowtimeId, cancellationToken);
        var createdTicket = await CreateTicketAsync(request.Seats, showtime, cancellationToken);
        return BuildResponse(createdTicket, showtime);
    }

    private void ValidateContiguousSeats(IEnumerable<SeatDto> seatsToBeBooked)
    {
        var distinctRows = seatsToBeBooked.Select(s => s.Row).Distinct().Count();
        if (distinctRows > 1)
        {
            throw new NonContiguousSeatsException();
        }

        var minSeatNumber = seatsToBeBooked.Min(s => s.Number);
        var numberOfSeats = seatsToBeBooked.Count();

        var theoricalContiguousSeats = Enumerable.Range(minSeatNumber, numberOfSeats).Select(n => (short)n);
        
        var areRequestedSeatsContiguous = theoricalContiguousSeats.Count() == seatsToBeBooked.Count() 
                                          && !seatsToBeBooked.Select(s => s.Number).Except(theoricalContiguousSeats).Any();
        if (!areRequestedSeatsContiguous)
        {
            throw new NonContiguousSeatsException();
        }
    }

    private void ValidateSeatsAreNotBooked(IEnumerable<TicketEntity> existingTickets, IEnumerable<SeatDto> seatsToBeBooked)
    {
        var pendingReservations = existingTickets.Where(t => t.IsPendingToBePaid);
        if (!pendingReservations.Any())
        {
            return;
        }

        var isAnySeatAlreadyBooked = (from existingTicketSeat in pendingReservations.SelectMany(r => r.Seats.Select(s => s.Seat))
                                      join seatToBeBooked in seatsToBeBooked 
                                          on new { existingTicketSeat.Row, existingTicketSeat.SeatNumber } equals new { seatToBeBooked.Row, SeatNumber = seatToBeBooked.Number }
                                      select new
                                      {
                                          Row = existingTicketSeat.Row,
                                          SeatNumber = existingTicketSeat.SeatNumber
                                      }).Any();

        if (isAnySeatAlreadyBooked)
        {
            throw new AlreadyBookedSeatsException();
        }
    }
    
    private void ValidateAlreadyPaidSeats(IEnumerable<TicketEntity> existingTickets, IEnumerable<SeatDto> seatsToBeBooked)
    {
        var paidTickets = existingTickets.Where(t => t.Paid);
        if (!paidTickets.Any())
        {
            return;
        }

        var isAnySeatAlreadyPaid = (from existingTicketSeat in paidTickets.SelectMany(r => r.Seats.Select(s => s.Seat))
                                    join seatToBeBooked in seatsToBeBooked
                                        on new { existingTicketSeat.Row, existingTicketSeat.SeatNumber } equals new { seatToBeBooked.Row, SeatNumber = seatToBeBooked.Number }
                                    select new
                                    {
                                        Row = existingTicketSeat.Row,
                                        SeatNumber = existingTicketSeat.SeatNumber
                                    }).Any();
        if (isAnySeatAlreadyPaid)
        {
            throw new AlreadyPaidSeatsException();
        }
    }

    private async Task<TicketEntity> CreateTicketAsync(IEnumerable<SeatDto> seats, ShowtimeEntity showtime, CancellationToken cancellationToken)
    {
        var seatsToBeBooked = await BuildTicketSeatsAsync(showtime.AuditoriumId, seats, cancellationToken);
        return await _ticketsRepository.CreateAsync(showtime, seatsToBeBooked, cancellationToken);
    }

    private async Task<List<TicketSeatEntity>> BuildTicketSeatsAsync(int auditoriumId, IEnumerable<SeatDto> seats, CancellationToken cancellationToken)
    {
        var auditorium = await _auditoriumsRepository.GetAsync(auditoriumId, cancellationToken);
        
        var seatsToBeAdded = new List<TicketSeatEntity>();
        foreach (var seat in seats)
        {
            var auditoriumSeat = auditorium.Seats.FirstOrDefault(s => s.Row == seat.Row && s.SeatNumber == seat.Number);
            if (auditoriumSeat is null)
            {
                throw new AuditoriumSeatNotExistentException();
            }
            seatsToBeAdded.Add(new() { Seat = auditoriumSeat });
        }
        return seatsToBeAdded;
    }
    
    private static BookSeatsResponse BuildResponse(TicketEntity createdTicket, ShowtimeEntity showtime)
    {
        return new()
        {
            TicketId = createdTicket.Id,
            BookedSeatNumbers = createdTicket.Seats.Select(s => s.Seat.SeatNumber),
            AuditoriumId = showtime.AuditoriumId,
            MovieId = showtime.Movie.Id
        };
    }
}