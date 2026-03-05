using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimeTrack.API.DTOs.Common;
using TimeTrack.API.DTOs.Notification;
using TimeTrack.API.Models;
using TimeTrack.API.Service.ServiceInterface;

namespace TimeTrack.API.Controllers;

// CONTROLLER: NotificationController
// PURPOSE: Handles all notification-related API requests from frontend.
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }


    // API ENDPOINT: POST /api/notification
    // CALLED FROM FRONTEND: createNotification() function
    // PURPOSE: Creates a notification for a user.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<bool>>> CreateNotification([FromBody] CreateNotificationDto dto)
    {
        await _notificationService.CreateNotificationAsync(dto.UserId, dto.Type, dto.Message);
        return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Notification created successfully"));
    }

  
    // API ENDPOINT: GET /api/notification
    // CALLED FROM FRONTEND: getMyNotifications() function
    // PURPOSE: Gets all notifications for the current user.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<Notification>>>> GetMyNotifications()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var notifications = await _notificationService.GetUserNotificationsAsync(userId);
        return Ok(ApiResponseDto<IEnumerable<Notification>>.SuccessResponse(notifications));
    }

    // API ENDPOINT: GET /api/notification/unread
    // CALLED FROM FRONTEND: getUnreadNotifications() function
    // PURPOSE: Gets only unread notifications for the current user.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpGet("unread")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<Notification>>>> GetUnreadNotifications()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var notifications = await _notificationService.GetUnreadNotificationsAsync(userId);
        return Ok(ApiResponseDto<IEnumerable<Notification>>.SuccessResponse(notifications));
    }


    // API ENDPOINT: GET /api/notification/unread/count
    // CALLED FROM FRONTEND: getUnreadCount() function
    // PURPOSE: Gets the count of unread notifications for badge display.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpGet("unread/count")]
    public async Task<ActionResult<ApiResponseDto<int>>> GetUnreadCount()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var count = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(ApiResponseDto<int>.SuccessResponse(count));
    }

 
    // API ENDPOINT: PATCH /api/notification/{notificationId}/read
    // CALLED FROM FRONTEND: markAsRead() function
    // PURPOSE: Marks a specific notification as read.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpPatch("{notificationId:guid}/read")]
    public async Task<ActionResult<ApiResponseDto<bool>>> MarkAsRead(Guid notificationId)
    {
        await _notificationService.MarkAsReadAsync(notificationId);
        return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Notification marked as read"));
    }


    // API ENDPOINT: PATCH /api/notification/read-all
    // CALLED FROM FRONTEND: markAllAsRead() function
    // PURPOSE: Marks all notifications as read for the current user.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpPatch("read-all")]
    public async Task<ActionResult<ApiResponseDto<bool>>> MarkAllAsRead()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _notificationService.MarkAllAsReadAsync(userId);
        return Ok(ApiResponseDto<bool>.SuccessResponse(true, "All notifications marked as read"));
    }
}