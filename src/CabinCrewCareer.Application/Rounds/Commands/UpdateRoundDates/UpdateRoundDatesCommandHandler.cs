using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Entities;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CabinCrewCareer.Application.Rounds.Commands.UpdateRoundDates
{
    public class UpdateRoundDatesCommandHandler : IRequestHandler<UpdateRoundDatesCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        public UpdateRoundDatesCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateRoundDatesCommand request, CancellationToken cancellationToken)
        {
            var round = await _unitOfWork.Rounds.GetByIdAsync(request.RoundId);
            if (round == null)
                return false;

            // Lấy campaignRound cha
            var campaignRound = await _unitOfWork.CampaignRounds.GetByIdAsync(round.CampaignRoundId);
            if (campaignRound == null)
                return false;

            // Validate: ngày round không vượt phạm vi campaignRound
            if (request.StartDate.HasValue && campaignRound.StartDate.HasValue && request.StartDate.Value < campaignRound.StartDate.Value)
                return false;
            if (request.EndDate.HasValue && campaignRound.EndDate.HasValue && request.EndDate.Value > campaignRound.EndDate.Value)
                return false;

            // Xác định giá trị mới của startdate và enddate sau khi cập nhật (kết hợp giá trị cũ và mới)
            DateTime? newStartDate = request.StartDate.HasValue ? request.StartDate : round.StartDate;
            DateTime? newEndDate = request.EndDate.HasValue ? request.EndDate : round.EndDate;
            if (newStartDate.HasValue && newEndDate.HasValue && newStartDate.Value > newEndDate.Value)
                return false;

            // Validate: không cho sửa ngày đặc biệt cho Screening/Final
            if (round.RoundType.ToString() == "Screening")
            {
                // Chỉ cho phép cập nhật EndDate, không cho phép cập nhật StartDate
                if (request.StartDate.HasValue && campaignRound.StartDate.HasValue && request.StartDate.Value != campaignRound.StartDate.Value)
                    return false;
                if (request.StartDate.HasValue && (!campaignRound.StartDate.HasValue || request.StartDate.Value != campaignRound.StartDate.Value))
                    return false;
            }
            if (round.RoundType.ToString() == "Final")
            {
                // Chỉ cho phép cập nhật StartDate, không cho phép cập nhật EndDate
                if (request.EndDate.HasValue && campaignRound.EndDate.HasValue && request.EndDate.Value != campaignRound.EndDate.Value)
                    return false;
                if (request.EndDate.HasValue && (!campaignRound.EndDate.HasValue || request.EndDate.Value != campaignRound.EndDate.Value))
                    return false;
            }

            // Lấy tất cả rounds cùng campaignRoundId (cùng đợt)
            var siblingRounds = await _unitOfWork.Rounds.GetAllAsync(r => r.CampaignRoundId == round.CampaignRoundId && r.RoundId != round.RoundId);

            // Validate: không trùng ngày với các round khác trong cùng đợt
            foreach (var sibling in siblingRounds)
            {
                if (request.StartDate.HasValue && sibling.StartDate.HasValue && request.StartDate.Value == sibling.StartDate.Value)
                    return false;
                if (request.EndDate.HasValue && sibling.EndDate.HasValue && request.EndDate.Value == sibling.EndDate.Value)
                    return false;
            }

            // Validate: StartDate <= EndDate nếu cả hai cùng có giá trị
            if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate.Value > request.EndDate.Value)
                return false;

            // Validate: enddate của vòng trước phải nhỏ hơn startdate của vòng hiện tại (nếu có)
            // Lấy tất cả rounds trong cùng đợt, sắp xếp theo thứ tự (giả sử theo RoundId tăng dần)
            var allRounds = siblingRounds.ToList();
            allRounds.Add(round); // bao gồm cả round hiện tại
            allRounds = allRounds.OrderBy(r => r.RoundId).ToList();
            int idx = allRounds.FindIndex(r => r.RoundId == round.RoundId);
            if (idx > 0)
            {
                var prev = allRounds[idx - 1];
                // Nếu vòng trước có enddate và vòng hiện tại có startdate
                if (prev.EndDate.HasValue && request.StartDate.HasValue)
                {
                    if (request.StartDate.Value <= prev.EndDate.Value)
                        return false;
                }
            }
            // Ngoài ra, nếu vòng hiện tại có enddate và vòng sau có startdate đã được set, cũng phải đảm bảo không chồng lấn
            if (idx < allRounds.Count - 1)
            {
                var next = allRounds[idx + 1];
                if (request.EndDate.HasValue && next.StartDate.HasValue)
                {
                    if (next.StartDate.Value <= request.EndDate.Value)
                        return false;
                }
            }

            if (request.StartDate.HasValue)
                round.StartDate = request.StartDate;
            if (request.EndDate.HasValue)
                round.EndDate = request.EndDate;
            await _unitOfWork.Rounds.UpdateAsync(round);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
