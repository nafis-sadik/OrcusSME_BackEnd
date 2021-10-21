﻿using Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Abstraction;
using System.Collections.Generic;
using DataLayer;

namespace WebApplication.Controllers
{
    [Route("api/Subscription")]
    [ApiController]
    public class SubscriptionController
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpGet]
        [Authorize]
        [Route("Subscribe/{userId}/{subscriptionId}")]
        public IActionResult Subscribe(string userId, int subscriptionId)
        {
            if (_subscriptionService.Subscribe(userId, subscriptionId))
                return new OkObjectResult(new { Response = "" });
            else
                return new ConflictObjectResult(new { Response = CommonConstants.HttpResponseMessages.Exception });
        }

        [HttpGet]
        [Authorize]
        [Route("GetActiveSubscriptions/{userId}")]
        public IActionResult GetActiveSubscriptions(string userId) => new OkObjectResult(new { Response = _subscriptionService.GetActiveSubscriptions(userId) });

        [HttpGet]
        [Authorize]
        [Route("GetSubscriptionHistory/{pageNo}/{pageSize}/{userId}")]
        public IActionResult GetSubscriptionHistory(int pageNo, int? pageSize, string userId)
        {
            if (pageSize == null || pageSize <= 0)
                pageSize = CommonConstants.StandardPageSize;

            IEnumerable<SubscribedService> subHistory = _subscriptionService.GetSubscriptionHistory(new Pagination
            {
                PageNo = pageNo,
                PageSize = (int)pageSize
            }, userId);

            return new OkObjectResult(new { Response = subHistory });
        }

        [HttpGet]
        [Authorize]
        [Route("HasSubscription/{userId}/{subscriptionId}")]
        public IActionResult HasSubscription(string userId, int subscriptionId) => new OkObjectResult(new { Response = _subscriptionService.HasSubscription(userId, subscriptionId) });
    }
}
