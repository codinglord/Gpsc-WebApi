//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GpscWebApi
{
    using System;
    using System.Collections.Generic;
    
    public partial class EnergyGenYearly
    {
        public int Id { get; set; }
        public int PlantId { get; set; }
        public decimal Energy_Value { get; set; }
        public System.DateTime Energy_LatestUpdate { get; set; }
    
        public virtual Plant Plant { get; set; }
    }
}