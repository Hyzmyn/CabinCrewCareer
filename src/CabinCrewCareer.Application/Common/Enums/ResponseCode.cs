using System.ComponentModel;

namespace CabinCrewCareer.Application.Common.Enums
{
    public enum ResponseCode
    {
        #region Common

        [Description("Success")]
        SUCCESS = 0,

        [Description("Fail")]
        FAIL = 1,

        [Description("Refresh token is invalid")]
        AUTH_ERR_REFRESH_TOKEN = 2,

        [Description("Refresh token is invalid")]
        INVALID_CREDENTIALS = 3,

        [Description("Refresh token is invalid")]
        INACTIVE_USER = 4,

        [Description("User not found")]
        USER_NOT_FOUND = 5,

        [Description("Disabled successfully")]
        DISABLED_SUCCESS = 6,

        [Description("Updated successfully")]
        UPDATED_SUCCESS = 7,

        [Description("Failed authentication")]
        FAILED_AUTHENTICATION = 8,

        [Description("Invalid file size")]
        INVALID_FILE_SIZE = 9,

        [Description("Invalid file extension")]
        INVALID_FILE_EXTENSION = 10,

        [Description("Updated unsuccessfully")]
        UPDATED_UNSUCC = 11,

        [Description("Created unsuccessfuly")]
        CREATED_UNSUCC = 12,

        [Description("Created successfuly")]
        CREATED_SUCCESS = 13,

        #endregion
    }
}
