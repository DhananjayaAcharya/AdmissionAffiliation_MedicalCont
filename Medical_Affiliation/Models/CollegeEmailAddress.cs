using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CollegeEmailAddress
{
    public string FirstName { get; set; } = null!;

    public string LastNameRequired { get; set; } = null!;

    public string EmailAddress { get; set; } = null!;
}
