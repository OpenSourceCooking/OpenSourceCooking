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
    
    public partial class MeasurementUnit
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public MeasurementUnit()
        {
            this.RecipeStepsIngredients = new HashSet<RecipeStepsIngredient>();
        }
    
        public string MeasurementTypeName { get; set; }
        public string MeasurementUnitName { get; set; }
    
        public virtual MeasurementType MeasurementType { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RecipeStepsIngredient> RecipeStepsIngredients { get; set; }
    }
}
