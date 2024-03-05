// using ApiApplication;
// using ApiApplication.Infrastructure.Grpc.MoviesApi;
// using Grpc.Core;
// using Microsoft.Extensions.Configuration;
// using Moq;
// using ProtoDefinitions;
//
// namespace ApiApplicationTests.Infrastructure.Grpc.MoviesApi;
//
// public class ApiClientGrpcTests
// {
//     private readonly Mock<ProtoDefinitions.MoviesApi.MoviesApiClient> _moviesApiClientMock = new();
//     private readonly Mock<IConfiguration> _configurationMock = new();
//
//     private ApiClientGrpc _sut => new(_moviesApiClientMock.Object, _configurationMock.Object);
//
//     [Fact]
//     public async Task GetAll_When_requested_Then_result_is_as_expected()
//     {
//         //Arrange
//         var apiKey = "ApiKey";
//         _configurationMock.Setup(c => c[ConfigurationKeyNames.MoviesApi.Key]).Returns(apiKey);
//
//         var mockedResponse = CallHelpers.CreateAsyncUnaryCall(new responseModel());
//         _moviesApiClientMock
//             .Setup(m => m.GetAllAsync(It.IsAny<Empty>(), It.Is<Metadata>(h => h.Count == 1 && h[0].Key == "X-Apikey" && h[0].Value == apiKey), null, default)) //Test not working, it fails when it calls GetAllAsync
//             .Returns(mockedResponse);
//
//         //Act
//         var result = await _sut.GetAllAsync();
//         
//         //Assert
//         Assert.NotNull(result); //TODO: More exhaustive assertion.
//     }
// }