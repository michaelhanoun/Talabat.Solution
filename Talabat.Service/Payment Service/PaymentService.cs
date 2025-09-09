using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Stripe;
using Talabat.Core;
using Talabat.Core.Entities.Basket;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.Core.Entities.Product;
using Talabat.Core.Repositories.Contract;
using Talabat.Core.Services.Contract;
using Talabat.Core.Specifications;
using Product = Talabat.Core.Entities.Product.Product;

namespace Talabat.Application.Payment_Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly IBasketRepository _basketRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PaymentService(IConfiguration configuration,IBasketRepository basketRepository,IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _basketRepository = basketRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<CustomerBasket?> CreateOrUpdatePaymentIntent(string basketId)
        {
            StripeConfiguration.ApiKey = _configuration["StripeSettings:SecretKey"];
            var basket = await _basketRepository.GetBasketAsync(basketId);
            if (basket is null) return null;
            var shippingPrice = 0m;
            if(basket.DeliveryMethodId.HasValue)
            {
                var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(basket.DeliveryMethodId.Value);
                shippingPrice = deliveryMethod.Cost;
                basket.ShippingPrice = shippingPrice;

            }
            if (basket.Items?.Count() > 0) {
                var productRepo = _unitOfWork.Repository<Product>();
                foreach (var item in basket.Items) {
                    var product = await productRepo.GetByIdAsync(item.Id);
                    if(item.Price != product.Price)
                    item.Price = product.Price;
                }   
            }
            PaymentIntent paymentIntent ;
            PaymentIntentService paymentIntentService = new PaymentIntentService();
            if (string.IsNullOrEmpty( basket.PaymentIntentId))
            {
                var options = new PaymentIntentCreateOptions()
                {
                Amount = (long) (basket.Items.Sum(item=>item.Quantity*item.Price)+shippingPrice)*100,
                Currency = "usd",
                PaymentMethodTypes = new List<string>() {"card"}
                };
                paymentIntent = await paymentIntentService.CreateAsync(options);
                basket.PaymentIntentId = paymentIntent.Id;

                basket.ClientSecret = paymentIntent.ClientSecret;
            }
            else
            {
                var options = new PaymentIntentUpdateOptions()
                {
                    Amount = (long)(basket.Items.Sum(item => item.Quantity * item.Price) + shippingPrice) * 100,
                };
               paymentIntent = await paymentIntentService.UpdateAsync(basket.PaymentIntentId,options); 
            }
            await _basketRepository.UpdateBasketAsync(basket);
            return basket;
        }

        public async Task<Order> UpdateOrderStatus(string paymentIntentId, bool isPaid)
        {
            var orderRepo = _unitOfWork.Repository<Order>();
            var order = await orderRepo.GetWithSpecAsync(new OrderWithPaymentIntentSpecifications(paymentIntentId));
            if (order is null) return null;
            order.Status = isPaid ? OrderStatus.PaymentReceived : OrderStatus.PaymentFailed;
            orderRepo.Update(order);
            await _unitOfWork.CompleteAsync();
            return order;
        }
    }
}
