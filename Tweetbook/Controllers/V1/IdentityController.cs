using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetbook.Contracts.V1;
using Tweetbook.Contracts.V1.Reponses;
using Tweetbook.Contracts.V1.Requests;
using Tweetbook.Services;

namespace Tweetbook.Controllers.V1
{
    public class IdentityController : Controller
    {
        private readonly IIdentityService _identityService;
        public IdentityController(IIdentityService identityService)
        {
            _identityService = identityService;
        }
        [HttpPost(ApiRoutes.Identity.Register)]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthFailedResponse
                {
                    Errors = ModelState.Values.SelectMany(x => x.Errors.Select(xx => xx.ErrorMessage))
                });
            }
            var authReponse = await _identityService.RegisterAsync(request.Email, request.Password);
            if (!authReponse.Success)
            {
                return BadRequest(new AuthFailedResponse
                {
                    Errors = authReponse.Errors
                });
            }
            return Ok(new AuthSuccessReponse
            {
                Token = authReponse.Token
            });
        }

        [HttpPost(ApiRoutes.Identity.Login)]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            var authReponse = await _identityService.LoginAsync(request.Email, request.Password);
            if (!authReponse.Success)
            {
                return BadRequest(new AuthFailedResponse
                {
                    Errors = authReponse.Errors
                });
            }
            return Ok(new AuthSuccessReponse
            {
                Token = authReponse.Token
            });
        }
    }
}
