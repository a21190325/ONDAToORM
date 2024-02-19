using Contracts.Dtos.Shared;
using Contracts.Dtos;
using Contracts.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace WebAPI.Controllers
{
	[ApiController]
	[Authorize]
    public class ConverterController : ControllerBase
	{
        [HttpPost]
		[Route("api/[controller]/convert")]
		public async Task<IActionResult> Converter(
            [FromServices] IConverterService converterService,
            [FromBody] ConverterInputDto request)
        {
            ResultDto<string> result = await converterService.ExecuteServiceAsync(request);
            return Ok(result);
        }
    }
}
