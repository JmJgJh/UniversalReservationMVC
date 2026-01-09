using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using UniversalReservationMVC.Models;
using UniversalReservationMVC.Repositories;

namespace UniversalReservationMVC.Services
{
    public class CompanyMemberService : ICompanyMemberService
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly ICompanyMemberRepository _memberRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CompanyMemberService> _logger;

        public CompanyMemberService(
            ICompanyRepository companyRepository,
            ICompanyMemberRepository memberRepository,
            UserManager<ApplicationUser> userManager,
            ILogger<CompanyMemberService> logger)
        {
            _companyRepository = companyRepository;
            _memberRepository = memberRepository;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<List<CompanyMember>> GetCompanyMembersAsync(int companyId)
        {
            return await _memberRepository.GetCompanyMembersAsync(companyId);
        }

        public async Task<(bool Success, string Message)> AddMemberByEmailAsync(int companyId, string email, string role, bool canManageResources, bool canManageReservations)
        {
            var company = await _companyRepository.GetByIdAsync(companyId);
            if (company == null)
            {
                return (false, "Firma nie istnieje");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return (false, "Użytkownik o podanym e-mailu nie istnieje");
            }

            var exists = await _memberRepository.IsMemberAsync(companyId, user.Id);
            if (exists)
            {
                return (false, "Użytkownik jest już przypisany do firmy");
            }

            var member = new CompanyMember
            {
                CompanyId = companyId,
                UserId = user.Id,
                Role = string.IsNullOrWhiteSpace(role) ? "Employee" : role,
                CanManageResources = canManageResources,
                CanManageReservations = canManageReservations,
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _memberRepository.AddAsync(member);
            await _memberRepository.SaveAsync();

            _logger.LogInformation("User {UserId} added to company {CompanyId} as {Role}", user.Id, companyId, member.Role);
            return (true, "Użytkownik został przypisany do firmy");
        }

        public async Task<bool> RemoveMemberAsync(int companyId, string userId)
        {
            await _memberRepository.RemoveMemberAsync(companyId, userId);
            return true;
        }

        public async Task<bool> IsMemberAsync(int companyId, string userId)
        {
            return await _memberRepository.IsMemberAsync(companyId, userId);
        }
    }
}
