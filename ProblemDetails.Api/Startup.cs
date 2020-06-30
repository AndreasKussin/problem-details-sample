using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ProblemDetails.Api.Infrastructure;
using ProblemDetails.Api.Middleware;
using System.IO;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;


namespace ProblemDetails.Api
{
    public class Startup
    {
        private IConfiguration Configuration { get; }


        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add Application Insights
            services.AddApplicationInsightsTelemetry();

            // Add Http Context to Depedency Injection
            services.AddHttpContextAccessor();

            // Add custom configuration to application insights
            services.AddSingleton<ITelemetryInitializer>(service => new MyTelemetryInitializer(service.GetRequiredService<IHttpContextAccessor>()));

            // Add JWT bearer authentication
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.MetadataAddress = string.Format(Configuration["AadInstance"], Configuration["Policy"]);
                options.Audience = Configuration["ClientId"];
                options.RequireHttpsMetadata = true;
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = JwtEventHelper.AuthenticationFailed,
                    OnTokenValidated = SetPrincipal,
                    OnChallenge = JwtEventHelper.ChallengeFailedResponse,
                    OnForbidden = JwtEventHelper.AuthorizationFailed
                };
            });

            services.AddControllers();

            // Add CORS
            services.AddCors();

            // Add HSTS
            services.AddHsts(options =>
            {
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365.0);
                options.Preload = true;
            });

            // Handle validation errors
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var problemDetails = new ValidationProblemDetails(context.ModelState)
                    {
                        Instance = context.HttpContext.Request.Path,
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://www.my-error-portal.com/myproject/400",
                        Detail = "Please refer to the errors property for additional details."
                    };

                    problemDetails.Extensions.Add("traceId", context.HttpContext.Items["CorrelationId"]);

                    return new BadRequestObjectResult(problemDetails)
                    {
                        ContentTypes = { "application/problem+json" }
                    };
                };
            });

            // Add Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1.0", new OpenApiInfo
                {
                    Version = "v1.0",
                    Title = "ProblemDetails API",
                    Description = "Example API for using RFC 7807 - Problem Details",
                    TermsOfService = null,
                    Contact = new OpenApiContact { Name = "Andreas Kussin", Email = "", Url = new Uri("https://github.com/AndreasKussin") },
                    License = new OpenApiLicense { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") }
                });

                // Swagger JWT Authentication
                var scheme = new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "oauth2",
                    In = ParameterLocation.Header,
                };

                c.AddSecurityDefinition("Bearer", scheme);

                // Set the comments path for the Swagger JSON and UI.
                SetXMLDocumentationPath($"{Assembly.GetExecutingAssembly().GetName().Name}.xml", c);

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { scheme, Array.Empty<string>() }
                });
            });
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            app.UseMiddleware<CorrelationIdMiddleware>();

            if (env.IsDevelopment())
            {
                logger.LogInformation("Use config for development environment.");
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "ProblemDetails API V1.0");
            });
        }


        /// <summary>
        /// Sets the user principal
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        private async Task SetPrincipal(TokenValidatedContext ctx)
        {
            if (ctx.Principal != null && ctx.Principal.Identity.IsAuthenticated)
            {
                try
                {
                    // Set user principal. Replace with your custom logic if needed
                    var identity = ctx.Principal.Identity as ClaimsIdentity;
                    var user = new ClaimsPrincipal(identity);

                    user.AddIdentity(identity);
                    ctx.Principal = user;
                }
                catch (Exception ex)
                {
                    var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<JwtEventHelper>>();
                    logger.LogError(ex, "Setting user principal failed.");
                    ctx.Fail("Setting user principal failed.");
                }
            }
        }


        public void SetXMLDocumentationPath(string fileName, SwaggerGenOptions swaggerOptions)
        {
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                swaggerOptions.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, fileName));
            }
        }
    }
}