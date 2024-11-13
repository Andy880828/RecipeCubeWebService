using System;
using System.Collections.Generic;

namespace RecipeCubeWebService.Models;

public partial class user_groups
{
    public int groupId { get; set; }

    public string? groupName { get; set; }

    public string? groupAdmin { get; set; }

    public int? groupInvite { get; set; }
}
