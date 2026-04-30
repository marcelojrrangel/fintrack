using FinTrack.Application.Common.Models;
using FinTrack.Application.Features.Dashboard.Queries;
using FinTrack.Application.Features.Transactions.Commands;
using FinTrack.Application.Features.Transactions.Queries;
using FinTrack.Domain.Enums;
using FinTrack.WebAPI.Controllers;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace FinTrack.Backend.UnitTests.WebAPI;

public sealed class ControllerTests
{
    [Fact]
    public void HealthController_ShouldReturnHealthyResponse()
    {
        var controller = new HealthController();

        var result = controller.Get();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(ApiResponse<object>.Ok(new { application = "FinTrack", status = "Healthy" }, "Service is healthy."));
    }

    [Fact]
    public async Task DashboardController_ShouldReturnOkResponse()
    {
        var sender = Substitute.For<ISender>();
        var dto = new DashboardDto(10m, 5m, 2m, "green");
        sender.Send(Arg.Any<GetDashboardQuery>(), Arg.Any<CancellationToken>()).Returns(dto);
        var controller = new DashboardController(sender);

        var result = await controller.Get(CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task TransactionsController_ShouldHandleReadEndpoints()
    {
        var sender = Substitute.For<ISender>();
        var transaction = new TransactionDto(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Salary", 10m, DateTime.UtcNow, TransactionType.Income, "Salary", false, DateTime.UtcNow, null);
        var pagedResponse = PagedResponse<TransactionDto>.Create(new[] { transaction }, 1, 1, 5);
        sender.Send(Arg.Any<GetTransactionsQuery>(), Arg.Any<CancellationToken>()).Returns(pagedResponse);
        sender.Send(Arg.Any<GetTransactionByIdQuery>(), Arg.Any<CancellationToken>()).Returns(transaction);
        sender.Send(Arg.Any<GetTransactionHistoryQuery>(), Arg.Any<CancellationToken>())
            .Returns(new[] { new TransactionHistoryDto(Guid.NewGuid(), transaction.Id, HistoryActionType.Created, "Created", null, null, DateTime.UtcNow) });
        var controller = new TransactionsController(sender);

        (await controller.GetAll(1, 5, CancellationToken.None)).Result.Should().BeOfType<OkObjectResult>();
        (await controller.GetById(transaction.Id, CancellationToken.None)).Result.Should().BeOfType<OkObjectResult>();
        (await controller.GetHistory(transaction.Id, CancellationToken.None)).Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task TransactionsController_ShouldHandleMutationEndpoints()
    {
        var sender = Substitute.For<ISender>();
        var transaction = new TransactionDto(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Salary", 10m, DateTime.UtcNow, TransactionType.Income, "Salary", false, DateTime.UtcNow, null);
        sender.Send(Arg.Any<CreateTransactionCommand>(), Arg.Any<CancellationToken>()).Returns(transaction);
        sender.Send(Arg.Any<UpdateTransactionCommand>(), Arg.Any<CancellationToken>()).Returns(transaction);
        sender.Send(Arg.Any<DeleteTransactionCommand>(), Arg.Any<CancellationToken>()).Returns(transaction);
        var controller = new TransactionsController(sender);
        var request = new TransactionMutationRequest(transaction.CategoryId, transaction.Amount, transaction.TransactionDateUtc, transaction.Type, transaction.Description);

        var created = await controller.Create(request, CancellationToken.None);
        var updated = await controller.Update(transaction.Id, request, CancellationToken.None);
        var deleted = await controller.Delete(transaction.Id, CancellationToken.None);

        created.Result.Should().BeOfType<CreatedAtActionResult>();
        updated.Result.Should().BeOfType<OkObjectResult>();
        deleted.Result.Should().BeOfType<OkObjectResult>();
    }
}
