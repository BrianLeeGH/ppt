using Aliyun.OSS;
using Aliyun.OSS.Common;

namespace SlideBuilder.Api.Errors;

public static class StorageErrorMapper
{
    public static string MapToUserMessage(Exception ex)
    {
        if (ex is OssException ossEx)
        {
            return ossEx.ErrorCode switch
            {
                "NoSuchBucket" => "The configured storage bucket does not exist.",
                "AccessDenied" => "Access to storage was denied. Please check credentials.",
                "EntityTooLarge" => "The file you are trying to upload is too large.",
                _ => $"Storage error: {ossEx.Message}"
            };
        }

        return "An unexpected error occurred while accessing storage.";
    }
}
