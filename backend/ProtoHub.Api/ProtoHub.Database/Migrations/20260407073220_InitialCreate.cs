using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProtoHub.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "protohub");

            migrationBuilder.CreateTable(
                name: "t_menu_group",
                schema: "protohub",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "主键ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false, comment: "分组名称"),
                    icon = table.Column<string>(type: "text", nullable: true, comment: "图标"),
                    order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "排序"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "描述"),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()", comment: "创建时间"),
                    update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()", comment: "更新时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_menu_group", x => x.id);
                },
                comment: "菜单分组表（项目表）");

            migrationBuilder.CreateTable(
                name: "t_menu_item",
                schema: "protohub",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "主键ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    group_id = table.Column<long>(type: "bigint", nullable: false, comment: "所属分组ID"),
                    name = table.Column<string>(type: "text", nullable: false, comment: "菜单名称"),
                    type = table.Column<string>(type: "text", nullable: false, defaultValue: "web", comment: "类型（web/miniprogram/doc/link/internal）"),
                    url = table.Column<string>(type: "text", nullable: true, comment: "链接地址"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "描述"),
                    order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "排序"),
                    viewport_config = table.Column<string>(type: "jsonb", nullable: true, comment: "视口配置"),
                    doc_file_id = table.Column<string>(type: "text", nullable: true, comment: "文档文件ID"),
                    doc_file_name = table.Column<string>(type: "text", nullable: true, comment: "文档文件名"),
                    doc_description = table.Column<string>(type: "text", nullable: true, comment: "文档描述"),
                    route = table.Column<string>(type: "text", nullable: true, comment: "内部路由"),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()", comment: "创建时间"),
                    update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()", comment: "更新时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_menu_item", x => x.id);
                },
                comment: "菜单项表（项目子项表）");

            migrationBuilder.CreateTable(
                name: "t_permission",
                schema: "protohub",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "主键ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "text", nullable: false, comment: "权限编码"),
                    name = table.Column<string>(type: "text", nullable: false, comment: "权限名称"),
                    category = table.Column<string>(type: "text", nullable: true, comment: "分类"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "描述")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_permission", x => x.id);
                },
                comment: "权限表");

            migrationBuilder.CreateTable(
                name: "t_role",
                schema: "protohub",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "主键ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "text", nullable: false, comment: "角色编码"),
                    name = table.Column<string>(type: "text", nullable: false, comment: "角色名称"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "描述"),
                    is_system = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "是否系统内置"),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()", comment: "创建时间"),
                    update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()", comment: "更新时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_role", x => x.id);
                },
                comment: "角色表");

            migrationBuilder.CreateTable(
                name: "t_role_permission",
                schema: "protohub",
                columns: table => new
                {
                    role_id = table.Column<long>(type: "bigint", nullable: false, comment: "角色ID"),
                    permission_id = table.Column<long>(type: "bigint", nullable: false, comment: "权限ID"),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()", comment: "创建时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_role_permission", x => new { x.role_id, x.permission_id });
                },
                comment: "角色权限关联表");

            migrationBuilder.CreateTable(
                name: "t_role_system_menu",
                schema: "protohub",
                columns: table => new
                {
                    role_id = table.Column<long>(type: "bigint", nullable: false, comment: "角色ID"),
                    system_menu_id = table.Column<long>(type: "bigint", nullable: false, comment: "系统菜单ID"),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()", comment: "创建时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_role_system_menu", x => new { x.role_id, x.system_menu_id });
                },
                comment: "角色系统菜单关联表");

            migrationBuilder.CreateTable(
                name: "t_system_menu",
                schema: "protohub",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "主键ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    parent_id = table.Column<long>(type: "bigint", nullable: true, comment: "父菜单ID（支持多级）"),
                    name = table.Column<string>(type: "text", nullable: false, comment: "菜单名称"),
                    code = table.Column<string>(type: "text", nullable: false, comment: "菜单编码"),
                    icon = table.Column<string>(type: "text", nullable: true, comment: "图标"),
                    path = table.Column<string>(type: "text", nullable: true, comment: "路由路径"),
                    order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "排序"),
                    is_visible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true, comment: "是否可见"),
                    permission = table.Column<string>(type: "text", nullable: true, comment: "所需权限"),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()", comment: "创建时间"),
                    update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()", comment: "更新时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_system_menu", x => x.id);
                },
                comment: "系统功能菜单表");

            migrationBuilder.CreateTable(
                name: "t_user",
                schema: "protohub",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "主键ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_name = table.Column<string>(type: "text", nullable: false, comment: "用户名"),
                    password = table.Column<string>(type: "text", nullable: false, comment: "密码"),
                    nick_name = table.Column<string>(type: "text", nullable: false, comment: "昵称"),
                    email = table.Column<string>(type: "text", nullable: true, comment: "邮箱"),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1, comment: "状态（0=禁用, 1=启用）"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "描述"),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()", comment: "创建时间"),
                    update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()", comment: "更新时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_user", x => x.id);
                },
                comment: "用户表");

            migrationBuilder.CreateTable(
                name: "t_user_project_access",
                schema: "protohub",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "主键ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false, comment: "用户ID"),
                    project_id = table.Column<long>(type: "bigint", nullable: false, comment: "项目ID（关联t_menu_group.id）"),
                    access_type = table.Column<string>(type: "text", nullable: false, defaultValue: "view", comment: "访问类型（view=查看, manage=管理）"),
                    granted_by = table.Column<long>(type: "bigint", nullable: false, comment: "授权人ID"),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()", comment: "创建时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_user_project_access", x => x.id);
                },
                comment: "用户项目访问表");

            migrationBuilder.CreateTable(
                name: "t_user_role",
                schema: "protohub",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false, comment: "用户ID"),
                    role_id = table.Column<long>(type: "bigint", nullable: false, comment: "角色ID"),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()", comment: "创建时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_user_role", x => new { x.user_id, x.role_id });
                },
                comment: "用户角色关联表");

            migrationBuilder.InsertData(
                schema: "protohub",
                table: "t_permission",
                columns: new[] { "id", "category", "code", "description", "name" },
                values: new object[,]
                {
                    { 1L, "系统", "dashboard", "访问仪表板", "仪表板" },
                    { 2L, "系统", "settings", "系统设置管理", "系统设置" },
                    { 3L, "系统", "user-manage:view", "查看用户列表", "查看用户" },
                    { 4L, "系统", "user-manage:create", "创建新用户", "创建用户" },
                    { 5L, "系统", "user-manage:update", "编辑用户信息", "编辑用户" },
                    { 6L, "系统", "user-manage:delete", "删除用户", "删除用户" },
                    { 7L, "系统", "user-manage:assign-role", "为用户分配角色", "分配用户角色" },
                    { 8L, "系统", "role-manage:view", "查看角色列表", "查看角色" },
                    { 9L, "系统", "role-manage:create", "创建新角色", "创建角色" },
                    { 10L, "系统", "role-manage:update", "编辑角色信息", "编辑角色" },
                    { 11L, "系统", "role-manage:delete", "删除角色", "删除角色" },
                    { 12L, "系统", "role-manage:assign-permission", "为角色分配权限", "分配角色权限" },
                    { 13L, "系统", "permission-manage:view", "查看权限列表", "查看权限" },
                    { 14L, "系统", "system-menu-manage:view", "查看系统菜单列表", "查看系统菜单" },
                    { 15L, "系统", "system-menu-manage:create", "创建系统菜单", "创建系统菜单" },
                    { 16L, "系统", "system-menu-manage:update", "编辑系统菜单", "编辑系统菜单" },
                    { 17L, "系统", "system-menu-manage:delete", "删除系统菜单", "删除系统菜单" },
                    { 20L, "项目", "projects:view", "查看项目管理列表", "查看项目列表" },
                    { 21L, "项目", "projects:create", "创建新项目", "创建项目" },
                    { 22L, "项目", "projects:update", "编辑项目信息", "编辑项目" },
                    { 23L, "项目", "projects:delete", "删除项目", "删除项目" },
                    { 24L, "项目", "projects:manage-item", "管理项目菜单项", "管理菜单项" },
                    { 25L, "项目", "project-access:view", "查看项目成员列表", "查看项目成员" },
                    { 26L, "项目", "project-access:grant", "授权用户项目访问", "授权项目访问" },
                    { 27L, "项目", "project-access:revoke", "撤销用户项目访问", "撤销项目访问" },
                    { 28L, "项目", "projects:view-all", "查看所有项目（不论是否是项目成员）", "查看全部项目" }
                });

            migrationBuilder.InsertData(
                schema: "protohub",
                table: "t_role",
                columns: new[] { "id", "code", "create_time", "description", "is_system", "name", "update_time" },
                values: new object[,]
                {
                    { 1L, "admin", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "拥有系统所有权限", true, "系统管理员", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2L, "guest", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "只读权限", true, "访客", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                schema: "protohub",
                table: "t_role_permission",
                columns: new[] { "permission_id", "role_id", "create_time" },
                values: new object[,]
                {
                    { 1L, 1L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2L, 1L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3L, 1L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4L, 1L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5L, 1L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6L, 1L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7L, 1L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 8L, 1L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 9L, 1L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10L, 1L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 11L, 1L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 12L, 1L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 13L, 1L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 14L, 1L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 15L, 1L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 16L, 1L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 17L, 1L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 20L, 1L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 21L, 1L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 22L, 1L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 23L, 1L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 24L, 1L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 25L, 1L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 26L, 1L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 27L, 1L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 28L, 1L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 1L, 2L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                schema: "protohub",
                table: "t_role_system_menu",
                columns: new[] { "role_id", "system_menu_id", "create_time" },
                values: new object[,]
                {
                    { 1L, 1L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 1L, 2L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 1L, 3L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 1L, 4L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 1L, 5L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 1L, 10L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 1L, 11L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 1L, 12L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                schema: "protohub",
                table: "t_system_menu",
                columns: new[] { "id", "code", "create_time", "icon", "is_visible", "name", "order", "parent_id", "path", "permission", "update_time" },
                values: new object[,]
                {
                    { 1L, "system", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "setting", true, "系统管理", 100, null, "/system", null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2L, "user-manage", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "user", true, "用户管理", 1, 1L, "/system/user", "user-manage:view", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3L, "role-manage", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "team", true, "角色管理", 2, 1L, "/system/role", "role-manage:view", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4L, "permission-manage", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "lock", true, "权限管理", 3, 1L, "/system/permission", "permission-manage:view", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5L, "system-menu-manage", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "menu", true, "系统菜单", 4, 1L, "/system/menu", "system-menu-manage:view", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10L, "project", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "project", true, "项目管理", 200, null, "/project", null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 11L, "project-list", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "appstore", true, "项目列表", 1, 10L, "/project/list", "projects:view", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 12L, "project-access", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "team", true, "项目成员", 2, 10L, "/project/access", "project-access:view", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                schema: "protohub",
                table: "t_user",
                columns: new[] { "id", "create_time", "description", "email", "nick_name", "password", "status", "update_time", "user_name" },
                values: new object[] { 1L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "系统默认管理员", "admin@protohub.com", "管理员", "$2a$12$7ag2WeNZaLTotEEw/kyDF.4Qw1rPXXDtvYtzZlenMN0i49L5gKSAu", 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin" });

            migrationBuilder.InsertData(
                schema: "protohub",
                table: "t_user_role",
                columns: new[] { "role_id", "user_id", "create_time" },
                values: new object[] { 1L, 1L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.CreateIndex(
                name: "idx_menu_group_order",
                schema: "protohub",
                table: "t_menu_group",
                column: "order");

            migrationBuilder.CreateIndex(
                name: "idx_menu_item_group_id",
                schema: "protohub",
                table: "t_menu_item",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "idx_menu_item_group_order",
                schema: "protohub",
                table: "t_menu_item",
                columns: new[] { "group_id", "order" });

            migrationBuilder.CreateIndex(
                name: "idx_permission_category",
                schema: "protohub",
                table: "t_permission",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "idx_permission_code",
                schema: "protohub",
                table: "t_permission",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_role_code",
                schema: "protohub",
                table: "t_role",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_role_permission_permission_id",
                schema: "protohub",
                table: "t_role_permission",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "idx_role_system_menu_menu_id",
                schema: "protohub",
                table: "t_role_system_menu",
                column: "system_menu_id");

            migrationBuilder.CreateIndex(
                name: "idx_system_menu_code",
                schema: "protohub",
                table: "t_system_menu",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_system_menu_parent_id",
                schema: "protohub",
                table: "t_system_menu",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "idx_user_email",
                schema: "protohub",
                table: "t_user",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "idx_user_name",
                schema: "protohub",
                table: "t_user",
                column: "user_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_user_project_access_project_id",
                schema: "protohub",
                table: "t_user_project_access",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "idx_user_project_access_unique",
                schema: "protohub",
                table: "t_user_project_access",
                columns: new[] { "user_id", "project_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_user_project_access_user_id",
                schema: "protohub",
                table: "t_user_project_access",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_user_role_role_id",
                schema: "protohub",
                table: "t_user_role",
                column: "role_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_menu_group",
                schema: "protohub");

            migrationBuilder.DropTable(
                name: "t_menu_item",
                schema: "protohub");

            migrationBuilder.DropTable(
                name: "t_permission",
                schema: "protohub");

            migrationBuilder.DropTable(
                name: "t_role",
                schema: "protohub");

            migrationBuilder.DropTable(
                name: "t_role_permission",
                schema: "protohub");

            migrationBuilder.DropTable(
                name: "t_role_system_menu",
                schema: "protohub");

            migrationBuilder.DropTable(
                name: "t_system_menu",
                schema: "protohub");

            migrationBuilder.DropTable(
                name: "t_user",
                schema: "protohub");

            migrationBuilder.DropTable(
                name: "t_user_project_access",
                schema: "protohub");

            migrationBuilder.DropTable(
                name: "t_user_role",
                schema: "protohub");
        }
    }
}
