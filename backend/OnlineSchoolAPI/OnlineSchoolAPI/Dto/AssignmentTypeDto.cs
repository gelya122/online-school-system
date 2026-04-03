namespace OnlineSchoolAPI.Dto;

public class AssignmentTypeDto
{
    public int TypeId { get; set; }
    public string TypeName { get; set; } = null!;
    public string? Description { get; set; }
}

public class CreateAssignmentTypeDto
{
    public string TypeName { get; set; } = null!;
    public string? Description { get; set; }
}

public class UpdateAssignmentTypeDto
{
    public string? TypeName { get; set; }
    public string? Description { get; set; }
}

