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
    
    public partial class CloudFilesThumbnail
    {
        public int CloudFileId { get; set; }
        public string Url { get; set; }
        public string FileExtension { get; set; }
    
        public virtual CloudFile CloudFile { get; set; }
    }
}
