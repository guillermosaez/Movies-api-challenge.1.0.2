using ApiApplication;
using ApiApplication.Application.Showtime.Create;
using ApiApplication.Database.Entities;
using ApiApplication.Database.Repositories.Abstractions;
using ApiApplication.Exceptions;
using Moq;
using ProtoDefinitions;

namespace ApiApplicationTests.Application.Showtime.Create;

public class CreateShowtimeCommandHandlerTests
{
    private readonly Mock<IApiClientGrpc> _apiClientGrpcMock = new();
    private readonly Mock<IShowtimesRepository> _showtimesRepositoryMock = new();

    private CreateShowtimeCommandHandler _sut => new(_apiClientGrpcMock.Object, _showtimesRepositoryMock.Object);

    [Fact]
    public async Task Handle_When_movie_doesnt_exist_Then_notfound_exception_is_thrown()
    {
        //Arrange
        var command = new CreateShowtimeCommand
        {
            MovieId = "NonExistentMovie"
        };
        _apiClientGrpcMock.Setup(a => a.GetAll()).ReturnsAsync(new showListResponse());

        //Act
        var action = () => _sut.Handle(command, default);

        //Assert
        await Assert.ThrowsAsync<NotFoundException>(action);
    }
    
    [Fact]
    public async Task Handle_When_movie_exists_Then_showtime_is_created()
    {
        //Arrange
        var command = new CreateShowtimeCommand
        {
            AuditoriumId = 1,
            MovieId = "ExistentMovie",
            SessionDate = DateTime.UtcNow
        };
        var movies = new List<showResponse>
        {
            new()
            {
                Id = command.MovieId,
                Title = "TestMovie",
                Crew = "People who have worked in the movie",
                Year = "2024"
            }
        };
        _apiClientGrpcMock.Setup(a => a.GetAll()).ReturnsAsync(new showListResponse
        {
            Shows = { movies }
        });
        const int newShowtimeId = 123;
        _showtimesRepositoryMock
            .Setup(r => r.CreateShowtimeAsync(It.Is<ShowtimeEntity>(s => s.Id == default
                    && s.AuditoriumId == command.AuditoriumId
                    && s.SessionDate == command.SessionDate
                    && s.Movie.Id == default
                    && s.Movie.Title == movies[0].Title
                    && s.Movie.Stars == movies[0].Crew
                    && s.Movie.ReleaseDate == new DateTime(int.Parse(movies[0].Year), 1, 1, 0, 0, 0, DateTimeKind.Utc)),
                default))
            .Callback<ShowtimeEntity, CancellationToken>((showtimeEntity, _) => showtimeEntity.Id = newShowtimeId);

        //Act
        var result = await _sut.Handle(command, default);

        //Assert
        Assert.Equal(newShowtimeId, result.Id);
    }
}