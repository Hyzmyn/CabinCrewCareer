using System.ComponentModel;

namespace CabinCrewCareer.Application.Common.Enums
{
    public enum ResponseCode
    {
        #region Common

        [Description("Success")]
        SUCCESS = 0,

        [Description("Refresh token is invalid")]
        AUTH_ERR_REFRESH_TOKEN = 1,

        [Description("Refresh token is invalid")]
        INVALID_CREDENTIALS = 2,

        [Description("Refresh token is invalid")]
        INACTIVE_USER = 3,
        #endregion
    }
}
