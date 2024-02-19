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
			[FromServices] IConverterService converterService)
		{
			string? rawContent = default;
			using (var contentStream = Request.Body)
			{
				using var sr = new StreamReader(contentStream);
				rawContent = await sr.ReadToEndAsync().ConfigureAwait(false);
			}

			ResultDto<string> result = await converterService.ExecuteServiceAsync(new ConverterInputDto
			{
				SqlContentInBase64 = rawContent
			});
			return Ok(result);
		}
	}
}
