﻿using System;
using System.Collections.Generic;

namespace Quark_Backend.Entities;

public partial class Department
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<JobPosition> JobPositions { get; set; } = new List<JobPosition>();
}
