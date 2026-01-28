using Microsoft.EntityFrameworkCore;
using NGOPlatformWeb.Models;
using NGOPlatformWeb.Models.Entity;

namespace NGOPlatformWeb.Services
{
    // Background service for cleaning up expired password reset tokens
    public class TokenCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TokenCleanupService> _logger;
        private readonly IConfiguration _configuration;

        public TokenCleanupService(
            IServiceProvider serviceProvider,
            ILogger<TokenCleanupService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Token cleanup service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredTokens();

                    // Get cleanup interval from configuration, default to 24 hours
                    var cleanupIntervalHours = _configuration.GetValue<int>("TokenCleanup:CleanupIntervalHours", 24);
                    var delay = TimeSpan.FromHours(cleanupIntervalHours);

                    _logger.LogInformation($"Next cleanup will run in {delay.TotalHours} hours");
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Token cleanup service stopped");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during token cleanup");

                    // Wait shorter time before retry when error occurs
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
        }

        // Cleanup expired and used tokens from database
        private async Task CleanupExpiredTokens()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<NGODbContext>();

            try
            {
                var retentionDays = _configuration.GetValue<int>("TokenCleanup:RetentionDays", 7);
                var cutoffDate = DateTime.Now.AddDays(-retentionDays);

                // Cleanup conditions:
                // 1. Expired tokens that are beyond retention period
                // 2. Used tokens that are older than 1 day
                var tokensToDelete = await context.PasswordResetTokens
                    .Where(t =>
                        // Expired and beyond retention period
                        (t.ExpiresAt < DateTime.Now && t.CreatedAt < cutoffDate) ||
                        // Used and older than 1 day
                        (t.IsUsed && t.UsedAt != null && t.UsedAt < DateTime.Now.AddDays(-1))
                    )
                    .ToListAsync();

                if (tokensToDelete.Any())
                {
                    context.PasswordResetTokens.RemoveRange(tokensToDelete);
                    await context.SaveChangesAsync();

                    _logger.LogInformation($"Successfully cleaned up {tokensToDelete.Count} expired tokens");

                    // Log cleanup statistics
                    var expiredCount = tokensToDelete.Count(t => t.ExpiresAt < DateTime.Now);
                    var usedCount = tokensToDelete.Count(t => t.IsUsed);

                    _logger.LogInformation($"Cleanup details - Expired: {expiredCount}, Used: {usedCount}");
                }
                else
                {
                    _logger.LogInformation("No tokens to cleanup");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred during token cleanup");
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping token cleanup service...");
            await base.StopAsync(cancellationToken);
        }
    }
}
