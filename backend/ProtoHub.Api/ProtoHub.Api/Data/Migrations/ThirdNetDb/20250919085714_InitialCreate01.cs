using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ThirdNet.IdentityService.Template.Data.Migrations.ThirdNetDb
{
    /// <inheritdoc />
    public partial class InitialCreate01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "t_actionlist",
                columns: table => new
                {
                    domain = table.Column<string>(type: "text", unicode: false, nullable: false, comment: "服务程序名"),
                    controller = table.Column<string>(type: "text", unicode: false, nullable: false, comment: "控制器名"),
                    action = table.Column<string>(type: "text", unicode: false, nullable: false, comment: "方法名"),
                    path = table.Column<string>(type: "text", unicode: false, nullable: false, comment: "请求路径"),
                    methord = table.Column<string>(type: "text", unicode: false, nullable: false, comment: "http方法"),
                    remark = table.Column<string>(type: "text", nullable: true, comment: "备注信息")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_actionlist", x => new { x.domain, x.controller, x.action });
                },
                comment: "服务方法表");

            migrationBuilder.CreateTable(
                name: "t_application_updatelist",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "自增编号")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    application = table.Column<string>(type: "text", nullable: false, comment: "应用标识"),
                    version = table.Column<string>(type: "text", nullable: true, comment: "版本号"),
                    message = table.Column<string>(type: "text", nullable: false, comment: "更新内容"),
                    number = table.Column<int>(type: "integer", nullable: false, comment: "更新序号"),
                    time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "添加时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_application_updatelist", x => x.id);
                },
                comment: "应用更新日志列表");

            migrationBuilder.CreateTable(
                name: "t_applicationinfo",
                columns: table => new
                {
                    application = table.Column<string>(type: "text", unicode: false, nullable: false, comment: "应用标识"),
                    key = table.Column<string>(type: "text", unicode: false, nullable: false, comment: "加密密钥"),
                    iv = table.Column<string>(type: "text", unicode: false, nullable: false, comment: "加密iv"),
                    url = table.Column<string>(type: "text", unicode: false, nullable: true, comment: "应用地址"),
                    version = table.Column<string>(type: "text", unicode: false, nullable: false, comment: "当前版本号"),
                    prekey = table.Column<string>(type: "text", unicode: false, nullable: false, comment: "加密前缀"),
                    minversion = table.Column<string>(type: "text", unicode: false, nullable: true, comment: "最小版本号"),
                    duration = table.Column<int>(type: "integer", nullable: false, comment: "有效期"),
                    remark = table.Column<string>(type: "text", nullable: true, comment: "备注信息"),
                    time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "添加时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_applicationinfo", x => x.application);
                },
                comment: "应用配置表");

            migrationBuilder.CreateTable(
                name: "t_background_job_log",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "编号")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    addtime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "任务执行时间"),
                    name = table.Column<string>(type: "text", nullable: false, comment: "任务名"),
                    state = table.Column<int>(type: "integer", nullable: false, comment: "执行状态：1：成功，9：失败。"),
                    service = table.Column<string>(type: "text", nullable: false, comment: "当前服务名"),
                    remark = table.Column<string>(type: "text", nullable: true, comment: "备注")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_background_job_log", x => x.id);
                },
                comment: "后台任务日志");

            migrationBuilder.CreateTable(
                name: "t_ipblacklist",
                columns: table => new
                {
                    ip = table.Column<string>(type: "text", nullable: false, comment: "黑名单ip"),
                    duration = table.Column<int>(type: "integer", nullable: false, comment: "黑名单持续时间，单位小时。"),
                    time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "开始时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_ipblacklist", x => x.ip);
                },
                comment: "ip黑名单");

            migrationBuilder.CreateTable(
                name: "t_rolesinfo",
                columns: table => new
                {
                    rolename = table.Column<string>(type: "text", unicode: false, nullable: false, comment: "角色名"),
                    remark = table.Column<string>(type: "text", nullable: true, comment: "备注信息")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_rolesinfo", x => x.rolename);
                },
                comment: "角色表");

            migrationBuilder.CreateTable(
                name: "t_visitlog",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "自增编号")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    url = table.Column<string>(type: "text", unicode: false, nullable: false, comment: "请求url"),
                    ip = table.Column<string>(type: "text", unicode: false, nullable: false, comment: "请求ip"),
                    controller = table.Column<string>(type: "text", unicode: false, nullable: true, comment: "控制器名"),
                    domain = table.Column<string>(type: "text", unicode: false, nullable: true, comment: "服务程序名"),
                    action = table.Column<string>(type: "text", unicode: false, nullable: true, comment: "方法名"),
                    application = table.Column<string>(type: "text", unicode: false, nullable: true, comment: "应用标识"),
                    username = table.Column<string>(type: "text", unicode: false, nullable: true, comment: "用户名"),
                    auth = table.Column<string>(type: "text", unicode: false, nullable: true, comment: "认证头信息"),
                    useragent = table.Column<string>(type: "text", nullable: true, comment: "ua信息"),
                    statuscode = table.Column<int>(type: "integer", nullable: false, comment: "状态码"),
                    elapsed = table.Column<int>(type: "integer", nullable: false, defaultValue: -1, comment: "请求耗时"),
                    time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_visitlog", x => x.id);
                },
                comment: "访问日志表");

            migrationBuilder.CreateTable(
                name: "t_visitloghistory",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "自增编号")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    url = table.Column<string>(type: "text", unicode: false, nullable: false, comment: "请求url"),
                    ip = table.Column<string>(type: "text", unicode: false, nullable: false, comment: "请求ip"),
                    controller = table.Column<string>(type: "text", unicode: false, nullable: true, comment: "控制器名"),
                    domain = table.Column<string>(type: "text", unicode: false, nullable: true, comment: "服务程序名"),
                    action = table.Column<string>(type: "text", unicode: false, nullable: true, comment: "方法名"),
                    application = table.Column<string>(type: "text", unicode: false, nullable: true, comment: "应用标识"),
                    username = table.Column<string>(type: "text", unicode: false, nullable: true, comment: "用户名"),
                    auth = table.Column<string>(type: "text", unicode: false, nullable: true, comment: "认证头信息"),
                    useragent = table.Column<string>(type: "text", nullable: true, comment: "ua信息"),
                    statuscode = table.Column<int>(type: "integer", nullable: false, comment: "状态码"),
                    elapsed = table.Column<int>(type: "integer", nullable: false, defaultValue: -1, comment: "请求耗时"),
                    time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_visitloghistory", x => x.id);
                },
                comment: "访问日志历史表");

            migrationBuilder.CreateTable(
                name: "t_ipwhitelist",
                columns: table => new
                {
                    ip = table.Column<string>(type: "text", unicode: false, nullable: false, comment: "ip地址"),
                    application = table.Column<string>(type: "text", unicode: false, nullable: false, comment: "应用标识"),
                    remark = table.Column<string>(type: "text", nullable: true, comment: "备注信息"),
                    time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "添加时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_ipwhitelist", x => new { x.application, x.ip });
                    table.ForeignKey(
                        name: "fk_application_whitelist",
                        column: x => x.application,
                        principalTable: "t_applicationinfo",
                        principalColumn: "application",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "应用白名单");

            migrationBuilder.CreateTable(
                name: "t_applicationauthority",
                columns: table => new
                {
                    rolename = table.Column<string>(type: "text", nullable: false, comment: "角色名"),
                    application = table.Column<string>(type: "text", unicode: false, nullable: false, comment: "应用名")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_applicationauthority", x => new { x.rolename, x.application });
                    table.ForeignKey(
                        name: "fk_application_role",
                        column: x => x.application,
                        principalTable: "t_applicationinfo",
                        principalColumn: "application",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_role_authority",
                        column: x => x.rolename,
                        principalTable: "t_rolesinfo",
                        principalColumn: "rolename",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "应用对应角色表");

            migrationBuilder.CreateTable(
                name: "t_roles_authority",
                columns: table => new
                {
                    rolename = table.Column<string>(type: "text", nullable: false, comment: "角色名"),
                    domain = table.Column<string>(type: "text", unicode: false, nullable: false, comment: "服务程序名"),
                    controller = table.Column<string>(type: "text", unicode: false, nullable: false, comment: "控制器名"),
                    action = table.Column<string>(type: "text", unicode: false, nullable: false, comment: "方法名"),
                    remark = table.Column<string>(type: "text", nullable: true, comment: "备注信息")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_roles_authority", x => new { x.rolename, x.domain, x.controller, x.action });
                    table.ForeignKey(
                        name: "fk_rolesauthority_name",
                        column: x => x.rolename,
                        principalTable: "t_rolesinfo",
                        principalColumn: "rolename",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "角色权限表");

            migrationBuilder.CreateIndex(
                name: "IX_t_applicationauthority_application",
                table: "t_applicationauthority",
                column: "application");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_actionlist");

            migrationBuilder.DropTable(
                name: "t_application_updatelist");

            migrationBuilder.DropTable(
                name: "t_applicationauthority");

            migrationBuilder.DropTable(
                name: "t_background_job_log");

            migrationBuilder.DropTable(
                name: "t_ipblacklist");

            migrationBuilder.DropTable(
                name: "t_ipwhitelist");

            migrationBuilder.DropTable(
                name: "t_roles_authority");

            migrationBuilder.DropTable(
                name: "t_visitlog");

            migrationBuilder.DropTable(
                name: "t_visitloghistory");

            migrationBuilder.DropTable(
                name: "t_applicationinfo");

            migrationBuilder.DropTable(
                name: "t_rolesinfo");
        }
    }
}
