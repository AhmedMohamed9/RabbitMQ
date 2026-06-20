
using Producer.services;
using Shared.Configuration;

namespace Producer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var rabbitMQConfig = builder.Configuration.GetSection(nameof(RabbitMQConfiguration)).Get<RabbitMQConfiguration>();
            builder.Services.AddSingleton(rabbitMQConfig);
            builder.Services.AddScoped<IRabbitMQService, RabbitMQService>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
