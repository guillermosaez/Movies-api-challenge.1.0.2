using ApiApplication.Application.Tickets.PayReservation;
using ApiApplication.Database.Entities;
using ApiApplication.Database.Repositories.Abstractions;
using MediatR;
using Moq;

namespace ApiApplicationTests.Application.Tickets.PayReservation;

public class PayReservationCommandHandlerTests
{
    private readonly Mock<ITicketsRepository> _ticketsRepositoryMock = new();
    
    private PayReservationCommandHandler _sut => new(_ticketsRepositoryMock.Object);

    [Fact]
    public async Task Handle_When_ticket_doesnt_exist_Then_exception_is_thrown()
    {
        //Arrange
        var request = new PayReservationCommand
        {
            TicketId = Guid.NewGuid()
        };
        
        //Act
        var action = () => _sut.Handle(request, default);

        //Assert
        await Assert.ThrowsAsync<TicketNotFoundException>(action);
        _ticketsRepositoryMock.Verify(t => t.GetAsync(request.TicketId, default), Times.Once);
    }
    
    [Fact]
    public async Task Handle_When_ticket_is_already_paid_Then_exception_is_thrown()
    {
        //Arrange
        var request = new PayReservationCommand
        {
            TicketId = Guid.NewGuid()
        };

        var paidTicket = new TicketEntity { Paid = true };
        _ticketsRepositoryMock.Setup(t => t.GetAsync(request.TicketId, default)).ReturnsAsync(paidTicket);
        
        //Act
        var action = () => _sut.Handle(request, default);

        //Assert
        await Assert.ThrowsAsync<TicketAlreadyPaidException>(action);
    }
    
    [Fact]
    public async Task Handle_When_ticket_is_expired_Then_exception_is_thrown()
    {
        //Arrange
        var request = new PayReservationCommand
        {
            TicketId = Guid.NewGuid()
        };

        var expiredTicket = new TicketEntity { CreatedTime = DateTime.UtcNow.AddDays(-1) };
        _ticketsRepositoryMock.Setup(t => t.GetAsync(request.TicketId, default)).ReturnsAsync(expiredTicket);
        
        //Act
        var action = () => _sut.Handle(request, default);

        //Assert
        await Assert.ThrowsAsync<TicketExpiredException>(action);
    }

    [Fact]
    public async Task Handle_When_ticket_can_be_paid_Then_ticket_is_paid()
    {
        //Arrange
        var request = new PayReservationCommand
        {
            TicketId = Guid.NewGuid()
        };

        var ticket = new TicketEntity { CreatedTime = DateTime.UtcNow.AddSeconds(-30) };
        _ticketsRepositoryMock.Setup(t => t.GetAsync(request.TicketId, default)).ReturnsAsync(ticket);
        
        //Act
        var result = await _sut.Handle(request, default);
        
        //Assert
        Assert.Equal(Unit.Value, result);
        _ticketsRepositoryMock.Verify(t => t.ConfirmPaymentAsync(ticket, default), Times.Once);
    }
}