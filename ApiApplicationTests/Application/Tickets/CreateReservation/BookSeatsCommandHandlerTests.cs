using ApiApplication.Application.Tickets.CreateReservation;
using ApiApplication.Database.Entities;
using ApiApplication.Database.Repositories.Abstractions;
using Moq;

namespace ApiApplicationTests.Application.Tickets.CreateReservation;

public class BookSeatsCommandHandlerTests
{
    private readonly Mock<ITicketsRepository> _ticketsRepositoryMock = new();
    private readonly Mock<IShowtimesRepository> _showTimesRepositoryMock = new();
    private readonly Mock<IAuditoriumsRepository> _auditorioumsRepositoryMock = new();

    private BookSeatsCommandHandler _sut => new(_ticketsRepositoryMock.Object, _showTimesRepositoryMock.Object, _auditorioumsRepositoryMock.Object);

    [Fact]
    public async Task Handle_When_seats_are_not_same_row_Then_exception_is_thrown()
    {
        //Arrange
        var request = new BookSeatsCommand
        {
            Seats =
            [
                new() { Row = 1 },
                new() { Row = 2 }
            ]
        };

        //Act
        var action = () => _sut.Handle(request, default);

        //Assert
        await Assert.ThrowsAsync<NonContiguousSeatsException>(action);
    }
    
    [Fact]
    public async Task Handle_When_seats_are_not_contiguous_Then_exception_is_thrown()
    {
        //Arrange
        var request = new BookSeatsCommand
        {
            Seats =
            [
                new() { Row = 1, Number = 1 },
                new() { Row = 1, Number = 3 }
            ]
        };

        //Act
        var action = () => _sut.Handle(request, default);

        //Assert
        await Assert.ThrowsAsync<NonContiguousSeatsException>(action);
    }
    
    [Fact]
    public async Task Handle_When_a_seat_was_already_booked_Then_exception_is_thrown()
    {
        //Arrange
        var request = new BookSeatsCommand
        {
            ShowtimeId = 123,
            Seats =
            [
                new() { Row = 1, Number = 1 },
                new() { Row = 1, Number = 2 },
                new() { Row = 1, Number = 3 }
            ]
        };
        var tickets = new List<TicketEntity>
        {
            new() { CreatedTime = DateTime.UtcNow.AddDays(-1) },
            new()
            {
                CreatedTime = DateTime.UtcNow.AddMinutes(-5),
                Seats =
                [
                    new()
                    {
                        Seat = new()
                        {
                            Row = request.Seats.First().Row,
                            SeatNumber = request.Seats.First().Number
                        }
                    }
                ]
            }
        };
        _ticketsRepositoryMock.Setup(t => t.GetEnrichedAsync(request.ShowtimeId, default)).ReturnsAsync(tickets);

        //Act
        var action = () => _sut.Handle(request, default);

        //Assert
        await Assert.ThrowsAsync<AlreadyBookedSeatsException>(action);
    }
    
    [Fact]
    public async Task Handle_When_a_seat_was_already_paid_Then_exception_is_thrown()
    {
        //Arrange
        var request = new BookSeatsCommand
        {
            ShowtimeId = 123,
            Seats =
            [
                new() { Row = 1, Number = 1 },
                new() { Row = 1, Number = 2 },
                new() { Row = 1, Number = 3 }
            ]
        };
        var tickets = new List<TicketEntity>
        {
            new()
            {
                CreatedTime = DateTime.UtcNow.AddMinutes(-5),
                Paid = true,
                Seats =
                [
                    new()
                    {
                        Seat = new()
                        {
                            Row = request.Seats.First().Row,
                            SeatNumber = request.Seats.First().Number
                        }
                    }
                ]
            }
        };
        _ticketsRepositoryMock.Setup(t => t.GetEnrichedAsync(request.ShowtimeId, default)).ReturnsAsync(tickets);

        //Act
        var action = () => _sut.Handle(request, default);

        //Assert
        await Assert.ThrowsAsync<AlreadyPaidSeatsException>(action);
    }
    
    [Fact]
    public async Task Handle_When_seat_doesnt_exist_in_auditorium_Then_exception_is_thrown()
    {
        //Arrange
        var request = new BookSeatsCommand
        {
            ShowtimeId = 123,
            Seats =
            [
                new() { Row = 1, Number = 1 },
                new() { Row = 1, Number = 2 },
                new() { Row = 1, Number = 3 }
            ]
        };
        _ticketsRepositoryMock.Setup(t => t.GetEnrichedAsync(request.ShowtimeId, default)).ReturnsAsync([]);

        var showtime = new ShowtimeEntity
        {
            Id = request.ShowtimeId,
            Movie = new MovieEntity
            {
                Id = 33
            },
            AuditoriumId = 55
        };
        _showTimesRepositoryMock.Setup(s => s.GetWithMoviesByIdAsync(request.ShowtimeId, default)).ReturnsAsync(showtime);

        var auditorium = new AuditoriumEntity
        {
            Seats =
            [
                new() { Row = 1, SeatNumber = 1 },
                new() { Row = 2, SeatNumber = 99 }
            ]
        };
        _auditorioumsRepositoryMock.Setup(a => a.GetAsync(showtime.AuditoriumId, default)).ReturnsAsync(auditorium);

        //Act
        var action = () => _sut.Handle(request, default);

        //Assert
        await Assert.ThrowsAsync<AuditoriumSeatNotExistentException>(action);
        _ticketsRepositoryMock.Verify(t => t.CreateAsync(It.IsAny<ShowtimeEntity>(), It.IsAny<IEnumerable<TicketSeatEntity>>(), default), Times.Never);
    }
    
    [Fact]
    public async Task Handle_When_all_is_correct_Then_seats_are_booked()
    {
        //Arrange
        var request = new BookSeatsCommand
        {
            ShowtimeId = 123,
            Seats =
            [
                new() { Row = 1, Number = 1 },
                new() { Row = 1, Number = 2 },
                new() { Row = 1, Number = 3 }
            ]
        };
        var tickets = new List<TicketEntity>
        {
            new()
            {
                CreatedTime = DateTime.UtcNow.AddDays(-1),
                Seats =
                [
                    new()
                    {
                        Seat = new()
                        {
                            Row = 3,
                            SeatNumber = 4
                        }
                    }
                ]
            },
            new()
            {
                CreatedTime = DateTime.UtcNow.AddMinutes(-5),
                Paid = true,
                Seats =
                [
                    new()
                    {
                        Seat = new()
                        {
                            Row = 4,
                            SeatNumber = 5
                        }
                    }
                ]
            }
        };
        _ticketsRepositoryMock.Setup(t => t.GetEnrichedAsync(request.ShowtimeId, default)).ReturnsAsync(tickets);

        var showtime = new ShowtimeEntity
        {
            Id = request.ShowtimeId,
            Movie = new MovieEntity
            {
                Id = 33
            },
            AuditoriumId = 55
        };
        _showTimesRepositoryMock.Setup(s => s.GetWithMoviesByIdAsync(request.ShowtimeId, default)).ReturnsAsync(showtime);

        var auditorium = new AuditoriumEntity
        {
            Seats =
            [
                new() { Row = 1, SeatNumber = 1 },
                new() { Row = 1, SeatNumber = 2 },
                new() { Row = 1, SeatNumber = 3 }
            ]
        };
        _auditorioumsRepositoryMock.Setup(a => a.GetAsync(showtime.AuditoriumId, default)).ReturnsAsync(auditorium);
        
        
        var createdTicket = new TicketEntity
        {
            Id = Guid.NewGuid(),
            Seats = request.Seats.Select(s => new TicketSeatEntity { Seat = new() { SeatNumber = s.Number }}).ToList()
        };
        _ticketsRepositoryMock
            .Setup(t => t.CreateAsync(showtime, It.Is<IEnumerable<TicketSeatEntity>>(s => s.Count() == request.Seats.Count()
                                                                                        && s.ElementAt(0).Seat.SeatNumber == 1
                                                                                        && s.ElementAt(1).Seat.SeatNumber == 2
                                                                                        && s.ElementAt(2).Seat.SeatNumber == 3),
                default))
            .ReturnsAsync(createdTicket);

        //Act
        var result = await _sut.Handle(request, default);

        //Assert
        Assert.Equal(createdTicket.Id, result.TicketId);
        Assert.All(result.BookedSeatNumbers, number =>
        {
            Assert.Contains(request.Seats, s => s.Number == number);
        });
        Assert.Equal(showtime.AuditoriumId, result.AuditoriumId);
        Assert.Equal(showtime.Movie.Id, result.MovieId);
    }
}