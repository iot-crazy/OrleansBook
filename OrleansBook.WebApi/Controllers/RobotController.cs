using Microsoft.AspNetCore.Mvc;
using Orleans;
using OrleansBook.GrainInterfaces;

namespace OrleansBook.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class RobotController : ControllerBase
{
    private readonly IClusterClient Client;

	public RobotController(IClusterClient client)
	{
		Client = client;
	}

	[HttpGet]
	[Route("robot/{name}/instruction")]
	public Task<string> Get(string name)
	{
		var grain = Client.GetGrain<IRobotGrain>(name);
		return grain.GetNextInstruction();
	}

	[HttpPost]
	[Route("robot/{name}/instruction")]
	public async Task<IActionResult> Post(string name, string value)
	{
		var grain = Client.GetGrain<IRobotGrain>(name);
		await grain.AddInstruction(value);
		return Ok();
	}
}