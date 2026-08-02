using Restaurante.Domain.Entities;
using Restaurante.Domain.Helpers;

namespace Restaurante.Tests;

public class BusinessHoursHelperTests
{
    private static List<BusinessHour> Hours(int day, TimeSpan open, TimeSpan close, bool isClosed = false) =>
        new() { new BusinessHour { DayOfWeek = day, OpenTime = open, CloseTime = close, IsClosed = isClosed } };

    [Fact]
    public void IsOpenNow_WithNoHours_ReturnsNull()
    {
        Assert.Null(BusinessHoursHelper.IsOpenNow(null, DateTime.UtcNow));
        Assert.Null(BusinessHoursHelper.IsOpenNow(new List<BusinessHour>(), DateTime.UtcNow));
    }

    [Fact]
    public void IsOpenOn_WithNoHours_ReturnsNull()
    {
        Assert.Null(BusinessHoursHelper.IsOpenOn(null, DayOfWeek.Monday, new TimeSpan(12, 0, 0)));
    }

    [Fact]
    public void IsOpenOn_WithNoEntryForDay_ReturnsNull()
    {
        var hours = Hours((int)DayOfWeek.Monday, new TimeSpan(9, 0, 0), new TimeSpan(23, 0, 0));
        Assert.Null(BusinessHoursHelper.IsOpenOn(hours, DayOfWeek.Tuesday, new TimeSpan(12, 0, 0)));
    }

    [Theory]
    [InlineData(9, 0, true)]
    [InlineData(10, 0, true)]
    [InlineData(22, 59, true)]
    [InlineData(8, 59, false)]
    [InlineData(23, 0, false)]
    [InlineData(0, 30, false)]
    public void IsOpenOn_WithinSameDaySchedule(int hour, int minute, bool expected)
    {
        var hours = Hours((int)DayOfWeek.Wednesday, new TimeSpan(9, 0, 0), new TimeSpan(23, 0, 0));
        var time = new TimeSpan(hour, minute, 0);
        Assert.Equal(expected, BusinessHoursHelper.IsOpenOn(hours, DayOfWeek.Wednesday, time));
    }

    [Fact]
    public void IsOpenOn_ClosedDay_ReturnsFalse()
    {
        var hours = Hours((int)DayOfWeek.Sunday, new TimeSpan(9, 0, 0), new TimeSpan(23, 0, 0), isClosed: true);
        Assert.False(BusinessHoursHelper.IsOpenOn(hours, DayOfWeek.Sunday, new TimeSpan(12, 0, 0)));
    }

    [Theory]
    [InlineData(23, 30, true)]
    [InlineData(0, 30, true)]
    [InlineData(1, 59, true)]
    [InlineData(2, 0, false)]
    [InlineData(12, 0, false)]
    [InlineData(21, 59, false)]
    [InlineData(22, 0, true)]
    public void IsOpenOn_OvernightSchedule_CrossesMidnight(int hour, int minute, bool expected)
    {
        var hours = Hours((int)DayOfWeek.Friday, new TimeSpan(22, 0, 0), new TimeSpan(2, 0, 0));
        var time = new TimeSpan(hour, minute, 0);
        Assert.Equal(expected, BusinessHoursHelper.IsOpenOn(hours, DayOfWeek.Friday, time));
    }

    [Fact]
    public void IsOpenNow_DelegatesToIsOpenOn_WithUtcDayOfWeek()
    {
        var now = new DateTime(2026, 8, 1, 12, 0, 0, DateTimeKind.Utc); // Saturday
        var hours = Hours((int)DayOfWeek.Saturday, new TimeSpan(9, 0, 0), new TimeSpan(23, 0, 0));
        Assert.True(BusinessHoursHelper.IsOpenNow(hours, now));
    }
}
