﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetbook.Contracts.V1;
using Tweetbook.Contracts.V1.Reponses;
using Tweetbook.Contracts.V1.Requests;
using Tweetbook.Domain;
using Tweetbook.Extensions;
using Tweetbook.Services;

namespace Tweetbook.Controllers.V1
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PostsController : Controller
    {
        private readonly IPostService _postService;
        public PostsController(IPostService postService)
        {
            _postService = postService;
        }
        [HttpGet(ApiRoutes.Posts.GetAll)]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await _postService.GetPostsAsync());
        }
        [HttpGet(ApiRoutes.Posts.Get)]
        public async Task<IActionResult> GetAsync([FromRoute] Guid postId)
        {
            var post = await _postService.GetPostByIdAsync(postId);
            if (post == null)
            {
                return NotFound();
            }
            return Ok(post);
        }

        [HttpPut(ApiRoutes.Posts.Update)]
        public async Task<IActionResult> UpdateAsync([FromRoute] Guid postId, [FromBody] UpdatePostRequest request)
        {
            var userOwnPost = await _postService.UserOwnsPostAsync(postId, HttpContext.GetUserId());
            if (!userOwnPost)
            {
                return BadRequest(new { error = "you dont own this post" });
            }

            var post = await _postService.GetPostByIdAsync(postId);
            post.Name = request.Name;
            var updated = await _postService.UpdatePostAsync(post);
            if (updated)
            {
                return Ok(post);
            }
            return NotFound();
        }

        [HttpDelete(ApiRoutes.Posts.Delete)]
        public async Task<IActionResult> DeleteAsync([FromRoute] Guid postId)
        {
            var userOwnPost = await _postService.UserOwnsPostAsync(postId, HttpContext.GetUserId());
            if (!userOwnPost)
            {
                return BadRequest(new { error = "you dont own this post" });
            }

            var deleted = await _postService.DeletePostAsync(postId);
            if (deleted)
            {
                return NoContent();
            }
            return NotFound();
        }


        [HttpPost(ApiRoutes.Posts.Create)]
        public async Task<IActionResult> CreateAsync([FromBody] CreatePostRequest postRequest)
        {
            var post = new Post
            {

                Name = postRequest.Name,
                UserId = HttpContext.GetUserId()
            };
            if (post.Id != Guid.Empty)
            {
                post.Id = Guid.NewGuid();
            }
            await _postService.CreatePostAsync(post);

            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.ToUriComponent()}";
            var locationUri = baseUrl + "/" + ApiRoutes.Posts.Get.Replace("{postId}", post.Id.ToString());

            var response = new PostResponse { Id = post.Id };
            return Created(locationUri, response);
        }

    }
}
