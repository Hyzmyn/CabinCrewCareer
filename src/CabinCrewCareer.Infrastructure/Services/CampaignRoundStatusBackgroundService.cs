using CabinCrewCareer.Application.Common.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.Services
{
    public class CampaignRoundStatusBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<CampaignRoundStatusBackgroundService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromMinutes(1); // Chạy mỗi 1 phút

        public CampaignRoundStatusBackgroundService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<CampaignRoundStatusBackgroundService> logger)
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
                    var campaignRoundStatusService = scope.ServiceProvider.GetRequiredService<ICampaignRoundStatusService>();
                    
                    _logger.LogInformation("Bắt đầu cập nhật status campaign rounds theo thời gian...");
                    await campaignRoundStatusService.UpdateAllCampaignRoundStatusesAsync(stoppingToken);
                    _logger.LogInformation("Hoàn thành cập nhật status campaign rounds");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi cập nhật status campaign rounds: {Message}", ex.Message);
                }

                await Task.Delay(_period, stoppingToken);
            }
        }
    }
}