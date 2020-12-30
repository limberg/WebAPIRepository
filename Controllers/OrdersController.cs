using AutoMapper;
using Entities;
using Entities.Contracts;
using Entities.DTOs;
using Entities.Models;
using LoggerService.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPIRepository.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IRepositoryWrapper repository;
        private readonly ILoggerManager logger;
        private readonly IMapper mapper;

        public OrdersController(IRepositoryWrapper repository, ILoggerManager logger, IMapper mapper)
        {
            this.repository = repository;
            this.logger = logger;
            this.mapper = mapper;
        }

        [HttpGet ("GetOrders")]
        public IActionResult GetOrders([FromQuery] OrderParameters orderParameters)
        {
            try
            {
                logger.LogInfo("GetOrders");
                var orders =  repository.Orders.GetOrders(orderParameters);
                var ordersResult = mapper.Map<IEnumerable<OrderDto>>(orders);
                return Ok(ordersResult);
            }
            catch (Exception ex)
            {
                logger.LogError($"GetOrders(): {ex.Message} {ex.StackTrace}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error");
            }
        }

        [HttpGet("GetOrdersPagedList")]
        public IActionResult GetOrdersPagedList([FromQuery]OrderParameters orderParameters)
        {
            try
            {
                var ordersPaged = repository.Orders.GetOrdersPagedList(orderParameters);

                var metadata = new
                {
                    ordersPaged.CurrentPage,
                    ordersPaged.TotalCount,
                    ordersPaged.TotalPages,
                    ordersPaged.PageSize,
                    ordersPaged.HasPrevios,
                    ordersPaged.HastNext
                };

                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));

                logger.LogInfo($"GetOrdersPagedList: Returned {ordersPaged.TotalCount} orders from DB");

                return Ok(ordersPaged);

            }
            catch (Exception ex)
            {
                logger.LogError($"GetOrdersPagedList(): {ex.Message} {ex.StackTrace}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                logger.LogInfo("GetAllOrders");
                var orders = await repository.Orders.GetAllOrdersAsync();
                var ordersResult = mapper.Map<IEnumerable<OrderDto>>(orders);
                return Ok(ordersResult);
            }
            catch(Exception ex)
            {
                logger.LogError($": {ex.Message} {ex.StackTrace}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderByIDAsync(int id)
        {
            try
            {
                var order = await repository.Orders.GetOrderByIDAsync(id);
                if (order == null)
                {
                    logger.LogError($"Order with id: {id} not found");
                    return NotFound();
                }
            
                var orderDto = mapper.Map<OrderDto>(order);
                logger.LogInfo($"Order returned:{orderDto.ShipName}");
                return Ok(orderDto);
            }
            catch(Exception ex)
            {
                logger.LogError($": {ex.Message} {ex.StackTrace} {ex.InnerException}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error");
            }
        }

        [HttpGet("GetOrderWithDetails/{id}")]
        public async Task<IActionResult> GetOrderWithDetails(int id)
        {
            try
            {
                var order = await repository.Orders.GetOrderWithDetailsAsync(id);
                if (order == null)
                {
                    logger.LogError($"Order with Details ID: {id} not found");
                    return NotFound();
                }

                var orderDto = mapper.Map<OrderDto>(order);
                logger.LogInfo($"Order returned:{orderDto.ShipName}");
                return Ok(orderDto);
            }
            catch (Exception ex)
            {
                logger.LogError($": {ex.Message} {ex.StackTrace} {ex.InnerException}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderForCreationDto orderCreationDto)
        {
            try
            {

                if (orderCreationDto == null)
                {
                    logger.LogError("Order Creation DTO is null.");
                    return BadRequest("Order gesendet is null");
                }

                if (!ModelState.IsValid)
                {
                    logger.LogError("Invalid Order Object sent from client.");
                    return BadRequest("Invalid Order Object sent.");
                }

                var order = mapper.Map<Order>(orderCreationDto);

                repository.Orders.CreateOrder(order);

                await repository.SaveAsync();

                var createdOrder = mapper.Map<OrderDto>(order);

                return CreatedAtAction(nameof(GetOrderByIDAsync), new { id = createdOrder.OrderID }, createdOrder);

            }
            catch(Exception ex)
            {
                logger.LogError($"CreateOrder(): {ex.Message} {ex.StackTrace} {ex.InnerException}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error");
            }

        }


        [HttpPut ("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] OrderForUpdateDto orderForUpdateDto)
        {
            try
            {
                if(orderForUpdateDto == null)
                {
                    logger.LogError("Order object sent from client is null.");
                    return BadRequest("Order for Update is Null.");
                }

                if (!ModelState.IsValid)
                {
                    logger.LogError("Invalid order object sent from client.");
                    return BadRequest("Invalid Order to Update.");
                }

                var orderToUpdate = await repository.Orders.GetOrderByIDAsync(id);

                if(orderToUpdate == null)
                {
                    logger.LogError($"Order with id: {id}, hasn't been found in db");
                    return NotFound();
                }

                mapper.Map(orderForUpdateDto, orderToUpdate);

                repository.Orders.UpdateOrder(orderToUpdate);

                await repository.SaveAsync();

                return NoContent();

            }
            catch(Exception ex)
            {
                logger.LogError($"UpdateOrder(): {ex.Message} {ex.StackTrace} {ex.InnerException}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                var order = await repository.Orders.GetOrderByIDAsync(id);
                if (order == null)
                {
                    logger.LogError($"Order with id: {id}, hasn't been found in db.");
                    return NotFound();
                }

                if (repository.OrderDetails.GetOrderDetailsByOrderID(id).Any())
                {
                    logger.LogError($"Cannot delete order with id: {id}. It has related order details. Delete those order details first");
                    return BadRequest("Cannot delete order. It has related order details. Delete those order details first");
                }

                repository.Orders.DeleteOrder(order);
                await repository.SaveAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError($"Something went wrong inside DeleteOrder action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
