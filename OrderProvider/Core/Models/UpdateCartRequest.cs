using System;
using System.Collections.Generic;

namespace OrderProvider.Core.Models
{
    public class UpdateCartRequest
    {
        public List<Guid> ProductIds { get; set; }
    }
}
