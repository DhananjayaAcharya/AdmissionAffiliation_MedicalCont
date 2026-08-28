using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class EventCategory
{
    public int EventCategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? ColorCode { get; set; }

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}
