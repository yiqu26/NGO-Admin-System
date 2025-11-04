using Microsoft.EntityFrameworkCore;
using NGOPlatformWeb.Models.Entity;

namespace NGOPlatformWeb.Services
{
    public class PasswordMigrationService
    {
        private readonly NGODbContext _context;
        private readonly PasswordService _passwordService;

        public PasswordMigrationService(NGODbContext context, PasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        public async Task MigrateCasePasswordsAsync()
        {
            var caseLogins = await _context.CaseLogins.ToListAsync();
            
            foreach (var caseLogin in caseLogins)
            {
                if (!string.IsNullOrEmpty(caseLogin.Password) && caseLogin.Password.Length < 50)
                {
                    caseLogin.Password = _passwordService.HashPassword(caseLogin.Password);
                    _context.CaseLogins.Update(caseLogin);
                }
            }
            
            await _context.SaveChangesAsync();
        }
    }
}