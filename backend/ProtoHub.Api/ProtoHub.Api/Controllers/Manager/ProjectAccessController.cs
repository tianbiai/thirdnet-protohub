using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProtoHub.Database;
using ProtoHub.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProtoHub.Api.DTOs;
using ProtoHub.Api.Extensions;
using ProtoHub.Api.Filters;
using ThirdNet.Core.AspNetCore;

namespace ProtoHub.Api.Controllers.Manager
{
    /// <summary>
    /// 项目访问管理控制器
    /// </summary>
    [ApiController]
    [Route("api/manager/project-access")]
    [Authorize]
    public class ProjectAccessController : ControllerBase
    {
        private readonly ProtoHubDbContext _dbContext;

        public ProjectAccessController(ProtoHubDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 获取项目授权列表
        /// </summary>
        [HttpPost("list")]
        [HasPermission("project-access:view")]
        public async Task<IActionResult> GetList([FromBody] ProjectAccessListRequest request)
        {
            var query = from upa in _dbContext.UserProjectAccesses
                        join u in _dbContext.Users on upa.user_id equals u.id
                        join mg in _dbContext.MenuGroups on upa.project_id equals mg.id
                        join grantor in _dbContext.Users on upa.granted_by equals grantor.id into grantorGroup
                        from grantor in grantorGroup.DefaultIfEmpty()
                        select new { upa, u, mg, grantor = grantor ?? new UserModel() };

            // 项目筛选
            if (request.project_id.HasValue)
            {
                query = query.Where(x => x.upa.project_id == request.project_id.Value);
            }

            // 用户筛选
            if (request.user_id.HasValue)
            {
                query = query.Where(x => x.upa.user_id == request.user_id.Value);
            }

            // 关键字搜索
            if (!string.IsNullOrEmpty(request.keyword))
            {
                query = query.Where(x => x.u.user_name.Contains(request.keyword) ||
                                         x.u.nick_name.Contains(request.keyword) ||
                                         x.mg.name.Contains(request.keyword));
            }

            // 分页查询
            var pagedResult = await query
                .OrderByDescending(x => x.upa.create_time)
                .Select(x => new ProjectAccessResponse
                {
                    id = x.upa.id,
                    user_id = x.upa.user_id,
                    user_name = x.u.user_name,
                    nick_name = x.u.nick_name,
                    project_id = x.upa.project_id,
                    project_name = x.mg.name,
                    access_type = x.upa.access_type,
                    granted_by = x.upa.granted_by,
                    granted_by_name = !string.IsNullOrEmpty(x.grantor.nick_name) ? x.grantor.nick_name : (!string.IsNullOrEmpty(x.grantor.user_name) ? x.grantor.user_name : ""),
                    create_time = x.upa.create_time
                })
                .ToPagedResultAsync(request.page, request.page_size);

            return Ok(pagedResult);
        }

        /// <summary>
        /// 授予项目访问权限
        /// </summary>
        [HttpPost("grant")]
        [HasPermission("project-access:grant")]
        public async Task<IActionResult> Grant([FromBody] GrantAccessRequest request)
        {
            // 获取当前用户ID
            var currentUserId = User.GetUserId();

            // 检查用户是否存在
            var user = await _dbContext.Users.AnyAsync(u => u.id == request.user_id);
            if (!user)
            {
                throw new WebApiException(System.Net.HttpStatusCode.NotFound, "用户不存在");
            }

            // 检查项目是否存在
            var project = await _dbContext.MenuGroups.AnyAsync(mg => mg.id == request.project_id);
            if (!project)
            {
                throw new WebApiException(System.Net.HttpStatusCode.NotFound, "项目不存在");
            }

            // 检查是否已授权
            var exists = await _dbContext.UserProjectAccesses
                .AnyAsync(upa => upa.user_id == request.user_id && upa.project_id == request.project_id);

            if (exists)
            {
                throw new WebApiException(System.Net.HttpStatusCode.BadRequest, "该用户已获得此项目的访问权限");
            }

            // 创建授权记录
            var access = new UserProjectAccessModel
            {
                user_id = request.user_id,
                project_id = request.project_id,
                access_type = request.access_type,
                granted_by = currentUserId
            };

            _dbContext.UserProjectAccesses.Add(access);
            await _dbContext.SaveChangesAsync();

            return Ok(new { id = access.id, message = "授权成功" });
        }

        /// <summary>
        /// 撤销项目访问权限
        /// </summary>
        [HttpPost("revoke")]
        [HasPermission("project-access:revoke")]
        public async Task<IActionResult> Revoke([FromBody] RevokeAccessRequest request)
        {
            var access = await _dbContext.UserProjectAccesses.EnsureExistsAsync(request.id, "授权记录");

            _dbContext.UserProjectAccesses.Remove(access);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "撤销成功" });
        }

        /// <summary>
        /// 批量授权
        /// </summary>
        [HttpPost("batch-grant")]
        [HasPermission("project-access:grant")]
        public async Task<IActionResult> BatchGrant([FromBody] BatchGrantAccessRequest request)
        {
            // 获取当前用户ID
            var currentUserId = User.GetUserId();

            // 获取已存在的授权
            var existingAccesses = await _dbContext.UserProjectAccesses
                .Where(upa => request.user_ids.Contains(upa.user_id) && upa.project_id == request.project_id)
                .Select(upa => upa.user_id)
                .ToListAsync();

            // 过滤掉已授权的用户
            var newUserIds = request.user_ids.Except(existingAccesses).ToList();

            // 批量创建授权
            foreach (var userId in newUserIds)
            {
                _dbContext.UserProjectAccesses.Add(new UserProjectAccessModel
                {
                    user_id = userId,
                    project_id = request.project_id,
                    access_type = request.access_type,
                    granted_by = currentUserId
                });
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new { message = $"成功授权 {newUserIds.Count} 个用户", skipped = existingAccesses.Count });
        }

        /// <summary>
        /// 获取用户的项目访问列表
        /// </summary>
        [HttpPost("user/{id}/projects")]
        [HasPermission("project-access:view")]
        public async Task<IActionResult> GetUserProjects(long id)
        {
            var projects = await (from upa in _dbContext.UserProjectAccesses
                                  join mg in _dbContext.MenuGroups on upa.project_id equals mg.id
                                  where upa.user_id == id
                                  select new UserProjectResponse
                                  {
                                      id = upa.id,
                                      project_id = mg.id,
                                      project_name = mg.name,
                                      project_icon = mg.icon,
                                      access_type = upa.access_type,
                                      create_time = upa.create_time
                                  }).ToListAsync();

            return Ok(projects);
        }

        /// <summary>
        /// 获取当前用户可管理的项目
        /// </summary>
        [HttpPost("my-projects")]
        public async Task<IActionResult> GetMyProjects()
        {
            var userId = User.GetUserId();

            var projects = await (from upa in _dbContext.UserProjectAccesses
                                  join mg in _dbContext.MenuGroups on upa.project_id equals mg.id
                                  where upa.user_id == userId && upa.access_type == "manage"
                                  select new UserProjectResponse
                                  {
                                      id = upa.id,
                                      project_id = mg.id,
                                      project_name = mg.name,
                                      project_icon = mg.icon,
                                      access_type = upa.access_type,
                                      create_time = upa.create_time
                                  }).ToListAsync();

            return Ok(projects);
        }

        /// <summary>
        /// 更新访问类型
        /// </summary>
        [HttpPost("update-access-type")]
        [HasPermission("project-access:grant")]
        public async Task<IActionResult> UpdateAccessType([FromBody] UpdateAccessTypeRequest request)
        {
            var access = await _dbContext.UserProjectAccesses.EnsureExistsAsync(request.id, "授权记录");

            access.access_type = request.access_type;
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "更新成功" });
        }
    }

}
