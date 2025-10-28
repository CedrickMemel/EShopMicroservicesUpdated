using Discount.Grpc.Data;
using Discount.Grpc.Models;
using Grpc.Core;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Services;

public class DiscountService(DiscountContext dbContext, ILogger<DiscountService> logger)
    : DiscountProtoService.DiscountProtoServiceBase
{
    public override async Task<CouponModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
    {
        var coupon = await dbContext.Coupons.FirstOrDefaultAsync(cp => cp.ProductName == request.ProductName)
            ?? new Coupon { ProductName = "No Discount", Amount = 0, Description = "No Discount descrip" };

        logger.LogInformation("Discount is retrieved for ProductName: {ProductName}, Amount : {Amount}", coupon.ProductName, coupon.Amount);

        var couponModel = coupon.Adapt<CouponModel>();
        return couponModel;
    }

    public override async Task<CouponModel> CreateDiscount(CreateDiscountRequest request, ServerCallContext context)
    {
        var coupon = request.Coupon.Adapt<Coupon>() ?? throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid request object"));

        dbContext.Coupons.Add(coupon);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Discount is successfully created for {ProductName}", coupon.ProductName);

        var couponModel = coupon.Adapt<CouponModel>();
        return couponModel;
    }

    public override async Task<CouponModel> UpdateDiscount(UpdateDiscountRequest request, ServerCallContext context)
    {
        var coupon = request.Coupon.Adapt<Coupon>() ?? throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid resuest object"));
        var existingCoupon = await dbContext.Coupons.FindAsync(request.Coupon.Id) ?? throw new RpcException(new Status(StatusCode.NotFound, $"Discount with Id={request.Coupon.Id} is not existing"));
       
        request.Coupon.Adapt(existingCoupon);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Discount is successfully Updates for {ProductName}", coupon.ProductName);
        var couponModel = existingCoupon.Adapt<CouponModel>();

        return couponModel;
    }

    public override async Task<DeleteDiscountResponse> DeleteDiscount(DeleteDiscountRequest request, ServerCallContext context)
    {
        var coupon = await dbContext.Coupons.FirstOrDefaultAsync(cp => cp.ProductName == request.ProductName)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Discount with ProductName={request.ProductName} is not existing"));

        dbContext.Remove(coupon);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Discount is successfully deleted for {ProductName}", coupon.ProductName);

        return new DeleteDiscountResponse { Success = true };
    }
}
