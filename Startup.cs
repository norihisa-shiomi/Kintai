using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using KintaiSystem.Data;
using KintaiSystem.Models.Entities;
using KintaiSystem.Services;

namespace Kintai
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // false に設定してログイン認証クッキーが常にブラウザに書き込まれるようにする
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.Lax;
            });

            // クッキー認証の設定
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Login/Index"; // 未ログイン時のリダイレクト先
                    options.LogoutPath = "/Login/Logout";
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                });

            // EndpointRouting をオフにして従来型のルーティングを許可する設定
            services.AddMvc(options => options.EnableEndpointRouting = false)
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // 1. SQLite データベースの設定
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite("Data Source=kintai.db"));

            // 2. パスワードハッシュ照合用サービスの登録
            services.AddScoped<IPasswordHasher<Employee>, PasswordHasher<Employee>>();

            // 3. ログインサービスおよび打刻サービスの登録
            services.AddScoped<LoginService>();
            services.AddScoped<AttendanceService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, AppDbContext context)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();

            // 認証ミドルウェア (必ず UseMvc より前)
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Login}/{action=Index}/{id?}"); // 初期画面をログイン画面に設定
            });

            // アプリ起動時に DB 作成とテスト社員の登録を実行
            DbInitializer.Initialize(context);
        }
    }
}