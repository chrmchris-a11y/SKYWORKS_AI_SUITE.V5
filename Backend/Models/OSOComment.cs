using System;
using System.ComponentModel.DataAnnotations;

namespace Skyworks.Backend.Models
{
    /// <summary>
    /// Σχόλιο για Operational Safety Objective (OSO)
    /// OSO Comment Model supporting SORA 2.0 and 2.5
    /// </summary>
    public class OSOComment
    {
        /// <summary>
        /// Μοναδικό αναγνωριστικό σχολίου
        /// Unique Comment Identifier
        /// </summary>
        [Required]
        public Guid CommentId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Αναγνωριστικό OSO
        /// OSO Identifier
        /// </summary>
        [Required]
        [StringLength(10)]
        public string OsoId { get; set; }

        /// <summary>
        /// Έκδοση SORA
        /// SORA Version (2.0 or 2.5)
        /// </summary>
        [Required]
        [StringLength(3)]
        public string SoraVersion { get; set; }

        /// <summary>
        /// Περιεχόμενο σχολίου
        /// Comment Content
        /// </summary>
        [Required]
        [StringLength(1000)]
        public string Content { get; set; }

        /// <summary>
        /// Συγγραφέας σχολίου
        /// Comment Author
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Author { get; set; }

        /// <summary>
        /// Κατάσταση σχολίου
        /// Comment Status
        /// </summary>
        public OSOCommentStatus Status { get; set; } = OSOCommentStatus.Open;

        /// <summary>
        /// Ημερομηνία δημιουργίας
        /// Creation Date
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Ημερομηνία τελευταίας ενημέρωσης
        /// Last Update Date
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Προτεραιότητα σχολίου
        /// Comment Priority
        /// </summary>
        public OSOCommentPriority Priority { get; set; } = OSOCommentPriority.Low;
    }

    /// <summary>
    /// Κατάσταση σχολίου OSO
    /// OSO Comment Status
    /// </summary>
    public enum OSOCommentStatus
    {
        Open,
        InProgress,
        Resolved,
        Closed
    }

    /// <summary>
    /// Προτεραιότητα σχολίου OSO
    /// OSO Comment Priority
    /// </summary>
    public enum OSOCommentPriority
    {
        Low,
        Medium,
        High,
        Critical
    }
}