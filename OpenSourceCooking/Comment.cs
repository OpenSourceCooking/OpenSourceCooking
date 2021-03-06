//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OpenSourceCooking
{
    using System;
    using System.Collections.Generic;
    
    public partial class Comment
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Comment()
        {
            this.Comments1 = new HashSet<Comment>();
            this.CommentVotes = new HashSet<CommentVote>();
        }
    
        public long Id { get; set; }
        public string Text { get; set; }
        public string CreatorId { get; set; }
        public System.DateTime CreateDateUtc { get; set; }
        public Nullable<System.DateTime> EditDateUtc { get; set; }
        public Nullable<long> ParentCommentId { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Comment> Comments1 { get; set; }
        public virtual Comment Comment1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CommentVote> CommentVotes { get; set; }
        public virtual Recipe Recipe { get; set; }
    }
}
