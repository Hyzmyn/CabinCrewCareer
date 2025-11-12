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

        [Description("Created successfuly")]
        CREATED_SUCCESS = 2,

        [Description("Created unsuccessfuly")]
        CREATED_UNSUCC = 3,

        [Description("Updated successfully")]
        UPDATED_SUCCESS = 4,

        [Description("Updated unsuccessfully")]
        UPDATED_UNSUCC = 5,

        [Description("Disabled successfully")]
        DISABLED_SUCCESS = 6,

        [Description("Disabled unsuccessfully")]
        DISABLED_UNSUCC = 7,

        [Description("Refresh token is invalid")]
        AUTH_ERR_REFRESH_TOKEN = 8,

        [Description("Incorrect Username or Password")]
        INVALID_CREDENTIALS = 9,

        [Description("Failed authentication")]
        FAILED_AUTHENTICATION = 10,

        [Description("Invalid file size")]
        INVALID_FILE_SIZE = 11,

        [Description("Invalid file extension")]
        INVALID_FILE_EXTENSION = 12,

        [Description("Inactive account. Please contact Admin for further information")]
        INACTIVE_USER = 13,

        [Description("User not found")]
        USER_NOT_FOUND = 14,

        [Description("Campaign request not found")]
        CAMPAIGN_REQUEST_NOT_FOUND = 15,

        [Description("Ward not found")]
        WARD_NOT_FOUND = 16,

        [Description("Unauthorized. Only Examiner can perform this action")]
        UNAUTHORIZED_EXAMINER_ONLY = 17,

        [Description("Test not found")]
        TEST_NOT_FOUND = 18,

        [Description("Duplicate join code. Please try again")]
        DUPLICATE_JOIN_CODE = 19,

        [Description("Campaign Round not found")]
        CAMPAIGN_ROUND_NOT_FOUND = 20,

        #endregion
    }
}
