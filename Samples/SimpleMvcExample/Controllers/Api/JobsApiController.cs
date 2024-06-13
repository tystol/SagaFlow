using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleMvcExample.ResourceProviders;

namespace SimpleMvcExample.Controllers.Api;

[ApiController]
public class JobsApiController: ControllerBase
{
    private readonly JobsResourceProvider _jobsResourceProvider;

    public JobsApiController(JobsResourceProvider jobsResourceProvider)
    {
        _jobsResourceProvider = jobsResourceProvider;
    }
    [HttpGet("api/jobs")]
    [Authorize(Roles = "Admin")]
    public async Task<IEnumerable<Job>> Get()
    {
        return await _jobsResourceProvider.GetAll();
    }
}