namespace WHMapper.Services.EveAPI
{
    public static class Extensions
    {
        public static IServiceCollection AddDataPlane(this IServiceCollection services/*, IConfigurationSection section*/)
        {
            //services.Configure<DataPlaneAPIConfig>(section);
            services.AddScoped<IEveAPIServices, EveAPIServices>();

            return services;
        }
    }
}
