using System.Threading;
using System.Threading.Tasks;
using ApiApplication.Application.Showtime.Create;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ApiApplication.Controllers;

[Route("[controller]")]
public class ShowtimesController : ControllerBase
{
    private readonly ISender _sender;

    public ShowtimesController(ISender sender)
    {
        _sender = sender;
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateShowtime([FromBody] CreateShowtimeCommand command, CancellationToken cancellationToken)
    {
        var showTimeCreatedId = await _sender.Send(command, cancellationToken);
        
        return Created(showTimeCreatedId.Id.ToString(), showTimeCreatedId.Id);
    } 
}