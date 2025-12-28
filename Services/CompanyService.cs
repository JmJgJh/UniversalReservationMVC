using UniversalReservationMVC.Models;
using UniversalReservationMVC.Repositories;

namespace UniversalReservationMVC.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly ILogger<CompanyService> _logger;

        public CompanyService(ICompanyRepository companyRepository, ILogger<CompanyService> logger)
        {
            _companyRepository = companyRepository;
            _logger = logger;
        }

        public async Task<Company?> GetCompanyByOwnerAsync(string userId)
        {
            try
            {
                return await _companyRepository.GetByOwnerIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting company for owner {UserId}", userId);
                throw;
            }
        }

        public async Task<Company?> GetCompanyByIdAsync(int companyId)
        {
            try
            {
                return await _companyRepository.GetByIdAsync(companyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting company {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<IEnumerable<Company>> GetAllActiveCompaniesAsync()
        {
            try
            {
                return await _companyRepository.GetAllActiveAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all active companies");
                throw;
            }
        }

        public async Task<Company> CreateCompanyAsync(Company company)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(company.Name))
                    throw new ArgumentException("Company name is required");

                company.CreatedAt = DateTime.UtcNow;
                company.IsActive = true;

                await _companyRepository.AddAsync(company);
                await _companyRepository.SaveAsync();

                _logger.LogInformation("Company {CompanyId} created by user {UserId}", company.Id, company.OwnerId);
                return company;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating company for user {UserId}", company.OwnerId);
                throw;
            }
        }

        public async Task<Company> UpdateCompanyAsync(Company company)
        {
            try
            {
                company.UpdatedAt = DateTime.UtcNow;

                _companyRepository.Update(company);
                await _companyRepository.SaveAsync();

                _logger.LogInformation("Company {CompanyId} updated", company.Id);
                return company;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating company {CompanyId}", company.Id);
                throw;
            }
        }

        public async Task<bool> DeleteCompanyAsync(int companyId)
        {
            try
            {
                var company = await _companyRepository.GetByIdAsync(companyId);

                if (company == null)
                    return false;

                company.IsActive = false;
                _companyRepository.Update(company);
                await _companyRepository.SaveAsync();

                _logger.LogInformation("Company {CompanyId} deleted", companyId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting company {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<bool> UserIsCompanyOwnerAsync(string userId, int companyId)
        {
            try
            {
                var company = await _companyRepository.GetByIdAsync(companyId);
                return company != null && company.OwnerId == userId && company.IsActive;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking company ownership");
                throw;
            }
        }
    }
}
