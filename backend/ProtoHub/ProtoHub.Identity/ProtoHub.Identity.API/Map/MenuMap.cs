using System.ComponentModel.DataAnnotations;
using ProtoHub.Identity.Database.Models;

namespace ProtoHub.Identity.API.Map;

#region 菜单配置响应

/// <summary>
/// 菜单配置响应 DTO
/// </summary>
public class MenuConfigResponse
{
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = "项目管理聚合基座";

    /// <summary>
    /// 版本
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// 菜单分组列表
    /// </summary>
    public List<MenuGroupResponse> Groups { get; set; } = [];
}

/// <summary>
/// 菜单分组响应 DTO
/// </summary>
public class MenuGroupResponse
{
    /// <summary>
    /// 分组 ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 分组名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 图标
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// 菜单项列表
    /// </summary>
    public List<MenuItemResponse> Children { get; set; } = [];
}

/// <summary>
/// 菜单项响应 DTO
/// </summary>
public class MenuItemResponse
{
    /// <summary>
    /// 菜单项 ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 图标
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 类型
    /// </summary>
    public string Type { get; set; } = null!;

    /// <summary>
    /// 链接地址
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// 视口配置
    /// </summary>
    public ViewportConfig? Viewport { get; set; }

    /// <summary>
    /// 文档文件 ID
    /// </summary>
    public string? DocFileId { get; set; }

    /// <summary>
    /// 文档文件名
    /// </summary>
    public string? DocFileName { get; set; }

    /// <summary>
    /// 文档描述
    /// </summary>
    public string? DocDescription { get; set; }

    /// <summary>
    /// 内部路由
    /// </summary>
    public string? Route { get; set; }

    /// <summary>
    /// 所需权限
    /// </summary>
    public string? Permission { get; set; }
}

#endregion

#region 分组管理

/// <summary>
/// 分组列表响应 DTO（管理端）
/// </summary>
public class GroupListResponse
{
    /// <summary>
    /// 分组 ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 分组名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 图标
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// 菜单项数量
    /// </summary>
    public int ItemCount { get; set; }
}

/// <summary>
/// 创建分组请求 DTO
/// </summary>
public class CreateGroupRequest
{
    /// <summary>
    /// 分组名称
    /// </summary>
    [Required(ErrorMessage = "分组名称不能为空")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 图标
    /// </summary>
    public string? Icon { get; set; }
}

/// <summary>
/// 更新分组请求 DTO
/// </summary>
public class UpdateGroupRequest
{
    /// <summary>
    /// 分组 ID
    /// </summary>
    [Required(ErrorMessage = "分组ID不能为空")]
    public long Id { get; set; }

    /// <summary>
    /// 分组名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    public string? Icon { get; set; }
}

/// <summary>
/// 排序请求 DTO
/// </summary>
public class ReorderRequest
{
    /// <summary>
    /// 排序数据
    /// </summary>
    [Required(ErrorMessage = "排序数据不能为空")]
    public List<OrderItem> Orders { get; set; } = [];
}

/// <summary>
/// 排序项 DTO
/// </summary>
public class OrderItem
{
    /// <summary>
    /// ID
    /// </summary>
    [Required(ErrorMessage = "ID不能为空")]
    public long Id { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    [Required(ErrorMessage = "排序号不能为空")]
    public int Order { get; set; }
}

#endregion

#region 菜单项管理

/// <summary>
/// 菜单项列表请求 DTO
/// </summary>
public class ItemListRequest
{
    /// <summary>
    /// 分组 ID
    /// </summary>
    [Required(ErrorMessage = "分组ID不能为空")]
    public long GroupId { get; set; }
}

/// <summary>
/// 创建菜单项请求 DTO
/// </summary>
public class CreateItemRequest
{
    /// <summary>
    /// 所属分组 ID
    /// </summary>
    [Required(ErrorMessage = "分组ID不能为空")]
    public long GroupId { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [Required(ErrorMessage = "名称不能为空")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 图标
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 类型
    /// </summary>
    [Required(ErrorMessage = "类型不能为空")]
    public string Type { get; set; } = null!;

    /// <summary>
    /// 链接地址
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 视口配置
    /// </summary>
    public ViewportConfig? Viewport { get; set; }

    /// <summary>
    /// 文档文件 ID
    /// </summary>
    public string? DocFileId { get; set; }

    /// <summary>
    /// 文档文件名
    /// </summary>
    public string? DocFileName { get; set; }

    /// <summary>
    /// 文档描述
    /// </summary>
    public string? DocDescription { get; set; }

    /// <summary>
    /// 内部路由
    /// </summary>
    public string? Route { get; set; }

    /// <summary>
    /// 所需权限
    /// </summary>
    public string? Permission { get; set; }
}

/// <summary>
/// 更新菜单项请求 DTO
/// </summary>
public class UpdateItemRequest
{
    /// <summary>
    /// 菜单项 ID
    /// </summary>
    [Required(ErrorMessage = "菜单项ID不能为空")]
    public long Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 类型
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// 链接地址
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 视口配置
    /// </summary>
    public ViewportConfig? Viewport { get; set; }

    /// <summary>
    /// 文档文件 ID
    /// </summary>
    public string? DocFileId { get; set; }

    /// <summary>
    /// 文档文件名
    /// </summary>
    public string? DocFileName { get; set; }

    /// <summary>
    /// 文档描述
    /// </summary>
    public string? DocDescription { get; set; }

    /// <summary>
    /// 内部路由
    /// </summary>
    public string? Route { get; set; }

    /// <summary>
    /// 所需权限
    /// </summary>
    public string? Permission { get; set; }
}

/// <summary>
/// 菜单项排序请求 DTO
/// </summary>
public class ReorderItemsRequest
{
    /// <summary>
    /// 分组 ID
    /// </summary>
    [Required(ErrorMessage = "分组ID不能为空")]
    public long GroupId { get; set; }

    /// <summary>
    /// 排序数据
    /// </summary>
    [Required(ErrorMessage = "排序数据不能为空")]
    public List<OrderItem> Orders { get; set; } = [];
}

/// <summary>
/// 菜单项详情响应 DTO（管理端）
/// </summary>
public class MenuItemDetailResponse
{
    /// <summary>
    /// 菜单项 ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 所属分组 ID
    /// </summary>
    public long GroupId { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 图标
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 类型
    /// </summary>
    public string Type { get; set; } = null!;

    /// <summary>
    /// 链接地址
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// 视口配置
    /// </summary>
    public ViewportConfig? Viewport { get; set; }

    /// <summary>
    /// 文档文件 ID
    /// </summary>
    public string? DocFileId { get; set; }

    /// <summary>
    /// 文档文件名
    /// </summary>
    public string? DocFileName { get; set; }

    /// <summary>
    /// 文档描述
    /// </summary>
    public string? DocDescription { get; set; }

    /// <summary>
    /// 内部路由
    /// </summary>
    public string? Route { get; set; }

    /// <summary>
    /// 所需权限
    /// </summary>
    public string? Permission { get; set; }
}

#endregion
