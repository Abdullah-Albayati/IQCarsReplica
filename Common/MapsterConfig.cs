using Mapster;
using HotelSystemBackend.DTOs;
using HotelSystemBackend.Entities;

namespace HotelSystemBackend.Common;

public static class MapsterConfig
{
    public static void Configure()
    {
        // Add mappings here
       /* TypeAdapterConfig<Hotel, HotelDto>.NewConfig();
        TypeAdapterConfig<HotelForm, Hotel>.NewConfig();
        TypeAdapterConfig<HotelUpdate, Hotel>.NewConfig().IgnoreNullValues(true);
        TypeAdapterConfig<User, UserDto>.NewConfig().IgnoreNullValues(true);
        TypeAdapterConfig<UserForm, User>.NewConfig().IgnoreNullValues(true);
        TypeAdapterConfig<UserUpdate, User>.NewConfig().IgnoreNullValues(true);*/

    }
}
