using CabinCrewCareer.Application.Common.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.Services
{
    public class CampaignStatusBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<CampaignStatusBackgroundService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromMinutes(5); // Chạy mỗi 5 phút

        public CampaignStatusBackgroundService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<CampaignStatusBackgroundService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var campaignStatusService = scope.ServiceProvider.GetRequiredService<ICampaignStatusService>();
                    
                    _logger.LogInformation("Bắt đầu cập nhật status campaigns theo thời gian...");
                    await campaignStatusService.UpdateAllCampaignStatusesAsync(stoppingToken);
                    _logger.LogInformation("Hoàn thành cập nhật status campaigns");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi cập nhật status campaigns: {Message}", ex.Message);
                }

                await Task.Delay(_period, stoppingToken);
            }
        }
    }
}