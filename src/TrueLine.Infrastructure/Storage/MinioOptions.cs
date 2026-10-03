namespace TrueLine.Infrastructure.Storage;

public class MinioOptions
{
    public string Endpoint { get; set; } = "localhost:9000";

    public string AccessKey { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public string Bucket { get; set; } = "trueline-news";

    public bool UseSsl { get; set; }
}
