using online_school_admin.Models;

namespace online_school_admin.Services;

public sealed class PermissionService
{
    private readonly SessionService _session;

    public PermissionService(SessionService session)
    {
        _session = session;
    }

    public string Role => (_session.CurrentUser?.RoleName ?? "").Trim().ToLowerInvariant();

    public bool IsAdmin => Role == "admin";
    public bool IsManager => Role == "manager";
    public bool IsTeacher => Role == "teacher";

    public bool CanViewSection(string sectionId)
    {
        // Временно отключаем разграничение прав (будет сделано позже и по-другому).
        return true;
    }

    /// <summary>TODO(roles): переназначение менеджера заявки, soft-delete, смена статуса чужих заявок — только admin/руководитель.</summary>
    public bool CanAdministrateApplications => IsAdmin;

    public bool CanEditStreams => true;
    public bool CanEditPromoCodes => true;
    public bool CanEditPayments => true;
    public bool CanEditStudents => true;
    public bool CanEditApplications => true;
    public bool CanEditNotifications => true;
    public bool CanEditSettings => true;
    public bool CanEditEmployees => true;
    public bool CanEditCourses => true;

    public bool CanAssignReviewer => true;
}

