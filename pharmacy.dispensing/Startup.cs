using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pharmacy.Dispensing.Models;
using Pharmacy.Models;
using Pharmacy.Models.Pocos;
using Pharmacy.Repositories;
using Pharmacy.Repositories.Interfaces;

namespace Pharmacy.Dispensing
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
            services.AddDbContext<PharmacyContext>(options => options.UseSqlServer(Configuration["ConnectionStrings:Entities"]));
            services.AddTransient<IUnitOfWork, UnitOfWork>();

            services.Configure<ServiceSettings>(Configuration.GetSection("ServiceSettings"));

            var config = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CustomerPoco, Customer>();
                cfg.CreateMap<OrderPoco, Order>();
                cfg.CreateMap<DrugPoco, Drug>();
                cfg.CreateMap<ReminderPoco, Reminder>();

                cfg.CreateMap<Customer, CustomerPoco>();
                cfg.CreateMap<Order, OrderPoco>();
                cfg.CreateMap<Drug, DrugPoco>();
                cfg.CreateMap<Reminder, ReminderPoco>();

                cfg.CreateMap<Shop, ShopPoco>();
                cfg.CreateMap<Doctor, DoctorPoco>();
                cfg.CreateMap<Practice, PracticePoco>();
                cfg.CreateMap<OrderLine, OrderLinePoco>();
            });

            var mapper = config.CreateMapper();
            services.AddSingleton(mapper);

            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddAzureAd(options => Configuration.Bind("AzureAd", options))
            .AddCookie();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
