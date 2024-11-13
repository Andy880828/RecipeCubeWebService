using System;
using System.Collections.Generic;

namespace RecipeCubeWebService.Models;

public partial class user
{
    public string Id { get; set; } = null!;

    public string? UserName { get; set; }

    public string? NormalizedUserName { get; set; }

    public string? Email { get; set; }

    public string? NormalizedEmail { get; set; }

    public sbyte EmailConfirmed { get; set; }

    public string? PasswordHash { get; set; }

    public string? SecurityStamp { get; set; }

    public string? ConcurrencyStamp { get; set; }

    public string? PhoneNumber { get; set; }

    public sbyte PhoneNumberConfirmed { get; set; }

    public sbyte TwoFactorEnabled { get; set; }

    public DateTime? LockoutEnd { get; set; }

    public sbyte LockoutEnabled { get; set; }

    public int AccessFailedCount { get; set; }

    public sbyte dietary_restrictions { get; set; }

    public sbyte exclusiveChecked { get; set; }

    public int groupId { get; set; }

    public sbyte preferredChecked { get; set; }

    public sbyte status { get; set; }
}
