using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Skyworks.Backend.Models;

namespace Skyworks.Backend.Services
{
    public interface IOSOCommentService
    {
        OSOComment AddComment(OSOComment comment);
        OSOComment UpdateCommentStatus(Guid commentId, OSOCommentStatus newStatus);
        List<OSOComment> GetCommentsByOSO(string osoId, string soraVersion);
        List<OSOComment> GetUnresolvedComments();
    }

    public class OSOCommentService : IOSOCommentService
    {
        private readonly ILogger<OSOCommentService> _logger;
        private readonly List<OSOComment> _comments;

        public OSOCommentService(ILogger<OSOCommentService> logger)
        {
            _logger = logger;
            _comments = new List<OSOComment>();
        }

        public OSOComment AddComment(OSOComment comment)
        {
            // Validate SORA version
            if (comment.SoraVersion != "2.0" && comment.SoraVersion != "2.5")
            {
                _logger.LogError($"Invalid SORA version: {comment.SoraVersion}");
                throw new ArgumentException("Invalid SORA version");
            }

            // Validate OSO ID based on SORA version
            ValidateOSOId(comment.OsoId, comment.SoraVersion);

            comment.CreatedAt = DateTime.UtcNow;
            _comments.Add(comment);

            _logger.LogInformation($"Comment added for OSO {comment.OsoId} in SORA {comment.SoraVersion}");
            return comment;
        }

        public OSOComment UpdateCommentStatus(Guid commentId, OSOCommentStatus newStatus)
        {
            var comment = _comments.FirstOrDefault(c => c.CommentId == commentId);
            
            if (comment == null)
            {
                _logger.LogWarning($"Comment not found: {commentId}");
                throw new KeyNotFoundException("Comment not found");
            }

            comment.Status = newStatus;
            comment.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation($"Comment {commentId} status updated to {newStatus}");
            return comment;
        }

        public List<OSOComment> GetCommentsByOSO(string osoId, string soraVersion)
        {
            ValidateOSOId(osoId, soraVersion);

            return _comments
                .Where(c => c.OsoId == osoId && c.SoraVersion == soraVersion)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();
        }

        public List<OSOComment> GetUnresolvedComments()
        {
            return _comments
                .Where(c => c.Status != OSOCommentStatus.Resolved && c.Status != OSOCommentStatus.Closed)
                .OrderByDescending(c => c.Priority)
                .ToList();
        }

        private void ValidateOSOId(string osoId, string soraVersion)
        {
            var validOSOs = soraVersion == "2.0"
                ? new[] { "OSO-10", "OSO-11", "OSO-12", "OSO-14", "OSO-15", "OSO-21", "OSO-22" }
                : new[] { "OSO-11", "OSO-17", "OSO-23" };

            if (!validOSOs.Contains(osoId))
            {
                _logger.LogError($"Invalid OSO {osoId} for SORA {soraVersion}");
                throw new ArgumentException($"Invalid OSO {osoId} for SORA {soraVersion}");
            }
        }
    }
}