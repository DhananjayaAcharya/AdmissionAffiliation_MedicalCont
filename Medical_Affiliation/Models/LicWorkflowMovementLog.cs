using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class LicWorkflowMovementLog
{
    public int MovementId { get; set; }

    public int ApprovalId { get; set; }

    public string? FromStage { get; set; }

    public string ToStage { get; set; } = null!;

    public int? FromUserId { get; set; }

    public int? ToUserId { get; set; }

    public string ActionType { get; set; } = null!;

    public string? Remarks { get; set; }

    public int ActionByUserId { get; set; }

    public string? ActionByDesignation { get; set; }

    public DateTime ActionAt { get; set; }
}
