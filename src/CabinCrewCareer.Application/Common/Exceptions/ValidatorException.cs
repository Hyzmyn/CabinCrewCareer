using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;

namespace CabinCrewCareer.Application.Common.Exceptions
{
    public class ValidationException : Exception
    {
        private int _errorCode;

        private List<string> _errors;

        private string _message;

        public ValidationException(ResponseCode responseCode, List<string> errors)
        {
            _errorCode = (int)responseCode;
            _errors = errors;
            _message = responseCode.GetDescription();
        }

        public ValidationException(int errorCode, List<string> errors, string message)
        {
            _errorCode = errorCode;
            _errors = errors;
            _message = message;
        }

        public int ErrorCode => _errorCode;

        public List<string> Errors => _errors;

        public string ErrorMessage => _message;
    }
}
