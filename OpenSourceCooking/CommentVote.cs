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
    
    public partial class CommentVote
    {
        public long CommentId { get; set; }
        public short VoteValue { get; set; }
        public string VoterId { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual Comment Comment { get; set; }
    }
}
