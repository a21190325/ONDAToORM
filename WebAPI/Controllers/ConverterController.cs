using Contracts.Dtos.Shared;
using Contracts.Dtos;
using Contracts.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text;

namespace WebAPI.Controllers
{
	[ApiController]
	[Authorize]
	public class ConverterController : ControllerBase
	{
		[HttpPost]
		[Route("api/[controller]/convert")]
		public async Task<IActionResult> Convert(
			[FromServices] IConverterService converterService,
			[FromBody] ConverterInputDto inputDto)
		{
			ResultDto<List<FileDto>> result = await converterService.ExecuteServiceAsync(inputDto);
			return Ok(result);
		}
	}
}
