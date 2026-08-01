using Restaurante.Domain.Common;
using Restaurante.Domain.Enums;

namespace Restaurante.Domain.Entities;

public class Coupon : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public Guid? RestaurantId { get; set; }
    public Restaurant? Restaurant { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
    public int? MaxUses { get; set; }
    public int TimesUsed { get; set; } = 0;
    public decimal MinOrderAmount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}
