using Asp.Versioning;
using Microsoft.OpenApi;
using RetailCoreEcommerce.API.Shared;
using RetailCoreEcommerce.Persistence.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RetailCoreEcommerce.Contracts.Settings;
using RetailCoreEcommerce.Contracts.Shared;

namespace RetailCoreEcommerce.API;

public static class Startup
{
    public static void Configure(this WebApplicationBuilder builder)
    {
        builder.Services.AddPersistence(builder.Configuration);
        builder.ConfigureControllers();
        builder.ConfigureHealthChecks();
        builder.ConfigureApiVersioning();
        builder.ConfigureSwagger("PennyEcommerce", "v1");
        builder.ConfigureAuthentication();
    }

    /// <summary>
    ///     Configures the application request pipeline
    /// </summary>
    public static void Configure(this WebApplication app)
    {
        app.UseErrorHandling();

        // app.UseCorrelationId();
        // app.UseRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "PennyEcommerce v1");
                c.RoutePrefix = "swagger";
            });
        }
        else
        {
            app.UseHsts();
        }

        app.UseRouting();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();


        app.MapControllers();
        app.MapHealthChecks("/health");
    }


    public static void ConfigureAuthentication(this WebApplicationBuilder builder)
    {
        var jwtSettings = builder.Configuration.GetSection(JwtSettings.Section).Get<JwtSettings>() ??
                          throw new Exception("JwtSettings are not configured");

        // RSA (Rivest-Shamir-Adleman) is an asymmetric encryption algorithm
        // In JWT context, we use RSA for digital signatures:
        // - The private key (kept secret) is used to sign tokens
        // - The public key (shared openly) is used to verify the signature
        // This is more secure than symmetric algorithms (HMAC) because the verification
        // public key doesn't need to be kept secret
        // var rsa = RSA.Create();

        // Get the RSA public key directly from the service

        var rsaPublicKey = jwtSettings.PublicKeyBytes.ReadRsaKeyBase64();

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new RsaSecurityKey(rsaPublicKey),
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtSettings.Audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        RoleClaimType = "role"
                    };
                }
            );
        builder.Services.AddAuthorization();
    }

    public static void ConfigureSwagger(this WebApplicationBuilder builder, string serviceName, string version)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(opt =>
        {
            opt.SwaggerDoc(version, new OpenApiInfo
            {
                Title = $"{serviceName} Service",
                Version = version,
                Description = $"{serviceName} Ecommerce API documentation"
            });
            opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                BearerFormat = "JWT",
                Description = "JWT Authorization header using the Bearer scheme.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer"
            });

            opt.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("bearer", document)] = []
            });
        });
    }

    public static void ConfigureApiVersioning(this WebApplicationBuilder builder)
    {
        // Read API versioning settings from configuration
        var settings = builder.Configuration
            .GetSection(ApiVersioningSettings.Section)
            .Get<ApiVersioningSettings>() ?? new ApiVersioningSettings();

        // Configure API versioning based on settings
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(settings.DefaultMajor, settings.DefaultMinor);
            options.AssumeDefaultVersionWhenUnspecified = settings.AssumeDefaultWhenUnspecified;

            // Report API versions in response headers if enabled in settings
            options.ReportApiVersions = settings.ReportApiVersions;

            // Configure how the API version is read from incoming requests based on settings
            options.ApiVersionReader = settings.Reader switch
            {
                "Header" => new HeaderApiVersionReader("x-api-version"),
                "Query" => new QueryStringApiVersionReader("api-version"),
                _ => new UrlSegmentApiVersionReader()
            };
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV"; // v1, v1.0, v2
            options.SubstituteApiVersionInUrl = true; // replace {version} in routes
        });
    }

    public static void ConfigureHealthChecks(this WebApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks();
    }

    public static void ConfigureControllers(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
    }

    public static void UseErrorHandling(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}