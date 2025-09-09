using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.Core.Entities.Product;
using Talabat.Core.Repositories.Contract;
using Talabat.Core.Services.Contract;
using Talabat.Core.Specifications;

namespace Talabat.Application.Order_Service
{
    public class OrderService : IOrderService
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentService _paymentService;

        public OrderService(IBasketRepository basketRepository, IUnitOfWork unitOfWork,IPaymentService paymentService)
        {
            _basketRepository = basketRepository;
            _unitOfWork = unitOfWork;
            _paymentService = paymentService;
        }
        public async Task<Order?> CreateOrderAsync(string buyerEmail, string basketId, int deliveryMethodId, Address shippingAddress)
        {
            var basket = await _basketRepository.GetBasketAsync(basketId);
            var orderItems = new List<OrderItem>();
            if (basket?.Items?.Count > 0) {
                 var productRepo = _unitOfWork.Repository<Product>();
                foreach (var item in basket.Items)
                {
                    var product = await productRepo.GetByIdAsync(item.Id);
                    var productItemOrder = new ProductItemOrdered(product.Id, product.Name, product.PictureUrl);
                    var orderItem = new OrderItem(productItemOrder,product.Price,item.Quantity);
                    orderItems.Add(orderItem);
                }  
            }
            var subTotal = orderItems.Sum(OI=>OI.Price*OI.Quantity);
            var delivery = await _unitOfWork.Repository<DeliveryMethod>().GetByIdWithTrackingAsync(deliveryMethodId);
            var orderRepo =  _unitOfWork.Repository<Order>();
            var existingOrder = await orderRepo.GetWithSpecAsync (new OrderWithPaymentIntentSpecifications(basket?.PaymentIntentId??"")) ;
            if (existingOrder is not null)
            { orderRepo.Delete(existingOrder);
                await _paymentService.CreateOrUpdatePaymentIntent(basketId);
            }
            var order = new Order(buyerEmail,shippingAddress,delivery.Id,orderItems, subTotal, basket?.PaymentIntentId??"");
           await orderRepo.Add(order);
            var result = await _unitOfWork.CompleteAsync();
            return result<=0? null: order;

        }

        public async Task<IReadOnlyList<DeliveryMethod>> GetDeliveryMethodsAsync() => await _unitOfWork.Repository<DeliveryMethod>().GetAllAsync();
         
        public async Task<Order?> GetOrderByIdForUserAsync(string buyerEmail, int orderId)
        {
            var orderRepo = _unitOfWork.Repository<Order>();
            var spec = new OrderSpecifications(buyerEmail,orderId);
            var orders = await orderRepo.GetWithSpecAsync(spec);
            return orders;
        }

        public async Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string buyerEmail)
        {
            var orderRepo = _unitOfWork.Repository<Order>();
            var spec = new OrderSpecifications(buyerEmail);
            var orders = await orderRepo.GetAllWithSpecAsync(spec);
            return orders;
        }
    }
}
