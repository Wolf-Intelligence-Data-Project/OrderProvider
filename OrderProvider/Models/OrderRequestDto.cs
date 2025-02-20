using System;

namespace OrderProvider.Models
{
    public class OrderRequestDto
    {
        public Guid UserId { get; set; }
        public string FiltersUsed { get; set; }  // Filters applied when ordering
    }

}
