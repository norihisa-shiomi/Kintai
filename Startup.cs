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
            // 認証クッキーがブラウザに正しく書き込まれるよう設定
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.Lax;
            });

            // クッキー認証の設定（未ログイン時は /Login/Login へリダイレクト）
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Login/Login";
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

            // --- ここから追加 ---
            // ルートアクセス (/) 時に Login.html を自動で開く設定
            var defaultFileOptions = new DefaultFilesOptions();
            defaultFileOptions.DefaultFileNames.Clear();
            defaultFileOptions.DefaultFileNames.Add("Login.html"); // wwwroot/Login.html を既定ファイルに指定
            app.UseDefaultFiles(defaultFileOptions); // 必ず UseStaticFiles より前に呼ぶ
            // --- ここまで追加 ---

            app.UseStaticFiles();
            app.UseCookiePolicy();

            // 認証ミドルウェア (必ず UseMvc の直前に追加)
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                // デフォルトルートを LoginController の Login アクションに設定
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Login}/{action=Login}/{id?}");
            });

            // アプリ起動時に DB 作成とテスト社員の登録を実行
            DbInitializer.Initialize(context);
        }
    }
}