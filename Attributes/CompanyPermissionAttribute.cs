using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using UniversalReservationMVC.Extensions;
using UniversalReservationMVC.Repositories;

namespace UniversalReservationMVC.Attributes
{
    /// <summary>
    /// Authorization filter that checks if the user has specific company member permissions
    /// </summary>
    public class CompanyPermissionAttribute : TypeFilterAttribute
    {
        public CompanyPermissionAttribute(CompanyPermissionType permission) 
            : base(typeof(CompanyPermissionFilter))
        {
            Arguments = new object[] { permission };
        }
    }

    public enum CompanyPermissionType
    {
        ManageResources,
        ViewReservations,
        ManageReservations,
        ManageEvents,
        ViewAnalytics,
        ExportReports,
        ManageMembers
    }

    public class CompanyPermissionFilter : IAsyncAuthorizationFilter
    {
        private readonly CompanyPermissionType _requiredPermission;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CompanyPermissionFilter> _logger;

        public CompanyPermissionFilter(
            CompanyPermissionType requiredPermission,
            IUnitOfWork unitOfWork,
            ILogger<CompanyPermissionFilter> logger)
        {
            _requiredPermission = requiredPermission;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            
            // Check if user is authenticated
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var userId = user.GetCurrentUserId();
            if (userId == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Admins and global Owners bypass permission checks
            if (user.IsInRole("Admin") || user.IsInRole("Owner"))
            {
                return;
            }

            // Get companyId from route or query
            int? companyId = null;
            if (context.RouteData.Values.TryGetValue("companyId", out var routeCompanyId))
            {
                companyId = Convert.ToInt32(routeCompanyId);
            }
            else if (context.HttpContext.Request.Query.TryGetValue("companyId", out var queryCompanyId))
            {
                companyId = Convert.ToInt32(queryCompanyId);
            }

            // If we have a companyId, check member permissions
            if (companyId.HasValue)
            {
                var member = await _unitOfWork.CompanyMembers.GetMemberAsync(companyId.Value, userId);
                
                if (member == null || !member.IsActive)
                {
                    _logger.LogWarning("User {UserId} attempted to access company {CompanyId} without membership", 
                        userId, companyId.Value);
                    context.Result = new ForbidResult();
                    return;
                }

                // Check specific permission
                var hasPermission = _requiredPermission switch
                {
                    CompanyPermissionType.ManageResources => member.CanManageResources,
                    CompanyPermissionType.ViewReservations => member.CanViewReservations,
                    CompanyPermissionType.ManageReservations => member.CanManageReservations,
                    CompanyPermissionType.ManageEvents => member.CanManageEvents,
                    CompanyPermissionType.ViewAnalytics => member.CanViewAnalytics,
                    CompanyPermissionType.ExportReports => member.CanExportReports,
                    CompanyPermissionType.ManageMembers => member.CanManageMembers,
                    _ => false
                };

                if (!hasPermission)
                {
                    _logger.LogWarning("User {UserId} attempted to access {Permission} in company {CompanyId} without permission", 
                        userId, _requiredPermission, companyId.Value);
                    context.Result = new ForbidResult();
                    return;
                }
            }
            else
            {
                // No companyId - check if user is member of ANY company with this permission
                var userCompanies = await _unitOfWork.CompanyMembers.GetUserCompaniesAsync(userId);
                
                var hasPermissionInAnyCompany = userCompanies.Any(member => 
                {
                    if (!member.IsActive) return false;
                    
                    return _requiredPermission switch
                    {
                        CompanyPermissionType.ManageResources => member.CanManageResources,
                        CompanyPermissionType.ViewReservations => member.CanViewReservations,
                        CompanyPermissionType.ManageReservations => member.CanManageReservations,
                        CompanyPermissionType.ManageEvents => member.CanManageEvents,
                        CompanyPermissionType.ViewAnalytics => member.CanViewAnalytics,
                        CompanyPermissionType.ExportReports => member.CanExportReports,
                        CompanyPermissionType.ManageMembers => member.CanManageMembers,
                        _ => false
                    };
                });

                if (!hasPermissionInAnyCompany)
                {
                    _logger.LogWarning("User {UserId} attempted to access {Permission} without permission in any company", 
                        userId, _requiredPermission);
                    context.Result = new ForbidResult();
                }
            }
        }
    }
}
