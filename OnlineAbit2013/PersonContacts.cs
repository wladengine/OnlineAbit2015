//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OnlineAbit2013
{
    using System;
    using System.Collections.Generic;
    
    public partial class PersonContacts
    {
        public System.Guid PersonId { get; set; }
        public int CountryId { get; set; }
        public Nullable<int> RegionId { get; set; }
        public string Phone { get; set; }
        public string Mobiles { get; set; }
        public string Code { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string House { get; set; }
        public string Korpus { get; set; }
        public string Flat { get; set; }
        public string CodeReal { get; set; }
        public string CityReal { get; set; }
        public string StreetReal { get; set; }
        public string HouseReal { get; set; }
        public string KorpusReal { get; set; }
        public string FlatReal { get; set; }
        public string ForeignAddressInfo { get; set; }
        public string KladrCode { get; set; }
    
        public virtual Country Country { get; set; }
        public virtual Person Person { get; set; }
        public virtual Region Region { get; set; }
    }
}
