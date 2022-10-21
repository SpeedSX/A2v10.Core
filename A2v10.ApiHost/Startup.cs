﻿
using A2v10.Web.Identity.ApiKey;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace A2v10.ApiHost;

public class Startup
{
	public const String API_NAME = "A2v10.Api.Host";
	public Startup(IConfiguration configuration)
	{
		Configuration = configuration;
	}

	public IConfiguration Configuration { get; }

	public void ConfigureServices(IServiceCollection services)
	{
		services.AddControllers();

		// Data
		services.AddOptions<DataConfigurationOptions>();
		services.UseSimpleDbContext();
		services.Configure<DataConfigurationOptions>(options =>
		{
			options.ConnectionStringName = "Default";
			options.DisableWriteMetadataCaching = false;
		});

		// Identity
		services.AddPlatformIdentityCore(options =>
		{
		})
		.AddIdentityConfiguration(Configuration);

		// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen(c =>
		{
			c.SwaggerDoc("v1", new OpenApiInfo()
			{
				Title = API_NAME,
				Version = "v1",
				Description = "Api description",
				TermsOfService = new Uri("https://example.com/terms"),
				Contact = new OpenApiContact()
				{
					Name = "Contact1",
					Email = "mail@mail.com",
					Url = new Uri("https://example.com/contacts")
				},
				License = new OpenApiLicense()
				{
					Name = "Example License",
					Url = new Uri("https://example.com/license")
				}
			});

			c.AddSecurityDefinition(ApiKeyAuthenticationOptions.Scheme, ApiKeyAuthenticationOptions.OpenApiSecurityScheme);
			c.AddSecurityRequirement(ApiKeyAuthenticationOptions.OpenApiSecurityRequirement);

			var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
			c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
		});

		services.AddAuthentication(options =>
		{
			options.DefaultAuthenticateScheme = ApiKeyAuthenticationOptions.DefaultScheme;
			options.DefaultChallengeScheme = ApiKeyAuthenticationOptions.DefaultScheme;
		})
		.AddApiKeyAuthorization(options => {
		});

	}

	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
	{

		if (env.IsDevelopment())
		{
		}
		app.UseDeveloperExceptionPage();
		app.UseSwagger();
		app.UseSwaggerUI();

		app.UseHttpsRedirection();

		app.UseAuthentication();

		app.UseRouting();
		app.UseAuthorization();
		app.UseEndpoints(endpoints =>
			endpoints.MapControllers()
		);
	}
}
