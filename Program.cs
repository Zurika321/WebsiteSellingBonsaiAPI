﻿using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WebsiteSellingBonsaiAPI.DTOS.User;
using WebsiteSellingBonsaiAPI.Models;

namespace WebsiteSellingBonsaiAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Tên policy dùng cho cấu hình CORS
            var MyPolicies = "_Policies";
            var builder = WebApplication.CreateBuilder(args);

            // Cấu hình các dịch vụ cần thiết cho ứng dụng
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    // Hỗ trợ xử lý vòng lặp trong JSON
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
                });

            // Thêm công cụ Swagger để hỗ trợ API documentation
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "WebsiteSellingBonsaiAPI",
                    Version = "v1",
                    Description = "API for managing Bonsai products"
                });

                // Cấu hình Swagger để sử dụng JWT Bearer
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT token"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        new string[] {}
                    }
                });
            });

            // Cấu hình CORS để cho phép truy cập từ tất cả các nguồn
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: MyPolicies,
                    builder =>
                    {
                        builder.SetIsOriginAllowed(_ => true) // Cho phép tất cả nguồn gốc
                            .AllowAnyHeader()                // Cho phép tất cả header
                            .AllowCredentials()              // Cho phép gửi cookie/credentials
                            .AllowAnyMethod();               // Cho phép tất cả HTTP method
                    });
            });

            // Cấu hình kết nối cơ sở dữ liệu
            builder.Services.AddDbContext<MiniBonsaiDBAPI>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            );

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            );

            // Cấu hình Authorization với các chính sách vai trò
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOrUser", policy =>
                     policy.RequireAssertion(context =>
                         context.User.IsInRole("Admin") ||
                         context.User.IsInRole("User")
                     )
                );
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
            });

            // Thêm dịch vụ Identity để quản lý người dùng và vai trò
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Cấu hình JWT Authentication
            var jwtSettings = builder.Configuration.GetSection("JWT");
            var secretKey = Encoding.UTF8.GetBytes(jwtSettings["Secret"]);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["ValidIssuer"],
                    ValidAudience = jwtSettings["ValidAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(secretKey)
                };
            });

            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserIdClaimType = "sub";
            });


            // Đăng ký AuthService làm dịch vụ
            builder.Services.AddScoped<IAuthService, AuthService>();

            var app = builder.Build();

            // Sử dụng Swagger trong môi trường phát triển
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Cấu hình middleware trong ứng dụng
            app.UseRouting(); // Định tuyến
            app.UseHttpsRedirection(); // Tự động chuyển sang HTTPS
            app.UseAuthentication(); // Xác thực người dùng
            app.UseAuthorization(); // Ủy quyền người dùng
            app.UseCors(MyPolicies); // Cấu hình CORS

            // Định nghĩa các endpoint cho controller
            app.MapControllers();

            app.Run(); // Chạy ứng dụng
        }
    }
}
