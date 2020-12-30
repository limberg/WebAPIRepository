using AutoMapper;
using Entities.DTOs;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPIRepository.Profiles
{
    public class OrderProfile: Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, OrderDto>();
            CreateMap<OrderDetail, OrderDetailsDto>();
            CreateMap<Product, ProductDto>();

            CreateMap<OrderForCreationDto, Order>();
            CreateMap<OrderForUpdateDto, Order>();
        }
    }
}
