using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Api.Controllers;

public abstract class CurrentUserControllerBase : ControllerBase
{
    protected Guid? GetUserId()
    {
        return Guid.Parse("11111111-1111-1111-1111-111111111111");
    }
}
