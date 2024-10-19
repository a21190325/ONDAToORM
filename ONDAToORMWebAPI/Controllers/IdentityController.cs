using Contracts.Dtos;
using Contracts.Dtos.Shared;
using Contracts.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ONDAToORMWebAPI.Controllers
{
    [ApiController]
    public class IdentityController : ControllerBase
    {
        [HttpPost]
        [Route("api/[controller]/token")]
        [AllowAnonymous]
        public async Task<IActionResult> GenerateToken(
            [FromServices] IGenerateTokenService generateTokenService,
            [FromBody] GenerateTokenInputDto request)
        {
            ResultDto<GenerateTokenOutputDto> result = await generateTokenService.ExecuteServiceAsync(request);
            return Ok(result);
        }
    }
}
