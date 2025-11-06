using FluentValidation;

namespace CabinCrewCareer.Application.CampaignRequests.Commands.UpdateRejectedCampaignRequest
{
    public class UpdateRejectedCampaignRequestCommandValidator : AbstractValidator<UpdateRejectedCampaignRequestCommand>
    {
        public UpdateRejectedCampaignRequestCommandValidator()
        {
            RuleFor(x => x.RequestId)
                .GreaterThan(0)
                .WithMessage("Request ID phải lớn hơn 0");

            RuleFor(x => x.CampaignName)
                .NotEmpty()
                .WithMessage("Tên chiến dịch không được để trống")
                .MaximumLength(200)
                .WithMessage("Tên chiến dịch không được vượt quá 200 ký tự");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("Mô tả không được để trống")
                .MaximumLength(1000)
                .WithMessage("Mô tả không được vượt quá 1000 ký tự");

            RuleFor(x => x.TargetQuantity)
                .GreaterThan(0)
                .WithMessage("Số lượng mục tiêu phải lớn hơn 0");
        }
    }
}
