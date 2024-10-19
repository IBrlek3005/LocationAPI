using FluentValidation;
using LocationServiceAPI.DTOs;

namespace LocationServiceAPI.Validations
{
    public class LocationRequestValidator : AbstractValidator<LocationRequestDTO>
    {
        public LocationRequestValidator()
        {
            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("Zemljopisna širina mora biti između -90 i 90.");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("Zemljopisna dužina mora biti između -180 i 180.");

            RuleFor(x => x.Radius)
                .GreaterThan(0).WithMessage("Radius mora biti veći od 0.");
        }
    }
}
