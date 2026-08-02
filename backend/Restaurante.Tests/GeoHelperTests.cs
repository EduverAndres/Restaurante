using Restaurante.Application.Helpers;

namespace Restaurante.Tests;

public class GeoHelperTests
{
    [Fact]
    public void DistanceKm_SamePoint_ReturnsZero()
    {
        Assert.Equal(0, GeoHelper.DistanceKm(19.4326, -99.1332, 19.4326, -99.1332), 4);
    }

    [Fact]
    public void DistanceKm_OneDegreeOfLatitude_IsAbout111Km()
    {
        // 0,0 -> 0,1: ~111.19 km on the equator.
        Assert.Equal(111.19, GeoHelper.DistanceKm(0, 0, 0, 1), 0.5);
    }

    [Fact]
    public void DistanceKm_NewYorkToLosAngeles_IsAbout3935Km()
    {
        var km = GeoHelper.DistanceKm(40.7128, -74.0060, 34.0522, -118.2437);
        Assert.InRange(km, 3875, 3995);
    }

    [Fact]
    public void DistanceKm_LondonToParis_IsAbout343Km()
    {
        var km = GeoHelper.DistanceKm(51.5074, -0.1278, 48.8566, 2.3522);
        Assert.InRange(km, 330, 356);
    }

    [Fact]
    public void DistanceKm_IsSymmetric()
    {
        var a = GeoHelper.DistanceKm(19.4326, -99.1332, 40.7128, -74.0060);
        var b = GeoHelper.DistanceKm(40.7128, -74.0060, 19.4326, -99.1332);
        Assert.Equal(a, b, 6);
    }

    [Fact]
    public void DistanceKm_OrdersRidersByProximity()
    {
        var restaurantLat = 19.4326;
        var restaurantLon = -99.1332;

        var near = GeoHelper.DistanceKm(restaurantLat, restaurantLon, 19.4330, -99.1335);
        var far = GeoHelper.DistanceKm(restaurantLat, restaurantLon, 20.7000, -103.3500); // ~Guadalajara

        Assert.True(near < far);
        Assert.InRange(near, 0.03, 0.10);
        Assert.InRange(far, 450, 475);
    }
}
