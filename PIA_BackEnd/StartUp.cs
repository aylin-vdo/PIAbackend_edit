﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using System.Text.Json.Serialization;
using PIA_BackEnd.DTOs;
using PIA_BackEnd.Entidades;

namespace PIA_BackEnd
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddDbContext<ApplicationDBContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("defaultConnection")));

            services.AddResponseCaching();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opciones => opciones.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("keyjwt")),
                    ClockSkew = TimeSpan.Zero
                });

            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PIA_BackEnd", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                         new String[]{}
                    }
                });
            });

            services.AddAutoMapper(typeof(Startup));

            MapperConfiguration mappingConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<UsuarioDTO, Usuario>();
            });

            services.AddSingleton(mappingConfig.CreateMapper());

            services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDBContext>()
            .AddDefaultTokenProviders();

            services.AddAuthorization(opciones =>
            {
                opciones.AddPolicy("EsAdmin", politica => politica.RequireClaim("esAdmin"));
                opciones.AddPolicy("EsUsuario", politica => politica.RequireClaim("esUsuario"));
            });

            services.AddCors(opciones =>
            {
                opciones.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins("https://apirequest.io").AllowAnyMethod().AllowAnyHeader();
                    //builder.WithOrigins("https://google.com").AllowAnyMethod().AllowAnyHeader();
                    //
                });
            });
        }

        //, ILogger<Startup> logger
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            //app.UseLoguearRespuestaHTTP();
            // Configure the HTTP request pipeline.
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapControllers();
                endpoints.MapControllerRoute(
                name: "obtenerusuario",
                pattern: "api/usuarios/{id}",
                defaults: new { controller = "Usuario", action = "Get" }
                );
            
            });

        }
    }
}