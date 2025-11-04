using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Skyworks.Backend.Models;
using Skyworks.Backend.Services;
using System;
using System.Collections.Generic;

namespace Skyworks.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OSOCommentController : ControllerBase
    {
        private readonly IOSOCommentService _commentService;
        private readonly ILogger<OSOCommentController> _logger;

        public OSOCommentController(
            IOSOCommentService commentService, 
            ILogger<OSOCommentController> logger)
        {
            _commentService = commentService;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult AddComment([FromBody] OSOComment comment)
        {
            try
            {
                var addedComment = _commentService.AddComment(comment);
                return CreatedAtAction(nameof(GetCommentsByOSO), 
                    new { osoId = addedComment.OsoId, soraVersion = addedComment.SoraVersion }, 
                    addedComment);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid comment submission");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{osoId}/{soraVersion}")]
        public IActionResult GetCommentsByOSO(string osoId, string soraVersion)
        {
            try
            {
                var comments = _commentService.GetCommentsByOSO(osoId, soraVersion);
                return Ok(comments);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error retrieving comments");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{commentId}/status")]
        public IActionResult UpdateCommentStatus(
            Guid commentId, 
            [FromBody] OSOCommentStatus newStatus)
        {
            try
            {
                var updatedComment = _commentService.UpdateCommentStatus(commentId, newStatus);
                return Ok(updatedComment);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Comment not found");
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpGet("unresolved")]
        public IActionResult GetUnresolvedComments()
        {
            var unresolvedComments = _commentService.GetUnresolvedComments();
            return Ok(unresolvedComments);
        }
    }
}