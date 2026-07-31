using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.Menu.Commands;

public class DeleteMenuItemCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
}

public class DeleteMenuItemCommandHandler : IRequestHandler<DeleteMenuItemCommand, ApiResponse<bool>>
{
    private readonly IMenuItemRepository _menuItemRepository;

    public DeleteMenuItemCommandHandler(IMenuItemRepository menuItemRepository)
    {
        _menuItemRepository = menuItemRepository;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteMenuItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _menuItemRepository.GetByIdAsync(request.Id);
        if (item is null)
            return ApiResponse<bool>.Fail("Menu item not found");

        await _menuItemRepository.DeleteAsync(item);
        return ApiResponse<bool>.Ok(true, "Menu item deleted");
    }
}
