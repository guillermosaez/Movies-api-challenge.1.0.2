using System.Threading;
using System.Threading.Tasks;
using ApiApplication.Application.Tickets.CreateReservation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ApiApplication.Controllers;

[Route("[controller]")]
public class TicketsController : ControllerBase
{
    private readonly ISender _sender;

    public TicketsController(ISender sender)
    {
        _sender = sender;
    }
    
    [HttpPost("book")]
    public async Task<IActionResult> BookSeats([FromBody] BookSeatsCommand command, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(command, cancellationToken);
        return Created(response.TicketId.ToString(), response);
    }
}