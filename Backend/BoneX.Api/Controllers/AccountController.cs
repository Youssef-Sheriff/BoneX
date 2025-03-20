﻿using BoneX.Api.Contracts.Users;
using BoneX.Api.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace BoneX.Api.Controllers;

[Route("me")]
[ApiController]
[Authorize]
public class AccountController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    [HttpGet("")]
    public async Task<IActionResult> Info()
    {
        var result = await _userService.GetProfileAsync(User.GetUserId()!);

        return Ok(result.Value);
    }

    [HttpPut("info")]
    public async Task<IActionResult> Info([FromBody] UpdateProfileRequest request)
    {
        await _userService.UpdateProfileAsync(User.GetUserId()!, request);

        return NoContent();
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var result = await _userService.ChangePasswordAsync(User.GetUserId()!, request);

        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPost("profile-picture")]
    public async Task<IActionResult> UploadProfilePicture([FromForm] UploadProfilePictureRequest request)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _userService.UploadProfilePictureAsync(userId, request.ProfilePicture);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new { ProfilePictureUrl = result.Value });
    }
}
