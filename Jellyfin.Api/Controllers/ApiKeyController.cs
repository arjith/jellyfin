using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Jellyfin.Api.Constants;
using MediaBrowser.Common.Api;
using MediaBrowser.Controller.Security;
using MediaBrowser.Model.Querying;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Api.Controllers;

[Route("Auth")]
[Tags("Authentication")]
public class ApiKeyController : BaseJellyfinApiController
{
    private readonly IAuthenticationManager _authenticationManager;

    public ApiKeyController(IAuthenticationManager authenticationManager)
    {
        _authenticationManager = authenticationManager;
    }

    [HttpGet("Keys")]
    [Authorize(Policy = Policies.RequiresElevation)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<QueryResult<AuthenticationInfo>>> GetKeys()
    {
        var keys = await _authenticationManager.GetApiKeys().ConfigureAwait(false);
        return new QueryResult<AuthenticationInfo>(keys);
    }

    /// <summary>Creates a new API key and returns its persisted metadata.</summary>
    [HttpPost("Keys")]
    [Authorize(Policy = Policies.RequiresElevation)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthenticationInfo>> CreateKey([FromQuery, Required] string app)
    {
        return await _authenticationManager.CreateApiKey(app).ConfigureAwait(false);
    }

    [HttpDelete("Keys/{key}")]
    [Authorize(Policy = Policies.RequiresElevation)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> RevokeKey([FromRoute, Required] string key)
    {
        await _authenticationManager.DeleteApiKey(key).ConfigureAwait(false);
        return NoContent();
    }
}
