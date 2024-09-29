using DotNetCoreMVCApp.Entity.ViewModels;
using DotNetCoreMVCApp.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace WebAppDemo.Controllers
{
    public class AdministrativeController : Controller
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdministrativeController(RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public async Task<IActionResult> ListRoles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }
        [HttpGet]
        public async Task<IActionResult> CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(RoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var role = new ApplicationRole
                {
                    Name = model.Name,

                };
                var result = await _roleManager.CreateAsync(role);

                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(ListRoles));
                }
            }
            return View();
        }
        ///
        [HttpGet]
        public async Task<IActionResult> AddRemoveRoles(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);

            ViewBag.Id = Id;
            ViewBag.UserName = user.UserName;

            List<ManageRolesViewModel> models = new List<ManageRolesViewModel>();

            foreach (var role in _roleManager.Roles.ToList())
            {
                ManageRolesViewModel manageRolesView = new ManageRolesViewModel()
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    IsSelected = await _userManager.IsInRoleAsync(user, role.Name)

                };

                //if(await _userManager.IsInRoleAsync(user, role.Name))
                //{
                //    manageRolesView.IsSelected = true;
                //}
                

                models.Add(manageRolesView);

            }

            return View(models);
        }

        [HttpPost]
        public async Task<IActionResult> AddRemoveRoles(List<ManageRolesViewModel> model,string UserId)
        {
            var user = await _userManager.FindByIdAsync(UserId);

            ViewBag.Id = UserId;
            ViewBag.UserName = user.UserName;

            bool bFlag =false;

            for(int i=0; i < model.Count();i++)
            {

                IdentityResult result = new IdentityResult();

                if (model[i].IsSelected && !await _userManager.IsInRoleAsync(user, model[i].RoleName))
                {
                   result = await  _userManager.AddToRoleAsync(user, model[i].RoleName);
                }
                else if (!model[i].IsSelected && await _userManager.IsInRoleAsync(user, model[i].RoleName))
                {
                   result = await _userManager.RemoveFromRoleAsync(user, model[i].RoleName);

                }
                
                bFlag= result.Succeeded ? true : false;
                
            }

            if(bFlag) { return RedirectToAction(nameof(ListRoles)); }

            return View(model);
        }
        

    }
}
