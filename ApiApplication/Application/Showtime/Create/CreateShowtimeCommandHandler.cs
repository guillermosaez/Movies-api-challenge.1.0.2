using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiApplication.Database.Entities;
using ApiApplication.Database.Repositories.Abstractions;
using ApiApplication.Infrastructure.Grpc.MoviesApi;
using MediatR;
using ProtoDefinitions;

namespace ApiApplication.Application.Showtime.Create;

public class CreateShowtimeCommandHandler : IRequestHandler<CreateShowtimeCommand, CreateShowtimeResponse>
{
    private readonly IApiClientGrpc _apiClientGrpc;
    private readonly IShowtimesRepository _showtimesRepository;

    public CreateShowtimeCommandHandler(IApiClientGrpc apiClientGrpc, IShowtimesRepository showtimesRepository)
    {
        _apiClientGrpc = apiClientGrpc;
        _showtimesRepository = showtimesRepository;
    }
    
    public async Task<CreateShowtimeResponse> Handle(CreateShowtimeCommand request, CancellationToken cancellationToken)
    {
        var movie = await GetMovieAsync(request.MovieId);
        var showtime = new ShowtimeEntity
        {
            AuditoriumId = request.AuditoriumId,
            SessionDate = request.SessionDate,
            Movie = new MovieEntity
            {
                Title = movie.Title,
                Stars = movie.Crew,
                ReleaseDate = new DateTime(int.Parse(movie.Year), 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        };
        await _showtimesRepository.CreateShowtimeAsync(showtime, cancellationToken);
        
        return new() { Id = showtime.Id };
    }

    private async Task<showResponse> GetMovieAsync(string movieId)
    {
        var allMovies = await _apiClientGrpc.GetAllAsync();
        var movie = allMovies?.Shows?.FirstOrDefault(m => m.Id == movieId); //A client query filtering by movieId would probably be better in terms of performance.
        if (movie is null) throw new MovieNotFoundException();
        
        return movie;
    }
}