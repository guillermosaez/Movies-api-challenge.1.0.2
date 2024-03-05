namespace ApiApplication;

public static class ConfigurationKeyNames
{
    public static class MoviesApi
    {
        private const string Base = "MoviesApi";
        public const string Uri = $"{Base}:Uri";
        public const string Key = $"{Base}:Key";
    }

    public static class Redis
    {
        private const string Base = "Redis";
        public const string Url = $"{Base}:Url";
        public const string Port = $"{Base}:Port";
    }
}