using EmailService;
using FGMEmailSenderApp.Helpers;
using FGMEmailSenderApp.Models.EntityFrameworkModels;
using FGMEmailSenderApp.Models.InputModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace FGMEmailSenderApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        RequestController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        #region GET ALL REQUESTS

        [Authorize(Roles = RoleHelper.FGMEmployeeInternalRole)]
        [HttpGet]
        [Route("GetAllRequests")]
        public async Task<ActionResult<ICollection<Request>>> GetAllRequests()
        {
            return await _context.Requests.ToListAsync();
        }

        #endregion

        #region GET USER REQUESTS

        [Authorize(Roles = RoleHelper.FGMEmployeeInternalRole)]
        [HttpGet]
        [Route("GetUserRequests")]
        public async Task<ActionResult<ICollection<Request>>> GetUserRequests(string emailUser)
        {
            var emailController = new EmailAddressAttribute();

            if(String.IsNullOrEmpty(emailUser)) return NotFound( new { DateTime.Now });

            var user = await _userManager.FindByEmailAsync(emailUser);

            if (user == null) return NotFound( new { DateTime.Now });            

            var requests = _context.Requests.Where(r => r.IdUser == user.Id).ToListAsync();

            return Ok( new { requests, DateTime.Now });
        }

        #endregion

        #region GET DETAILS REQUEST

        [Authorize]
        [HttpPost]
        [Route("GetDetailsRequest")]
        public async Task<ActionResult<Request>> GetDetailsRequest(int idRequest)
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var user = await _userManager.FindByIdAsync(userId);

            var roleUser = await _userManager.GetRolesAsync(user);

            if (roleUser.Contains(RoleHelper.UserRole) || roleUser.Contains(RoleHelper.ReferentRole))
            {
                var requests = _context.Requests.Where(r => r.IdUser == userId && r.IdRequest == idRequest);

                return Ok(new { requests, DateTime.Now });
            }
            else
            {
                var requests = _context.Requests.Where(r => r.IdRequest == idRequest);

                return Ok(new { requests, DateTime.Now });
            }
        }

        #endregion

        #region GET REQUESTS FROM QUERY

        [Authorize(Roles = RoleHelper.FGMEmployeeInternalRole)]
        [HttpGet]
        [Route("GetRequestsFromQuery")]
        public async Task<ActionResult<ICollection<Request>>> GetRequestsFromQuery(string query)
        {
            try
            {
                GetDetailsRequest(Int32.Parse(query));
            }
            catch { }

            GetUserRequests(query);

            return await _context.Requests.Where(c => c.DescriptionRequest.ToLower().Contains(query) || c.Response == Boolean.Parse(query) || c.IdTypesRequest == Int32.Parse(query)).ToListAsync();
        }

        #endregion

        #region CREATE NEW REQUEST

        [Authorize(Roles = RoleHelper.UserRole)]
        [Authorize(Roles = RoleHelper.ReferentRole)]
        [HttpPost]
        [Route("CreateNewRequest")]
        public async Task<IActionResult> CreateNewRequest(RequestInputModel requestInserted)
        {
            if (!ModelState.IsValid) return StatusCode(406, $"The data you inserted is not valid, {DateTime.Now}");

            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var user = await _userManager.FindByIdAsync(userId);

            var newRequest = new Request
            {
                DescriptionRequest = requestInserted.DescriptionRequest,
                IdTypesRequest = requestInserted.IdTypeRequest,
                TypesRequest = new TypeRequest { IdTypeRequest = requestInserted.IdTypeRequest, TypeNameRequest = _context.TypesRequest.Where(t => t.IdTypeRequest == requestInserted.IdTypeRequest).FirstOrDefault().TypeNameRequest.ToString() },
                IdUser = userId,
                User = user,
            };

            await _context.SaveChangesAsync();

            return Ok( new { newRequest, DateTime.Now });
        }

        #endregion

        #region POST RESPONSE FOR REQUEST

        [Authorize(Roles = RoleHelper.FGMEmployeeInternalRole)]
        [HttpPost]
        [Route("PostResponseForRequest")]
        public async Task<IActionResult> PostResponseForRequest(int idRequest, bool response)
        {
            var request = _context.Requests.Where(r => r.IdRequest == idRequest);

            if (request == null) return NotFound(DateTime.Now);

            var user = await _userManager.FindByIdAsync(request.FirstOrDefault().IdUser);

            request.FirstOrDefault().Response = response;

            if (response == true && request.FirstOrDefault().TypesRequest.IdTypeRequest != 3)
            {
                var typeResponse = request.FirstOrDefault().TypesRequest.IdTypeRequest == 1 ? "updateCompany" : "createCompany";

                _emailSender.SendNotificationUserPermission(user.NameUser, user.LastNameUser, user.Email, request.FirstOrDefault().DescriptionRequest, user.Company.CompanyName);   
            }
            else
            {
                _emailSender.SendNotificationResponseRequest(idRequest, request.FirstOrDefault().TypesRequest.ToString(), request.FirstOrDefault().DescriptionRequest.ToString(), response, request.FirstOrDefault().User.Email, request.FirstOrDefault().User.NameUser, request.FirstOrDefault().User.LastNameUser);
            }

            await _context.SaveChangesAsync();

            return Ok(new { request, DateTime.Now });
        }

        #endregion
    }
}
