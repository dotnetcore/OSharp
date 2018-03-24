﻿// -----------------------------------------------------------------------
//  <copyright file="UserController.cs" company="OSharp开源团队">
//      Copyright (c) 2014-2018 OSharp. All rights reserved.
//  </copyright>
//  <site>http://www.osharp.org</site>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2018-03-10 16:15</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using OSharp.AspNetCore.UI;
using OSharp.Collections;
using OSharp.Demo.Identity;
using OSharp.Demo.Identity.Dtos;
using OSharp.Demo.Identity.Entities;
using OSharp.Entity;
using OSharp.Filter;
using OSharp.Identity;
using OSharp.Mapping;


namespace OSharp.Demo.WebApi.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Description("管理-用户信息")]
    public class UserController : AreaApiController
    {
        private readonly UserManager<User> _userManager;
        private readonly IIdentityContract _identityContract;

        public UserController(UserManager<User> userManager, IIdentityContract identityContract)
        {
            _userManager = userManager;
            _identityContract = identityContract;
        }

        [HttpPost]
        [Description("读取")]
        public IActionResult Read()
        {
            PageRequest request = new PageRequest(Request);
            Expression<Func<User, bool>> predicate = FilterHelper.GetExpression<User>(request.FilterGroup);
            var page = _userManager.Users.ToPage(predicate,
                request.PageCondition,
                m => new
                {
                    m.Id,
                    m.UserName,
                    m.Email,
                    m.EmailConfirmed,
                    m.PhoneNumber,
                    m.PhoneNumberConfirmed,
                    m.LockoutEnabled,
                    m.LockoutEnd,
                    m.AccessFailedCount,
                    m.IsLocked,
                    m.CreatedTime,
                    Roles = _identityContract.UserRoles.Where(n => n.UserId == m.Id)
                        .SelectMany(n => _identityContract.Roles.Where(o => o.Id == n.RoleId).Select(o => o.Name))
                });

            return Json(page.ToPageData());
        }

        [HttpPost]
        [Description("新增")]
        public async Task<IActionResult> Create(UserInputDto[] dtos)
        {
            Check.NotNull(dtos, nameof(dtos));
            List<string> names = new List<string>();
            foreach (var dto in dtos)
            {
                User user = dto.MapTo<User>();
                IdentityResult result = dto.Password.IsMissing()
                    ? await _userManager.CreateAsync(user)
                    : await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    return Json(result.ToOperationResult().ToAjaxResult());
                }
                names.Add(user.UserName);
            }
            return Json(new AjaxResult($"用户“{names.ExpandAndToString()}”创建成功", AjaxResultType.Success));
        }

        [HttpPost]
        [Description("更新")]
        public async Task<IActionResult> Update(UserInputDto[] dtos)
        {
            Check.NotNull(dtos, nameof(dtos));
            List<string> names = new List<string>();
            foreach (var dto in dtos)
            {
                User user = await _userManager.FindByIdAsync(dto.Id.ToString());
                user = dto.MapTo(user);
                IdentityResult result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return Json(result.ToOperationResult().ToAjaxResult());
                }
                names.Add(user.UserName);
            }
            return Json(new AjaxResult($"用户“{names.ExpandAndToString()}”更新成功", AjaxResultType.Success));
        }

        [HttpPost]
        [Description("删除")]
        public async Task<IActionResult> Delete(int[] ids)
        {
            Check.NotNull(ids, nameof(ids));
            List<string> names = new List<string>();
            foreach (int id in ids)
            {
                User user = await _userManager.FindByIdAsync(id.ToString());
                IdentityResult result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    return Json(result.ToOperationResult().ToAjaxResult());
                }
                names.Add(user.UserName);
            }
            return Json(new AjaxResult($"用户“{names.ExpandAndToString()}”删除成功", AjaxResultType.Success));
        }
    }
}